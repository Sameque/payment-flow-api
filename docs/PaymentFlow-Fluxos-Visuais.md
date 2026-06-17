# PaymentFlow - Complemento Visual da Documentação

Este documento deve ser anexado à documentação principal e tem como objetivo ilustrar visualmente os fluxos descritos no sistema.

---

# Fluxo Completo do PaymentFlow

```text
┌────────────────────┐
│      CLIENTE       │
└─────────┬──────────┘
          │
          │ POST /payments
          ▼

┌────────────────────┐
│    PAYMENT API     │
└─────────┬──────────┘
          │
          │ Salva Payment
          │ Salva OutboxMessage
          ▼

┌────────────────────┐
│    PostegreSQL     │
│ Payments           │
│ OutboxMessages     │
└─────────┬──────────┘
          │
          ▼

┌────────────────────┐
│ OUTBOX DISPATCHER  │
└─────────┬──────────┘
          │ Publish
          ▼

┌──────────────────────────────────────────┐
│              RABBITMQ                    │
│ Exchange: pf.payment.exchange            │
└─────────┬────────────────────────────────┘
          │
          │ payment.transaction.created
          ▼

┌────────────────────┐
│   FRAUD WORKER     │
└─────────┬──────────┘
          │
          ├───────────────┐
          │               │
          ▼               ▼

payment.fraud.approved    payment.fraud.rejected
          │               │
          │               ▼
          │     ┌───────────────────┐
          │     │ NOTIFICATION      │
          │     │ WORKER            │
          │     └───────────────────┘
          │
          ▼

┌────────────────────┐
│ PROCESSOR WORKER   │
└─────────┬──────────┘
          │
          ├───────────────┐
          │               │
          ▼               ▼

payment.processor.approved
payment.processor.rejected
          │
          ▼

┌───────────────────┐
│ NOTIFICATION      │
│ WORKER            │
└─────────┬─────────┘
          ▼

payment.notification.sent
```

---

# Fluxo de Auditoria

```text
                     RabbitMQ
                         │
                         │ payment.#
                         ▼

                ┌─────────────────┐
                │  AUDIT WORKER   │
                └────────┬────────┘
                         │
                         ▼

                ┌─────────────────┐
                │   AuditLogs     │
                └─────────────────┘
```

Explicação:

- O Audit Worker recebe todos os eventos do domínio Payment.
- Nenhum evento é ignorado.
- Permite rastrear toda a jornada de um pagamento.
- Facilita auditorias e investigações.

---

# Fluxo do Outbox Pattern

```text
┌─────────────┐
│ Payment API │
└──────┬──────┘
       │
       ▼

BEGIN TRANSACTION

       │
       ▼

INSERT Payment

       │
       ▼

INSERT OutboxMessage

       │
       ▼

COMMIT

       │
       ▼

Outbox Dispatcher

       │
       ▼

RabbitMQ
```

Objetivo:

Garantir consistência entre banco de dados e mensageria.

---

# Fluxo de Retry

```text
Mensagem Recebida
        │
        ▼

Processamento
        │
        ▼

Falha

        │
        ▼

Tentativa 1
    ↓ 5s

Tentativa 2
    ↓ 15s

Tentativa 3
    ↓ 45s
```

Após a terceira tentativa:

```text
Mensagem
   │
   ▼

DLQ
```

---

# Fluxo de Dead Letter Queue

```text
Processamento
      │
      ▼

Falha
      │
      ▼

Retry 1
      │
      ▼

Retry 2
      │
      ▼

Retry 3
      │
      ▼

┌──────────────────────┐
│ Dead Letter Queue    │
└──────────────────────┘
```

Objetivo:

Preservar mensagens que não puderam ser processadas.

---

# Fluxo de Idempotência

```text
Mensagem Recebida
        │
        ▼

ProcessedMessages
        │
        ▼

Já processada?
        │

   ┌────┴────┐
   │         │

  SIM       NÃO
   │         │

   ▼         ▼

Ignora    Processa
           Evento
              │
              ▼

      Registra Processamento
```

Objetivo:

Evitar que uma mesma mensagem produza efeitos duplicados.

---

# Fluxo Executivo Simplificado

```text
Cliente
   │
   ▼

Payment API
   │
   ▼

Banco de Dados
   │
   ▼

Outbox
   │
   ▼

RabbitMQ
   │
   ▼

Fraud Worker
   │
   ▼

Processor Worker
   │
   ▼

Notification Worker
   │
   ▼

Audit Worker
```

Resumo:

1. Cliente cria pagamento.
2. API registra pagamento.
3. Evento é salvo na Outbox.
4. Evento é publicado no RabbitMQ.
5. Fraude é analisada.
6. Pagamento é processado.
7. Cliente é notificado.
8. Todo o histórico é auditado.
