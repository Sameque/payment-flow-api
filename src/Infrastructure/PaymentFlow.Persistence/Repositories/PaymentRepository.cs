using Microsoft.EntityFrameworkCore;
using PaymentFlow.SharedKernel;

namespace PaymentFlow.Persistence;

public sealed class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        return dbContext.Payments.SingleOrDefaultAsync(payment => payment.Id == paymentId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Payment> Items, int TotalCount)> SearchAsync(
        int page,
        int pageSize,
        string? search,
        PaymentStatus? status,
        CancellationToken cancellationToken)
    {
        IQueryable<Payment> query = dbContext.Payments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalized = search.Trim();
            bool isGuid = Guid.TryParse(normalized, out Guid guidValue);

            query = query.Where(payment =>
                (isGuid && (payment.Id == guidValue || payment.CustomerId == guidValue)) ||
                EF.Functions.ILike(payment.Id.ToString(), $"%{normalized}%") ||
                EF.Functions.ILike(payment.CustomerId.ToString(), $"%{normalized}%"));
        }

        if (status.HasValue)
        {
            query = query.Where(payment => payment.Status == status.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<Payment> items = await query
            .OrderByDescending(payment => payment.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return dbContext.Payments.CountAsync(cancellationToken);
    }

    public Task<int> CountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken)
    {
        return dbContext.Payments.CountAsync(payment => payment.Status == status, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetCreatedSinceAsync(DateTimeOffset since, CancellationToken cancellationToken)
    {
        DateTimeOffset utcSince = since.ToUniversalTime();

        return await dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.CreatedAtUtc >= utcSince)
            .ToListAsync(cancellationToken);
    }
}
