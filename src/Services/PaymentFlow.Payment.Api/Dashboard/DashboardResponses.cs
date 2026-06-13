namespace PaymentFlow.Payment.Api.Dashboard;

public sealed record DashboardSummaryResponse(
    int TotalPayments,
    int ApprovedPayments,
    int RejectedPayments,
    int DlqMessages);

public sealed record PaymentsPerHourPointResponse(string Hour, int Count);

public sealed record ApprovalRatePointResponse(string Date, decimal Rate);
