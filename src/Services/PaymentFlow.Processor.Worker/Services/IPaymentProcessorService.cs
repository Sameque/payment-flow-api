using PaymentFlow.Contracts;

namespace PaymentFlow.Processor.Worker.Services;

public record ProcessingResult(bool IsApproved, string? Reference, string? Reason);

public interface IPaymentProcessorService
{
    Task<ProcessingResult> ProcessAsync(FraudApprovedEvent fraudApproved, CancellationToken cancellationToken = default);
}
