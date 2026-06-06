using System.Text;
using System.Text.Json;
using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using RabbitMQ.Client;

namespace PaymentFlow.RabbitMq;

public sealed class RabbitMqIntegrationEventPublisher(
    IRabbitMqConnectionProvider connectionProvider,
    IIntegrationEventTypeRegistry eventTypeRegistry) : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task PublishAsync(IntegrationEvent integrationEvent, string routingKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using IModel channel = connectionProvider.CreateChannel();
        RabbitMqTopologyInitializer.DeclarePaymentTopology(channel);

        string eventType = eventTypeRegistry.GetEventName(integrationEvent.GetType());
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions));

        IBasicProperties properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = integrationEvent.EventId.ToString("D");
        properties.CorrelationId = integrationEvent.CorrelationId.ToString("D");
        properties.Type = eventType;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(
            RabbitMqTopology.PaymentExchange,
            routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }
}
