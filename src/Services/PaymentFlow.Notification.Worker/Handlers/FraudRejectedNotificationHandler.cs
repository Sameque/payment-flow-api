using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Outbox;

namespace PaymentFlow.Notification.Worker;

public sealed class FraudRejectedNotificationHandler(
    IOutboxMessageWriter outboxMessageWriter,
    ILogger<FraudRejectedNotificationHandler> logger) : IIntegrationEventHandler
{
    public string EventType => EventNames.FraudRejected;

    public string ConsumerName => "notification-worker";

    public Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        var fraudRejected = (FraudRejectedEvent)integrationEvent;
        logger.LogInformation(
            "Dispatching fraud rejection notification for payment {PaymentId}; event {EventId}; correlation {CorrelationId}.",
            fraudRejected.PaymentId,
            fraudRejected.EventId,
            fraudRejected.CorrelationId);

        outboxMessageWriter.Add(
            new NotificationSentEvent(Guid.NewGuid(), fraudRejected.CorrelationId, DateTimeOffset.UtcNow, fraudRejected.PaymentId, fraudRejected.CustomerId, "email"),
            RoutingKeys.PaymentNotificationSent);

        return Task.CompletedTask;
    }
}
