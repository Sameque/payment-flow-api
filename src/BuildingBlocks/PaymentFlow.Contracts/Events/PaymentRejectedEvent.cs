namespace PaymentFlow.Contracts;

public sealed record PaymentRejectedEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid PaymentId,
    Guid CustomerId,
    decimal Amount,
    string Reason) : IntegrationEvent(EventId, CorrelationId, OccurredAtUtc), IPaymentEvent;
