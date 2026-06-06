using PaymentFlow.Contracts;

namespace PaymentFlow.Audit.Worker.Services;

public interface IAuditService
{
    Task LogEventAsync(IntegrationEvent integrationEvent, string eventType, string routingKey, CancellationToken cancellationToken = default);
}
