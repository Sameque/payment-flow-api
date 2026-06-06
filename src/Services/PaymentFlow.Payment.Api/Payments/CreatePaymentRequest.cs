namespace PaymentFlow.Payment.Api.Payments;

public sealed record CreatePaymentRequest(
    Guid CustomerId,
    decimal Amount,
    Guid? CorrelationId);
