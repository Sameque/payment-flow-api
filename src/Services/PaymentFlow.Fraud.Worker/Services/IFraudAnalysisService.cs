using PaymentFlow.Contracts;

namespace PaymentFlow.Fraud.Worker.Services;

public interface IFraudAnalysisService
{
    Task<bool> AnalyzeAsync(PaymentCreatedEvent paymentCreated, CancellationToken cancellationToken = default);
}
