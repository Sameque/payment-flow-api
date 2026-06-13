using PaymentFlow.Payment.Api.Common;
using PaymentFlow.SharedKernel;

namespace PaymentFlow.Payment.Api.Payments;

public interface IPaymentApplicationService
{
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken);

    Task<PaymentResponse?> GetAsync(Guid paymentId, CancellationToken cancellationToken);

    Task<PaymentDetailsResponse?> GetDetailsAsync(Guid paymentId, CancellationToken cancellationToken);

    Task<PaginatedResponse<PaymentListItemResponse>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        PaymentStatus? status,
        CancellationToken cancellationToken);
}
