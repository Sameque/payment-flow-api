using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentFlow.RabbitMq;

public sealed class RabbitMqConsumerBackgroundService(
    IRabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqConsumerOptions> options,
    ILogger<RabbitMqConsumerBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan[] RetryBackoff =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(45)
    ];

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly RabbitMqConsumerOptions options = options.Value;
    private IModel? channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        channel = connectionProvider.CreateChannel();
        RabbitMqTopologyInitializer.DeclarePaymentTopology(channel);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += ProcessReceivedMessageAsync;

        channel.BasicConsume(options.QueueName, autoAck: false, consumer);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override void Dispose()
    {
        channel?.Dispose();
        base.Dispose();
    }

    private async Task ProcessReceivedMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (channel is null)
        {
            return;
        }

        for (int attempt = 0; attempt <= RetryBackoff.Length; attempt++)
        {
            try
            {
                await DispatchAsync(args, CancellationToken.None);
                channel.BasicAck(args.DeliveryTag, multiple: false);
                return;
            }
            catch (Exception exception) when (attempt < RetryBackoff.Length)
            {
                logger.LogWarning(
                    exception,
                    "Message {MessageId} failed on attempt {Attempt}; retrying after {DelaySeconds}s.",
                    args.BasicProperties.MessageId,
                    attempt + 1,
                    RetryBackoff[attempt].TotalSeconds);

                await Task.Delay(RetryBackoff[attempt]);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Message {MessageId} exceeded retry policy and will be dead-lettered.",
                    args.BasicProperties.MessageId);

                channel.BasicNack(args.DeliveryTag, multiple: false, requeue: false);
                return;
            }
        }
    }

    private async Task DispatchAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        string eventTypeName = args.BasicProperties.Type;
        string payload = Encoding.UTF8.GetString(args.Body.ToArray());

        if (!Guid.TryParse(args.BasicProperties.MessageId, out Guid messageId))
        {
            throw new InvalidOperationException("RabbitMQ message is missing a valid MessageId.");
        }

        if (!Guid.TryParse(args.BasicProperties.CorrelationId, out Guid correlationId))
        {
            correlationId = messageId;
        }

        using IServiceScope scope = scopeFactory.CreateScope();
        var eventTypeRegistry = scope.ServiceProvider.GetRequiredService<IIntegrationEventTypeRegistry>();
        Type eventType = eventTypeRegistry.GetEventType(eventTypeName);
        var integrationEvent = (IntegrationEvent?)JsonSerializer.Deserialize(payload, eventType, SerializerOptions);

        if (integrationEvent is null)
        {
            throw new InvalidOperationException($"Message {messageId} could not be deserialized as {eventTypeName}.");
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        IEnumerable<IIntegrationEventHandler> handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler>()
            .Where(handler => handler.EventType == eventTypeName);

        bool handled = false;
        foreach (IIntegrationEventHandler handler in handlers)
        {
            handled = true;
            bool alreadyProcessed = await dbContext.ProcessedMessages.AnyAsync(
                message => message.MessageId == messageId && message.ConsumerName == handler.ConsumerName,
                cancellationToken);

            if (alreadyProcessed)
            {
                logger.LogInformation(
                    "Skipping duplicate message {EventId} for consumer {ConsumerName}.",
                    messageId,
                    handler.ConsumerName);
                continue;
            }

            var context = new MessageContext(
                handler.ConsumerName,
                args.RoutingKey,
                eventTypeName,
                messageId,
                correlationId);

            await handler.HandleAsync(integrationEvent, context, cancellationToken);

            await dbContext.ProcessedMessages.AddAsync(
                new ProcessedMessage
                {
                    MessageId = messageId,
                    ConsumerName = handler.ConsumerName,
                    ProcessedAtUtc = DateTimeOffset.UtcNow
                },
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!handled)
        {
            throw new InvalidOperationException($"No handler registered for integration event {eventTypeName}.");
        }
    }
}
