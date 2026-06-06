using PaymentFlow.Contracts;

namespace PaymentFlow.Messaging;

public sealed class IntegrationEventTypeRegistry : IIntegrationEventTypeRegistry
{
    private static readonly Dictionary<string, Type> EventTypes = new(StringComparer.Ordinal)
    {
        [PaymentFlow.Contracts.EventNames.PaymentCreated] = typeof(PaymentCreatedEvent),
        [PaymentFlow.Contracts.EventNames.FraudApproved] = typeof(FraudApprovedEvent),
        [PaymentFlow.Contracts.EventNames.FraudRejected] = typeof(FraudRejectedEvent),
        [PaymentFlow.Contracts.EventNames.PaymentApproved] = typeof(PaymentApprovedEvent),
        [PaymentFlow.Contracts.EventNames.PaymentRejected] = typeof(PaymentRejectedEvent),
        [PaymentFlow.Contracts.EventNames.NotificationSent] = typeof(NotificationSentEvent)
    };

    public IReadOnlyCollection<string> EventNames => EventTypes.Keys;

    public string GetEventName(Type eventType)
    {
        if (!typeof(IntegrationEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException($"Type {eventType.Name} is not an integration event.", nameof(eventType));
        }

        string? eventName = EventTypes.FirstOrDefault(pair => pair.Value == eventType).Key;
        return eventName ?? throw new InvalidOperationException($"Integration event type {eventType.Name} is not registered.");
    }

    public Type GetEventType(string eventName)
    {
        if (EventTypes.TryGetValue(eventName, out Type? eventType))
        {
            return eventType;
        }

        throw new InvalidOperationException($"Integration event '{eventName}' is not registered.");
    }
}
