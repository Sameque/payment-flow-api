namespace PaymentFlow.Contracts;

public sealed record FraudRejectedEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid PaymentId,
    Guid CustomerId,
    decimal Amount,
    string Reason) : IntegrationEvent(EventId, CorrelationId, OccurredAtUtc);
