# PaymentFlow - Documentação Funcional e Técnica Completa

## Objetivo deste Documento

Este documento complementa o README.md e tem como objetivo explicar, em detalhes, o funcionamento da plataforma PaymentFlow.

O público-alvo inclui:

- Desenvolvedores
- Arquitetos de Software
- Analistas de Sistemas
- Product Owners
- Gestores
- Analistas de Suporte
- Profissionais não técnicos

Este documento descreve os processos de negócio, os componentes técnicos, o fluxo das mensagens, os mecanismos de resiliência e as decisões arquiteturais adotadas.

---

# Visão Geral do Sistema

PaymentFlow é uma plataforma de processamento de pagamentos orientada a eventos.

Em vez de processar tudo de forma síncrona dentro de uma única API, cada etapa do processo é executada por serviços independentes que se comunicam através do RabbitMQ.

Benefícios:

- Escalabilidade
- Resiliência
- Baixo acoplamento
- Facilidade de manutenção
- Tolerância a falhas

---

# Problema de Negócio

Quando um cliente realiza um pagamento, diversas validações precisam ocorrer:

1. Registro do pagamento
2. Análise antifraude
3. Processamento financeiro
4. Notificação ao cliente
5. Auditoria

Executar tudo dentro de uma única requisição HTTP aumenta o risco de falhas e reduz a escalabilidade.

Por esse motivo foi adotada uma arquitetura baseada em eventos.

---

# Visão Geral da Arquitetura

```text
Cliente
   |
   v
Payment API
   |
   v
SQL Server
   |
   v
Outbox
   |
   v
RabbitMQ
   |
   +--> Fraud Worker
   |
   +--> Processor Worker
   |
   +--> Notification Worker
   |
   +--> Audit Worker
```

---

# Componentes do Sistema

## Payment API

Responsável por:

- Receber solicitações
- Validar dados
- Persistir pagamentos
- Registrar eventos na Outbox

Não executa:

- Análise antifraude
- Processamento financeiro
- Notificações

Essas responsabilidades pertencem aos Workers.

---

## RabbitMQ

Responsável pelo transporte das mensagens.

O RabbitMQ funciona como um intermediador entre os serviços.

Sua função é garantir que as mensagens sejam entregues aos consumidores corretos.

---

## Fraud Worker

Responsável por analisar riscos.

Funções:

- Receber pagamentos criados
- Executar regras antifraude
- Aprovar ou rejeitar pagamentos

---

## Processor Worker

Responsável pelo processamento financeiro.

Funções:

- Simular adquirente financeira
- Aprovar ou rejeitar pagamentos
- Simular falhas temporárias

---

## Notification Worker

Responsável por informar o resultado do processamento.

Exemplos:

- E-mail
- Push Notification
- Webhook
- SMS

No MVP, realiza apenas registro em log.

---

## Audit Worker

Responsável pelo rastreamento completo do sistema.

Funções:

- Registrar todos os eventos
- Permitir auditorias futuras
- Apoiar investigações de incidentes

---

# Fluxo Completo do Negócio

## Etapa 1 – Criação do Pagamento

Cliente envia:

```http
POST /payments
```

Exemplo:

```json
{
  "customerId": "123",
  "amount": 100.00
}
```

A API:

1. Valida os dados
2. Cria o pagamento
3. Persiste no banco
4. Cria evento na Outbox

Resultado:

```text
Pagamento registrado
Evento aguardando publicação
```

---

# Etapa 2 – Outbox Pattern

O evento ainda não é enviado ao RabbitMQ.

Primeiro ele é salvo na tabela:

```text
OutboxMessages
```

Isso garante consistência entre:

- Banco de dados
- Mensageria

Sem esse mecanismo poderia existir pagamento salvo sem evento publicado.

---

# Etapa 3 – Dispatcher

Um processo em background monitora a tabela Outbox.

Fluxo:

```text
Busca eventos pendentes
↓
Publica no RabbitMQ
↓
Marca como processado
```

---

# Etapa 4 – RabbitMQ

Exchange:

```text
pf.payment.exchange
```

Routing Key:

```text
payment.transaction.created
```

A mensagem é encaminhada para:

```text
pf.payment.fraud.queue
```

---

# Etapa 5 – Análise Antifraude

Fraud Worker recebe:

```text
payment.transaction.created
```

Regra do MVP:

```text
Valor <= 1000
→ Aprova

Valor > 1000
→ 30% de chance de rejeição
```

Eventos possíveis:

