namespace PaymentFlow.Contracts;

public abstract record IntegrationEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc);
