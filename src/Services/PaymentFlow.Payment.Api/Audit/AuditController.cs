using Microsoft.AspNetCore.Mvc;
using PaymentFlow.Payment.Api.Common;

namespace PaymentFlow.Payment.Api.Audit;

[ApiController]
[Route("audit")]
public sealed class AuditController(IAuditQueryService auditQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AuditEventResponse>>> Search(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? correlationId = null,
        [FromQuery] string? eventType = null,
        [FromQuery] DateTimeOffset? dateFrom = null,
        [FromQuery] DateTimeOffset? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        PaginatedResponse<AuditEventResponse> response = await auditQueryService.SearchAsync(
            page,
            pageSize,
            correlationId,
            eventType,
            dateFrom,
            dateTo,
            cancellationToken);

        return Ok(response);
    }
}
