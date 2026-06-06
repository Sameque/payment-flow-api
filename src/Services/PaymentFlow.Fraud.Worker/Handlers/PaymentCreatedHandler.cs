using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Outbox;
using PaymentFlow.Persistence;
using PaymentFlow.SharedKernel;

namespace PaymentFlow.Fraud.Worker;

public sealed class PaymentCreatedHandler(
    PaymentDbContext dbContext,
    IOutboxMessageWriter outboxMessageWriter,
    ILogger<PaymentCreatedHandler> logger) : IIntegrationEventHandler
{
    public string EventType => EventNames.PaymentCreated;

    public string ConsumerName => "fraud-worker";

    public async Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        var paymentCreated = (PaymentCreatedEvent)integrationEvent;

        Payment payment = await dbContext.Payments.FindAsync([paymentCreated.PaymentId], cancellationToken)
            ?? throw new InvalidOperationException($"Payment {paymentCreated.PaymentId} was not found for fraud analysis.");

        payment.MarkFraudAnalysis(DateTimeOffset.UtcNow);

        bool approved = paymentCreated.Amount <= 1000 || Random.Shared.NextDouble() >= 0.30;

        if (approved)
        {
            var approvedEvent = new FraudApprovedEvent(
                Guid.NewGuid(),
                paymentCreated.CorrelationId,
                DateTimeOffset.UtcNow,
                paymentCreated.PaymentId,
                paymentCreated.CustomerId,
                paymentCreated.Amount);

            outboxMessageWriter.Add(approvedEvent, RoutingKeys.PaymentFraudApproved);

            logger.LogInformation(
                "Fraud approved payment {PaymentId}; source event {EventId}; correlation {CorrelationId}.",
                paymentCreated.PaymentId,
                paymentCreated.EventId,
                paymentCreated.CorrelationId);
        }
        else
        {
            payment.MarkRejected(DateTimeOffset.UtcNow);

            var rejectedEvent = new FraudRejectedEvent(
                Guid.NewGuid(),
                paymentCreated.CorrelationId,
                DateTimeOffset.UtcNow,
                paymentCreated.PaymentId,
                paymentCreated.CustomerId,
                paymentCreated.Amount,
                "Fraud risk threshold exceeded.");

            outboxMessageWriter.Add(rejectedEvent, RoutingKeys.PaymentFraudRejected);

            logger.LogInformation(
                "Fraud rejected payment {PaymentId}; source event {EventId}; correlation {CorrelationId}.",
                paymentCreated.PaymentId,
                paymentCreated.EventId,
                paymentCreated.CorrelationId);
        }
    }
}
