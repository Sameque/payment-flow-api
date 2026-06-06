# PaymentFlow

PaymentFlow is a distributed payment processing platform built with .NET 8, RabbitMQ, SQL Server, EF Core, hosted workers, the Outbox Pattern, idempotent consumers, structured logging, and OpenTelemetry.

## Architecture Overview

The platform models a payment lifecycle as integration events. The Payment API persists the aggregate and an outbox message in one transaction. A background outbox dispatcher publishes events to RabbitMQ. Workers consume durable queues with manual ACK/NACK, idempotency checks, retry backoff, and dead-letter queues.

## Folder Structure

```txt
src/
  BuildingBlocks/
    PaymentFlow.SharedKernel
    PaymentFlow.Contracts
    PaymentFlow.Messaging
    PaymentFlow.Observability
  Services/
    PaymentFlow.Payment.Api
    PaymentFlow.Fraud.Worker
    PaymentFlow.Processor.Worker
    PaymentFlow.Notification.Worker
    PaymentFlow.Audit.Worker
  Infrastructure/
    PaymentFlow.Persistence
    PaymentFlow.Outbox
    PaymentFlow.RabbitMq
tests/
  Unit
  Integration
```

## Event Flow

1. `POST /payments` creates a `Payment` and stores `PaymentCreatedEvent` in `OutboxMessages`.
2. Outbox dispatcher publishes `payment.transaction.created`.
3. Fraud worker consumes it and publishes `payment.fraud.approved` or `payment.fraud.rejected`.
4. Processor worker consumes approvals and publishes `payment.processor.approved` or `payment.processor.rejected`.
5. Notification worker consumes final events and publishes `payment.notification.sent`.
6. Audit worker consumes `payment.#` and stores audit rows.

## Queue Topology

Exchange: `pf.payment.exchange` topic, durable.

Queues:

- `pf.payment.fraud.queue` -> `payment.transaction.created`, DLQ `pf.payment.fraud.dlq`
- `pf.payment.processor.queue` -> `payment.fraud.approved`, DLQ `pf.payment.processor.dlq`
- `pf.payment.notification.queue` -> `payment.processor.*`, `payment.fraud.rejected`, DLQ `pf.payment.notification.dlq`
- `pf.payment.audit.queue` -> `payment.#`, DLQ `pf.payment.audit.dlq`

## Setup

Install:

- .NET 8 SDK
- Docker Desktop

Run infrastructure and services:

```powershell
docker compose up --build
```

Apply EF migrations before processing real requests:

```powershell
dotnet ef database update --project src/Infrastructure/PaymentFlow.Persistence --startup-project src/Services/PaymentFlow.Payment.Api
```

## API Examples

Create payment:

```http
POST http://localhost:8080/payments
Content-Type: application/json

{
  "customerId": "5fca48cd-24a4-4cf6-af5c-bda0636a3672",
  "amount": 250.00,
  "correlationId": "651e3f99-872d-46f2-99e6-9666b4240a68"
}
```

Get payment:

```http
GET http://localhost:8080/payments/{id}
```

## Failure Scenarios

Consumers use three retries with 5s, 15s, and 45s backoff. Processor timeout simulation throws an exception so the message remains under retry control. After max retries, the service issues `BasicNack(..., requeue: false)` and RabbitMQ dead-letters the message.

## DLQ Explanation

Each service queue is configured with a dead-letter exchange and dedicated DLQ. Poison messages are moved to the service-specific DLQ so operators can inspect failed payloads without blocking the main queue.
