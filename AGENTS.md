# AGENTS.md

## Project Identity

Project name: PaymentFlow

PaymentFlow is a portfolio-grade distributed payment processing platform built to demonstrate enterprise backend engineering skills using .NET 8 and RabbitMQ.

This repository must showcase:

- Event-driven architecture
- Distributed systems design
- RabbitMQ messaging
- Background workers
- Outbox Pattern
- Idempotent consumers
- Retry + Dead Letter Queue
- Structured logging
- Observability
- Clean Architecture
- Dockerized services

This codebase is intended for professional portfolio presentation for senior .NET backend/distributed systems opportunities.

---

# Primary Objective

Always prioritize:

1. Correct architecture
2. Production-grade code organization
3. Readability
4. Consistency
5. Compile-ready implementation
6. Realistic enterprise patterns

Do not generate tutorial-grade code.

Avoid simplistic examples.

All code should resemble a real-world backend platform.

---

# Technology Stack

## Backend

- .NET 8
- ASP.NET Core Web API
- Worker Services
- Entity Framework Core
- PostegreSQL
- RabbitMQ
- Serilog
- OpenTelemetry

## Infrastructure

- Docker
- Docker Compose

## Testing

- xUnit

---

# Architecture Style

Use:

- Clean Architecture
- DDD-lite
- Event-driven microservices
- Repository Pattern
- Outbox Pattern
- Idempotent Consumers

Do NOT use:

- Minimal APIs
- MediatR unless explicitly requested
- Generic repository anti-patterns
- Static service locators
- In-memory fake persistence unless required for tests

---

# Solution Structure

The solution must always preserve this structure.

```txt
PaymentFlow.sln

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

Never collapse these projects.

Never merge service responsibilities.

---

# RabbitMQ Topology

## Exchange

Name:

`pf.payment.exchange`

Type:

`topic`

Durable:

`true`

---

## Queues

### Fraud Queue

Queue:
`pf.payment.fraud.queue`

Binding:
`payment.transaction.created`

DLQ:
`pf.payment.fraud.dlq`

---

### Processor Queue

Queue:
`pf.payment.processor.queue`

Binding:
`payment.fraud.approved`

DLQ:
`pf.payment.processor.dlq`

---

### Notification Queue

Queue:
`pf.payment.notification.queue`

Bindings:

- `payment.processor.*`
- `payment.fraud.rejected`

DLQ:
`pf.payment.notification.dlq`

---

### Audit Queue

Queue:
`pf.payment.audit.queue`

Binding:
`payment.#`

DLQ:
`pf.payment.audit.dlq`

---

# Routing Key Convention

Always use:

`<context>.<aggregate>.<action>.<status>`

Valid routing keys:

- payment.transaction.created
- payment.fraud.approved
- payment.fraud.rejected
- payment.fraud.failed
- payment.processor.started
- payment.processor.approved
- payment.processor.rejected
- payment.processor.failed
- payment.notification.sent
- payment.notification.failed
- payment.audit.logged

Do not invent alternative naming conventions.

---

# Event Contracts

All events inherit from:

`IntegrationEvent`

Required fields:

- EventId
- CorrelationId
- OccurredAtUtc

Required contracts:

- PaymentCreatedEvent
- FraudApprovedEvent
- FraudRejectedEvent
- PaymentApprovedEvent
- PaymentRejectedEvent
- NotificationSentEvent

Events must be immutable record types.

Use nullable reference types correctly.

---

# Domain Rules

## Aggregate Root

Payment

Properties:

- Id
- CustomerId
- Amount
- Status
- CreatedAtUtc
- UpdatedAtUtc

---

## PaymentStatus

Allowed values only:

- Pending
- FraudAnalysis
- Approved
- Rejected
- Failed

Do not introduce additional states unless necessary.

---

# API Rules

Payment API exposes only:

## Create Payment

POST `/payments`

## Get Payment

GET `/payments/{id}`

POST must:

