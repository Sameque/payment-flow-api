# PaymentFlow

PaymentFlow is a distributed payment processing platform built with .NET 8, RabbitMQ, SQL Server, EF Core, hosted workers, the Outbox Pattern, idempotent consumers, structured logging, and OpenTelemetry.

## Architecture Overview

The platform models a payment lifecycle as integration events. The Payment API persists the aggregate and an outbox message in one transaction. A background outbox dispatcher publishes events to RabbitMQ. Workers consume durable queues with manual ACK/NACK, idempotency checks, retry backoff, and dead-letter queues.

To ensure database consistency across deployments, the system implements a **Migration Sidecar** pattern: a dedicated container applies all pending Entity Framework migrations before the main API and workers start.

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

## Payment Lifecycle Flow

The payment process is a distributed saga managed via integration events.

### 1. Initiation
- **Request**: `POST /payments` $\rightarrow$ `PaymentApplicationService`.
- **Persistence**: The service creates a `Payment` entity (Status: `Pending`) and a `PaymentCreatedEvent` in the `OutboxMessages` table within a single transaction.
- **Dispatch**: A background worker publishes `payment.transaction.created` to RabbitMQ.

### 2. Fraud Analysis
- **Consumption**: `Fraud Worker` consumes the event.
- **Logic**: `FraudAnalysisService` evaluates the payment.
  - **Approved**: Payment status moves to `FraudAnalysis` $\rightarrow$ publishes `payment.fraud.approved`.
  - **Rejected**: Payment status moves to `Rejected` $\rightarrow$ publishes `payment.fraud.rejected`.

### 3. Payment Processing
- **Consumption**: `Processor Worker` consumes `payment.fraud.approved`.
- **Logic**: `PaymentProcessorService` simulates an external payment gateway.
  - **Approved**: Payment status moves to `Approved` $\rightarrow$ publishes `payment.processor.approved`.
  - **Rejected**: Payment status moves to `Rejected` $\rightarrow$ publishes `payment.processor.rejected`.
  - **Timeout**: Throws an exception to trigger the retry policy and eventual DLQ.

### 4. Finalization & Observability
- **Notifications**: `Notification Worker` consumes final outcomes (`approved`, `rejected`, `fraud.rejected`) and dispatches notifications.
- **Audit**: `Audit Worker` consumes all events (`payment.#`) and uses `AuditService` to persist a full immutable history of the payment lifecycle.

### Status Transitions
`Pending` $\rightarrow$ `FraudAnalysis` $\rightarrow$ `Approved` / `Rejected` / `Failed`

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
- EF Core Tools: `dotnet tool install --global dotnet-ef`

Run infrastructure and services:

```powershell
docker compose up --build
```

**Note:** Database migrations are automatically applied by the `payment-migration` container before the API starts.

For manual migration updates during development:

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
