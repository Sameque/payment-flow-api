namespace PaymentFlow.Contracts;

public sealed record NotificationSentEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid PaymentId,
    Guid CustomerId,
    string NotificationChannel) : IntegrationEvent(EventId, CorrelationId, OccurredAtUtc);