1. Validate request
2. Persist payment
3. Persist outbox event
4. Publish PaymentCreatedEvent

---

# Worker Responsibilities

## Fraud Worker

Consumes:

`payment.transaction.created`

Rules:

- Amount <= 1000 => approve
- Amount > 1000 => random 30% rejection

Publishes:

- payment.fraud.approved
- payment.fraud.rejected

---

## Processor Worker

Consumes:

`payment.fraud.approved`

Simulate outcomes:

- Approved
- Rejected
- Timeout

Publishes:

- payment.processor.approved
- payment.processor.rejected

---

## Notification Worker

Consumes final events.

Logs notification dispatch.

Publishes:

- payment.notification.sent

---

## Audit Worker

Consumes:

`payment.#`

Persists audit logs.

---

# Outbox Pattern

Must always be implemented.

Table:

OutboxMessages

Columns:

- Id
- EventType
- Payload
- Processed
- CreatedAtUtc

A background dispatcher must publish pending messages.

Never bypass the outbox directly from API handlers.

---

# Idempotency

Must always be implemented.

Table:

ProcessedMessages

Columns:

- MessageId
- ConsumerName
- ProcessedAtUtc

Consumers must skip duplicates.

---

# Retry Policy

Required:

3 retries

Backoff:

- 5s
- 15s
- 45s

After max retries:

Send to DLQ

Use manual ACK/NACK.

---

# Database Rules

Use:

Entity Framework Core migrations

Required tables:

- Payments
- OutboxMessages
- ProcessedMessages
- AuditLogs

Do not use EnsureCreated.

Always generate migrations.

---

# Observability

Required:

## Logging

Use Serilog structured logging.

Always include:

- CorrelationId
- PaymentId
- EventId

---

## Tracing

Use OpenTelemetry.

Track:

- API request flow
- Event publication
- Event consumption
- Worker execution

---

# Docker Rules

Must generate:

docker-compose.yml

Required services:

- rabbitmq
- sqlserver
- payment-api
- fraud-worker
- processor-worker
- notification-worker
- audit-worker

Expose RabbitMQ management UI.

---

# Coding Standards

Always:

- Use async/await
- Use constructor DI
- Use nullable reference types
- Use file-scoped namespaces
- Use explicit access modifiers
- Use strongly typed configuration
- Use cancellation tokens
- Use proper exception handling
- Generate compile-ready code

Never:

- Leave TODO placeholders
- Generate pseudo-code
- Stub implementations
- Omit Program.cs
- Omit dependency injection wiring

---

# Implementation Order

When generating code, follow this order:

1. Solution scaffold
2. Shared contracts
3. Persistence
4. RabbitMQ infrastructure
5. Payment API
6. Fraud Worker
7. Processor Worker
8. Notification Worker
9. Audit Worker
10. Docker
11. README
12. Tests

Do not skip order.

---

# README Requirements

README must include:

- Architecture overview
- Folder structure
- Event flow
- Queue topology
- Setup instructions
- Run instructions
- API examples
- Failure scenarios
- DLQ explanation

---

# Agent Roles

## SOLID Architect
Ensures all code strictly adheres to SOLID principles to maintain a professional, enterprise-grade architecture:
- **S**ingle Responsibility: Each class and module must have one, and only one, reason to change.
- **O**pen-Closed: Software entities should be open for extension, but closed for modification.
- **L**iskov Substitution: Objects of a superclass should be replaceable with objects of its subclasses without breaking the application.
- **I**nterface Segregation: No client should be forced to depend on methods it does not use.
- **D**ependency Inversion: High-level modules should not depend on low-level modules; both should depend on abstractions.

---

# Agent Behavior Rules

When making changes:

1. Preserve architectural consistency
2. Avoid unnecessary refactors
3. Do not rename established queues/exchanges/contracts
4. Prefer incremental implementation
5. Validate compile correctness before introducing new features

If ambiguity exists, choose enterprise backend best practices for .NET distributed systems.