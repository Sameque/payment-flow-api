namespace PaymentFlow.Payment.Api.Payments;

public sealed record PaymentTimelineStepResponse(
    string Id,
    string Label,
    string EventType,
    string Status,
    DateTimeOffset? Timestamp,
    string? Description);
