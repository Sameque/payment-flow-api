namespace PaymentFlow.Payment.Api.Dashboard;

public interface IDashboardQueryService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<PaymentsPerHourPointResponse>> GetPaymentsPerHourAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ApprovalRatePointResponse>> GetApprovalRateAsync(CancellationToken cancellationToken);
}
