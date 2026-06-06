using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Outbox;
using PaymentFlow.Persistence;
using PaymentFlow.SharedKernel;

namespace PaymentFlow.Processor.Worker;

public sealed class FraudApprovedHandler(
    PaymentDbContext dbContext,
    IOutboxMessageWriter outboxMessageWriter,
    ILogger<FraudApprovedHandler> logger) : IIntegrationEventHandler
{
    public string EventType => EventNames.FraudApproved;

    public string ConsumerName => "processor-worker";

    public async Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        var fraudApproved = (FraudApprovedEvent)integrationEvent;

        Payment payment = await dbContext.Payments.FindAsync([fraudApproved.PaymentId], cancellationToken)
            ?? throw new InvalidOperationException($"Payment {fraudApproved.PaymentId} was not found for processing.");

        int outcome = Random.Shared.Next(0, 3);
        if (outcome == 2)
        {
            throw new TimeoutException($"Processor timeout for payment {fraudApproved.PaymentId}.");
        }

        if (outcome == 0)
        {
            payment.MarkApproved(DateTimeOffset.UtcNow);

            var approvedEvent = new PaymentApprovedEvent(
                Guid.NewGuid(),
                fraudApproved.CorrelationId,
                DateTimeOffset.UtcNow,
                fraudApproved.PaymentId,
                fraudApproved.CustomerId,
                fraudApproved.Amount,
                $"PF-{Guid.NewGuid():N}");

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
                "Processor authorization rejected.");

            outboxMessageWriter.Add(rejectedEvent, RoutingKeys.PaymentProcessorRejected);

            logger.LogInformation(
                "Processor rejected payment {PaymentId}; source event {EventId}; correlation {CorrelationId}.",
                fraudApproved.PaymentId,
                fraudApproved.EventId,
                fraudApproved.CorrelationId);
        }
    }
}
