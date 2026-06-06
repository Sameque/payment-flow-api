using System.Text.Json;
using PaymentFlow.Contracts;
using PaymentFlow.Messaging;
using PaymentFlow.Audit.Worker.Services;

namespace PaymentFlow.Audit.Worker;

public sealed class AuditIntegrationEventHandler(
    string eventType,
    IAuditService auditService,
    ILogger<AuditIntegrationEventHandler> logger) : IIntegrationEventHandler
{
    //private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string EventType { get; } = eventType;

    public string ConsumerName => "audit-worker";

    public static AuditIntegrationEventHandler Create(string eventType, IServiceProvider provider)
    {
        return new AuditIntegrationEventHandler(
            eventType,
            provider.GetRequiredService<IAuditService>(),
            provider.GetRequiredService<ILogger<AuditIntegrationEventHandler>>());
    }

    public async Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken)
    {
        await auditService.LogEventAsync(integrationEvent, context.EventType, context.RoutingKey, cancellationToken);
    }
}
