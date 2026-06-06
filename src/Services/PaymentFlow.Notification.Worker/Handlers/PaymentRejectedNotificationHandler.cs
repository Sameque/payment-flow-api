using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Outbox;

namespace PaymentFlow.Notification.Worker;

public sealed class PaymentRejectedNotificationHandler(
    IOutboxMessageWriter outboxMessageWriter,
    ILogger<PaymentRejectedNotificationHandler> logger) : IIntegrationEventHandler
{
    public string EventType => EventNames.PaymentRejected;

    public string ConsumerName => "notification-worker";

    public Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        var paymentRejected = (PaymentRejectedEvent)integrationEvent;
        logger.LogInformation(
            "Dispatching rejection notification for payment {PaymentId}; event {EventId}; correlation {CorrelationId}.",
            paymentRejected.PaymentId,
            paymentRejected.EventId,
            paymentRejected.CorrelationId);

        outboxMessageWriter.Add(
            new NotificationSentEvent(Guid.NewGuid(), paymentRejected.CorrelationId, DateTimeOffset.UtcNow, paymentRejected.PaymentId, paymentRejected.CustomerId, "email"),
            RoutingKeys.PaymentNotificationSent);

        return Task.CompletedTask;
    }
}
