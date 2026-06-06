namespace PaymentFlow.Contracts;

public sealed record PaymentApprovedEvent(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid PaymentId,
    Guid CustomerId,
    decimal Amount,
    string ProcessorReference) : IntegrationEvent(EventId, CorrelationId, OccurredAtUtc);
