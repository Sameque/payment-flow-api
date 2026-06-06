using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Outbox;

namespace PaymentFlow.Notification.Worker;

public sealed class PaymentApprovedNotificationHandler(
    IOutboxMessageWriter outboxMessageWriter,
    ILogger<PaymentApprovedNotificationHandler> logger) : IIntegrationEventHandler
{
    public string EventType => EventNames.PaymentApproved;

    public string ConsumerName => "notification-worker";

    public Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        var paymentApproved = (PaymentApprovedEvent)integrationEvent;
        logger.LogInformation(
            "Dispatching approval notification for payment {PaymentId}; event {EventId}; correlation {CorrelationId}.",
            paymentApproved.PaymentId,
            paymentApproved.EventId,
            paymentApproved.CorrelationId);

        AddNotificationSent(paymentApproved.PaymentId, paymentApproved.CustomerId, paymentApproved.CorrelationId);
        return Task.CompletedTask;
    }

    private void AddNotificationSent(Guid paymentId, Guid customerId, Guid correlationId)
    {
        outboxMessageWriter.Add(
            new NotificationSentEvent(Guid.NewGuid(), correlationId, DateTimeOffset.UtcNow, paymentId, customerId, "email"),
            RoutingKeys.PaymentNotificationSent);
    }
}
