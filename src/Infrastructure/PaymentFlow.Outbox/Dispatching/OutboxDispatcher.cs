using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Persistence;

namespace PaymentFlow.Outbox;

public sealed class OutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxDispatcherOptions> options,
    ILogger<OutboxDispatcher> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly OutboxDispatcherOptions options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox dispatch cycle failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(options.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task DispatchPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
        var eventTypeRegistry = scope.ServiceProvider.GetRequiredService<IIntegrationEventTypeRegistry>();

        List<OutboxMessage> messages = await dbContext.OutboxMessages
            .Where(message => !message.Processed)
            .OrderBy(message => message.CreatedAtUtc)
            .Take(options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (OutboxMessage message in messages)
        {
            try
            {
                Type eventType = eventTypeRegistry.GetEventType(message.EventType);
                var integrationEvent = (IntegrationEvent?)JsonSerializer.Deserialize(message.Payload, eventType, SerializerOptions);

                if (integrationEvent is null)
                {
                    throw new InvalidOperationException($"Outbox message {message.Id} could not be deserialized.");
                }

                await publisher.PublishAsync(integrationEvent, message.RoutingKey, cancellationToken);

                message.Processed = true;
                message.ProcessedAtUtc = DateTimeOffset.UtcNow;
                message.Error = null;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                message.Error = exception.Message;
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogError(
                    exception,
                    "Failed to dispatch outbox message {EventId} with routing key {RoutingKey}.",
                    message.Id,
                    message.RoutingKey);
            }
        }
    }
}
