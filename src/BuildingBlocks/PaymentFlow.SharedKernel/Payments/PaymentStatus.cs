namespace PaymentFlow.SharedKernel;

public enum PaymentStatus
{
    Pending = 0,
    FraudAnalysis = 1,
    Approved = 2,
    Rejected = 3,
    Failed = 4
}
