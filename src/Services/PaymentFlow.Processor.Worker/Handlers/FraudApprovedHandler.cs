using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Outbox;
using PaymentFlow.Persistence;
using PaymentFlow.SharedKernel;
using PaymentFlow.Processor.Worker.Services;

namespace PaymentFlow.Processor.Worker;

public sealed class FraudApprovedHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IPaymentProcessorService processorService,
    IOutboxMessageWriter outboxMessageWriter,
    ILogger<FraudApprovedHandler> logger) : IIntegrationEventHandler
{
    public string EventType => EventNames.FraudApproved;

    public string ConsumerName => "processor-worker";

    public async Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        var fraudApproved = (FraudApprovedEvent)integrationEvent;

        Payment payment = await paymentRepository.GetByIdAsync(fraudApproved.PaymentId, cancellationToken)
            ?? throw new InvalidOperationException($"Payment {fraudApproved.PaymentId} was not found for processing.");

        var result = await processorService.ProcessAsync(fraudApproved, cancellationToken);

        if (result.IsApproved)
        {
            payment.MarkApproved(DateTimeOffset.UtcNow);

            var approvedEvent = new PaymentApprovedEvent(
                Guid.NewGuid(),
                fraudApproved.CorrelationId,
                DateTimeOffset.UtcNow,
                fraudApproved.PaymentId,
                fraudApproved.CustomerId,
                fraudApproved.Amount,
                result.Reference!);

            outboxMessageWriter.Add(approvedEvent, RoutingKeys.PaymentProcessorApproved);

            logger.LogInformation(
                "Processor approved payment {PaymentId}; source event {EventId}; correlation {CorrelationId}.",
                fraudApproved.PaymentId,
                fraudApproved.EventId,
                fraudApproved.CorrelationId);
        }
        else
        {
            payment.MarkRejected(DateTimeOffset.UtcNow);

            var rejectedEvent = new PaymentRejectedEvent(
                Guid.NewGuid(),
                fraudApproved.CorrelationId,
                DateTimeOffset.UtcNow,
                fraudApproved.PaymentId,
                fraudApproved.CustomerId,
                fraudApproved.Amount,
                result.Reason ?? "Processor authorization rejected.");

            outboxMessageWriter.Add(rejectedEvent, RoutingKeys.PaymentProcessorRejected);

            logger.LogInformation(
                "Processor rejected payment {PaymentId}; source event {EventId}; correlation {CorrelationId}.",
                fraudApproved.PaymentId,
                fraudApproved.EventId,
                fraudApproved.CorrelationId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
