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
}
