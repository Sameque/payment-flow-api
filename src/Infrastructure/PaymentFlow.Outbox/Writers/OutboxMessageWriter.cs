using System.Text.Json;
using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Persistence;

namespace PaymentFlow.Outbox;

public sealed class OutboxMessageWriter(
    PaymentDbContext dbContext,
    IIntegrationEventTypeRegistry eventTypeRegistry) : IOutboxMessageWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Add(IntegrationEvent integrationEvent, string routingKey)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var message = new OutboxMessage
        {
            Id = integrationEvent.EventId,
            EventType = eventTypeRegistry.GetEventName(integrationEvent.GetType()),
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions),
            Processed = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.OutboxMessages.Add(message);
    }
}
