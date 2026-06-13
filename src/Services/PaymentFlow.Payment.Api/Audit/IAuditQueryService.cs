using PaymentFlow.Payment.Api.Common;

namespace PaymentFlow.Payment.Api.Audit;

public interface IAuditQueryService
{
    Task<PaginatedResponse<AuditEventResponse>> SearchAsync(
        int page,
        int pageSize,
        string? correlationId,
        string? eventType,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        CancellationToken cancellationToken);
}
