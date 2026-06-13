using Microsoft.EntityFrameworkCore;

namespace PaymentFlow.Persistence;

public sealed class AuditLogRepository(PaymentDbContext dbContext) : IAuditLogRepository
{
    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> SearchAsync(
        int page,
        int pageSize,
        string? correlationId,
        string? eventType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken cancellationToken)
    {
        IQueryable<AuditLog> query = dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            string normalized = correlationId.Trim();
            query = query.Where(log => EF.Functions.ILike(log.CorrelationId.ToString(), $"%{normalized}%"));
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(log => log.EventType == eventType);
        }

        if (dateFrom.HasValue)
        {
            DateTimeOffset utcFrom = dateFrom.Value.ToUniversalTime();
            query = query.Where(log => log.CreatedAtUtc >= utcFrom);
        }

        if (dateTo.HasValue)
        {
            DateTimeOffset utcTo = dateTo.Value.ToUniversalTime();
            query = query.Where(log => log.CreatedAtUtc <= utcTo);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<AuditLog> items = await query
            .OrderByDescending(log => log.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.PaymentId == paymentId)
            .OrderBy(log => log.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> GetCorrelationIdForPaymentAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        return await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.PaymentId == paymentId && log.EventType == "PaymentCreatedEvent")
            .OrderBy(log => log.CreatedAtUtc)
            .Select(log => (Guid?)log.CorrelationId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