```text
payment.fraud.approved
payment.fraud.rejected
```

---

# Etapa 6 – Processamento Financeiro

Quando aprovado na fraude:

```text
payment.fraud.approved
```

O Processor Worker inicia o processamento.

Possíveis resultados:

- Aprovado
- Rejeitado
- Timeout

Eventos gerados:

```text
payment.processor.approved
payment.processor.rejected
```

---

# Etapa 7 – Notificação

Notification Worker recebe eventos finais.

Exemplos:

```text
Pagamento aprovado
Pagamento rejeitado
```

Objetivo:

Informar o cliente sobre o resultado.

---

# Etapa 8 – Auditoria

Audit Worker escuta:

```text
payment.#
```

Isso significa:

Todos os eventos do domínio Payment.

Cada evento é armazenado em:

```text
AuditLogs
```

---

# Outbox Pattern em Detalhes

## Problema

Sem Outbox:

```text
Salvar pagamento
↓
RabbitMQ indisponível
↓
Evento perdido
```

Resultado:

Sistema inconsistente.

---

## Solução

Salvar:

```text
Pagamento
+
Evento
```

Na mesma transação.

Somente depois:

```text
Dispatcher
↓
RabbitMQ
```

---

# Idempotência

## Problema

Uma mesma mensagem pode ser entregue mais de uma vez.

Exemplo:

```text
Mensagem processada
↓
Falha antes do ACK
↓
RabbitMQ reenviará
```

---

## Solução

Tabela:

```text
ProcessedMessages
```

Fluxo:

```text
Recebe mensagem
↓
Já foi processada?
↓
Sim → Ignora
Não → Processa
```

---

# Retry

Falhas temporárias são normais.

Exemplos:

- Rede
- Banco
- RabbitMQ
- Serviço externo

Configuração:

```text
Tentativa 1
↓ 5s

Tentativa 2
↓ 15s

Tentativa 3
↓ 45s
```

---

# Dead Letter Queue (DLQ)

Quando todas as tentativas falham:

```text
Mensagem
↓
DLQ
```

Exemplo:

```text
pf.payment.processor.dlq
```

Objetivo:

Permitir análise posterior sem perda de dados.

---

# Banco de Dados

## Payments

Armazena pagamentos.

Campos principais:

- Id
- CustomerId
- Amount
- Status
- CreatedAtUtc
- UpdatedAtUtc

---

## OutboxMessages

Armazena eventos pendentes.

---

## ProcessedMessages

Controla idempotência.

---

## AuditLogs

Armazena histórico completo.

---

# Observabilidade

## Logs

Todos os serviços devem registrar:

- CorrelationId
- EventId
- PaymentId

Benefício:

Rastreamento ponta a ponta.

---

## OpenTelemetry

Permite acompanhar:

```text
API
↓
RabbitMQ
↓
Worker
↓
Banco
```

Facilitando diagnósticos.

---

# CorrelationId

Cada pagamento recebe um identificador de correlação.

Exemplo:

```text
CorrelationId:
ABC123
```

Esse identificador acompanha todo o fluxo.

Benefício:

Relacionar todos os eventos do mesmo pagamento.

---

# Fluxo Resumido

```text
Cliente
↓
Payment API
↓
SQL Server
↓
Outbox
↓
RabbitMQ
↓
Fraud Worker
↓
Processor Worker
↓
Notification Worker
↓
Audit Worker
```

---

# Cenários de Falha

## RabbitMQ indisponível

Impacto:

Nenhum pagamento é perdido.

Motivo:

Eventos permanecem na Outbox.

---

## Worker indisponível

Impacto:

Mensagens aguardam na fila.

Processamento retomará após recuperação.

---

## Banco indisponível

Impacto:

Novos pagamentos não serão registrados.

Sistema falha de forma consistente.

---

# Objetivos Arquiteturais

A arquitetura foi desenhada para demonstrar:

- Sistemas distribuídos
- Event-Driven Architecture
- RabbitMQ
- Outbox Pattern
- Idempotência
- Retry
- DLQ
- Observabilidade
- Clean Architecture
- Resiliência

---

# Conclusão

PaymentFlow foi projetado como um projeto de portfólio com características encontradas em plataformas corporativas reais, incluindo gateways de pagamento, marketplaces, ERPs e sistemas financeiros.

O foco principal é demonstrar boas práticas de arquitetura distribuída utilizando .NET 8, RabbitMQ e padrões amplamente utilizados na indústria.
