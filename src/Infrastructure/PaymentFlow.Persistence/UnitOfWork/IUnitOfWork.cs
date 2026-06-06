using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace PaymentFlow.Persistence;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
