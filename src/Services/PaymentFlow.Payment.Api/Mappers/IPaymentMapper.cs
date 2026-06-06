using PaymentFlow.SharedKernel;
using PaymentFlow.Payment.Api.Payments;
using DomainPayment = PaymentFlow.SharedKernel.Payment;

namespace PaymentFlow.Payment.Api.Mappers;

public interface IPaymentMapper
{
    PaymentResponse Map(DomainPayment payment);
}
