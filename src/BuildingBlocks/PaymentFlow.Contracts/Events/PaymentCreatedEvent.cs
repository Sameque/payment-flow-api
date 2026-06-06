namespace PaymentFlow.Contracts;

public sealed record PaymentCreatedEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid PaymentId,
    Guid CustomerId,
    decimal Amount) : IntegrationEvent(EventId, CorrelationId, OccurredAtUtc);
