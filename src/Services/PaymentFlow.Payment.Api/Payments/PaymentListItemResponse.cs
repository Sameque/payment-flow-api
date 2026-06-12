using PaymentFlow.SharedKernel;

namespace PaymentFlow.Payment.Api.Payments;

public sealed record PaymentListItemResponse(
    Guid Id,
    Guid CustomerId,
    decimal Amount,
    PaymentStatus Status,
    Guid? CorrelationId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
