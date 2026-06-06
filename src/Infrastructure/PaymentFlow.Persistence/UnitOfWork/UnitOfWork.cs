using Microsoft.EntityFrameworkCore.Storage;

namespace PaymentFlow.Persistence;

public sealed class UnitOfWork(PaymentDbContext dbContext) : IUnitOfWork
{
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }
}
