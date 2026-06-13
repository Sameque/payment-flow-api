namespace PaymentFlow.Payment.Api.Audit;

public sealed record AuditEventResponse(
    Guid EventId,
    Guid CorrelationId,
    string EventType,
    DateTimeOffset Timestamp,
    Guid? PaymentId,
    string? Payload);
