using System.Text.Json;
using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Persistence;

namespace PaymentFlow.Audit.Worker;

public sealed class AuditIntegrationEventHandler(
    string eventType,
    PaymentDbContext dbContext,
    ILogger<AuditIntegrationEventHandler> logger) : IIntegrationEventHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string EventType { get; } = eventType;

    public string ConsumerName => "audit-worker";

    public static AuditIntegrationEventHandler Create(string eventType, IServiceProvider provider)
    {
        return new AuditIntegrationEventHandler(
            eventType,
            provider.GetRequiredService<PaymentDbContext>(),
            provider.GetRequiredService<ILogger<AuditIntegrationEventHandler>>());
    }

    public async Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        await dbContext.AuditLogs.AddAsync(
            new AuditLog
            {
                Id = Guid.NewGuid(),
                EventId = integrationEvent.EventId,
                CorrelationId = integrationEvent.CorrelationId,
                PaymentId = GetPaymentId(integrationEvent),
                EventType = context.EventType,
                RoutingKey = context.RoutingKey,
                Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions),
                CreatedAtUtc = DateTimeOffset.UtcNow
            },
            cancellationToken);

        logger.LogInformation(
            "Audit logged event {EventId} for payment {PaymentId}; correlation {CorrelationId}.",
            integrationEvent.EventId,
            GetPaymentId(integrationEvent),
            integrationEvent.CorrelationId);
    }

    private static Guid? GetPaymentId(IntegrationEvent integrationEvent)
    {
        return integrationEvent switch
        {
            PaymentCreatedEvent paymentCreated => paymentCreated.PaymentId,
            FraudApprovedEvent fraudApproved => fraudApproved.PaymentId,
            FraudRejectedEvent fraudRejected => fraudRejected.PaymentId,
            PaymentApprovedEvent paymentApproved => paymentApproved.PaymentId,
            PaymentRejectedEvent paymentRejected => paymentRejected.PaymentId,
            NotificationSentEvent notificationSent => notificationSent.PaymentId,
            _ => null
        };
    }
}
