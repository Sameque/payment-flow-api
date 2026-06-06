using System.Threading.Tasks;

namespace PaymentFlow.Payment.Api.Payments;

public interface IPaymentApplicationService
{
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken);
    Task<PaymentResponse?> GetAsync(Guid paymentId, CancellationToken cancellationToken);
}
