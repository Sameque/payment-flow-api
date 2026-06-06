namespace PaymentFlow.Contracts;

public sealed record FraudApprovedEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid PaymentId,
    Guid CustomerId,
    decimal Amount) : IntegrationEvent(EventId, CorrelationId, OccurredAtUtc), IPaymentEvent;
