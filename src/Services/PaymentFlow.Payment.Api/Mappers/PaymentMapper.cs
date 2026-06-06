using PaymentFlow.SharedKernel;
using PaymentFlow.Payment.Api.Payments;
using DomainPayment = PaymentFlow.SharedKernel.Payment;

namespace PaymentFlow.Payment.Api.Mappers;

public sealed class PaymentMapper : IPaymentMapper
{
    public PaymentResponse Map(DomainPayment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.CustomerId,
            payment.Amount,
            payment.Status,
            payment.CreatedAtUtc,
            payment.UpdatedAtUtc);
    }
}
