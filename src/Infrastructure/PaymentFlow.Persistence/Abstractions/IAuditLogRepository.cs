namespace PaymentFlow.Persistence;

public interface IAuditLogRepository
{
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> SearchAsync(
        int page,
        int pageSize,
        string? correlationId,
        string? eventType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AuditLog>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken);

    Task<Guid?> GetCorrelationIdForPaymentAsync(Guid paymentId, CancellationToken cancellationToken);
}
