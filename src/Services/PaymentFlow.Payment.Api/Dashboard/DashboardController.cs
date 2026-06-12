using Microsoft.AspNetCore.Mvc;

namespace PaymentFlow.Payment.Api.Dashboard;

[ApiController]
[Route("dashboard")]
public sealed class DashboardController(IDashboardQueryService dashboardQueryService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await dashboardQueryService.GetSummaryAsync(cancellationToken));
    }

    [HttpGet("payments-per-hour")]
    public async Task<ActionResult<IReadOnlyList<PaymentsPerHourPointResponse>>> GetPaymentsPerHour(
        CancellationToken cancellationToken)
    {
        return Ok(await dashboardQueryService.GetPaymentsPerHourAsync(cancellationToken));
    }

    [HttpGet("approval-rate")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRatePointResponse>>> GetApprovalRate(
        CancellationToken cancellationToken)
    {
        return Ok(await dashboardQueryService.GetApprovalRateAsync(cancellationToken));
    }
}
