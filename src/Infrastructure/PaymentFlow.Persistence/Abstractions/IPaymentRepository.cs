using PaymentFlow.SharedKernel;

namespace PaymentFlow.Persistence;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken);

    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Payment> Items, int TotalCount)> SearchAsync(
        int page,
        int pageSize,
        string? search,
        PaymentStatus? status,
        CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task<int> CountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken);

    Task<IReadOnlyList<Payment>> GetCreatedSinceAsync(DateTimeOffset since, CancellationToken cancellationToken);
}
