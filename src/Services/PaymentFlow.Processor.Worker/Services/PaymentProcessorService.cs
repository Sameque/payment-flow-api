using PaymentFlow.Contracts;

namespace PaymentFlow.Processor.Worker.Services;

public sealed class PaymentProcessorService : IPaymentProcessorService
{
    public Task<ProcessingResult> ProcessAsync(FraudApprovedEvent fraudApproved, CancellationToken cancellationToken = default)
    {
        int outcome = Random.Shared.Next(0, 3);

        if (outcome == 2)
        {
            throw new TimeoutException($"Processor timeout for payment {fraudApproved.PaymentId}.");
        }

        if (outcome == 0)
        {
            return Task.FromResult(new ProcessingResult(
                IsApproved: true,
                Reference: $"PF-{Guid.NewGuid():N}",
                Reason: null));
        }

        return Task.FromResult(new ProcessingResult(
            IsApproved: false,
            Reference: null,
            Reason: "Processor authorization rejected."));
    }
}
