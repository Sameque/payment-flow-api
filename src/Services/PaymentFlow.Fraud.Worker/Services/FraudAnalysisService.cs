using PaymentFlow.Contracts;

namespace PaymentFlow.Fraud.Worker.Services;

public sealed class FraudAnalysisService : IFraudAnalysisService
{
    public Task<bool> AnalyzeAsync(PaymentCreatedEvent paymentCreated, CancellationToken cancellationToken = default)
    {
        // Rules: Amount <= 1000 => approve, Amount > 1000 => random 30% rejection
        bool approved = paymentCreated.Amount <= 1000 || Random.Shared.NextDouble() >= 0.30;
        return Task.FromResult(approved);
    }
}
