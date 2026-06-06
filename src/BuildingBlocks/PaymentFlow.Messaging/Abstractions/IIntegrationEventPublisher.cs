using PaymentFlow.Contracts;

namespace PaymentFlow.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationEvent integrationEvent, string routingKey, CancellationToken cancellationToken);
}
