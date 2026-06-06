using System.Text.Json;
using PaymentFlow.Contracts;
using PaymentFlow.Persistence;

namespace PaymentFlow.Audit.Worker.Services;

public sealed class AuditService(PaymentDbContext dbContext, ILogger<AuditService> logger) : IAuditService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task LogEventAsync(IntegrationEvent integrationEvent, string eventType, string routingKey, CancellationToken cancellationToken = default)
    {
        Guid? paymentId = (integrationEvent as IPaymentEvent)?.PaymentId;

        await dbContext.AuditLogs.AddAsync(
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EventId = integrationEvent.EventId,
                CorrelationId = integrationEvent.CorrelationId,
                PaymentId = paymentId,
                EventType = eventType,
                RoutingKey = routingKey,
                Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions),
                CreatedAtUtc = DateTimeOffset.UtcNow
            },
            cancellationToken);

        logger.LogInformation(
            "Audit logged event {EventId} for payment {PaymentId}; correlation {CorrelationId}.",
            integrationEvent.EventId,
            paymentId,
            integrationEvent.CorrelationId);
    }
}
