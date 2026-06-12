using PaymentFlow.Payment.Api.Common;
using PaymentFlow.Persistence;

namespace PaymentFlow.Payment.Api.Audit;

public sealed class AuditQueryService(IAuditLogRepository auditLogRepository) : IAuditQueryService
{
    public async Task<PaginatedResponse<AuditEventResponse>> SearchAsync(
        int page,
        int pageSize,
        string? correlationId,
        string? eventType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        (IReadOnlyList<AuditLog> items, int totalCount) = await auditLogRepository.SearchAsync(
            page,
            pageSize,
            correlationId,
            eventType,
            dateFrom,
            dateTo,
            cancellationToken);

        IReadOnlyList<AuditEventResponse> responses = items
            .Select(log => new AuditEventResponse(
                log.EventId,
                log.CorrelationId,
                log.EventType,
                log.CreatedAtUtc,
                log.PaymentId,
                log.Payload))
            .ToList();

        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<AuditEventResponse>(responses, totalCount, page, pageSize, totalPages);
    }
}
