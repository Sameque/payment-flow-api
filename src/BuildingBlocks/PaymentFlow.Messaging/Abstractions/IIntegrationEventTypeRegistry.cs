using PaymentFlow.Contracts;

namespace PaymentFlow.Messaging;

public interface IIntegrationEventTypeRegistry
{
    string GetEventName(Type eventType);

    Type GetEventType(string eventName);

    IReadOnlyCollection<string> EventNames { get; }
}
