using PaymentFlow.SharedKernel;

namespace PaymentFlow.Payment.Api.Payments;

public sealed record PaymentResponse(
    Guid Id,
    Guid CustomerId,
    decimal Amount,
    PaymentStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
