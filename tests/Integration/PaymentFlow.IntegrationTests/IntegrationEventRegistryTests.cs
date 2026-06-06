using PaymentFlow.Contracts;
using PaymentFlow.Messaging;

namespace PaymentFlow.IntegrationTests;

public sealed class IntegrationEventRegistryTests
{
    [Fact]
    public void Registry_contains_required_event_contracts()
    {
        var registry = new IntegrationEventTypeRegistry();

        Assert.Equal(typeof(PaymentCreatedEvent), registry.GetEventType(EventNames.PaymentCreated));
        Assert.Equal(typeof(FraudApprovedEvent), registry.GetEventType(EventNames.FraudApproved));
        Assert.Equal(typeof(FraudRejectedEvent), registry.GetEventType(EventNames.FraudRejected));
        Assert.Equal(typeof(PaymentApprovedEvent), registry.GetEventType(EventNames.PaymentApproved));
        Assert.Equal(typeof(PaymentRejectedEvent), registry.GetEventType(EventNames.PaymentRejected));
        Assert.Equal(typeof(NotificationSentEvent), registry.GetEventType(EventNames.NotificationSent));
    }
}
