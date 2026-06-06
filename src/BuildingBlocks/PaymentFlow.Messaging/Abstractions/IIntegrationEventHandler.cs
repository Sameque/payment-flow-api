using PaymentFlow.Contracts;

namespace PaymentFlow.Messaging;

public interface IIntegrationEventHandler
{
    string EventType { get; }

    string ConsumerName { get; }

    Task HandleAsync(IntegrationEvent integrationEvent, MessageContext context, CancellationToken cancellationToken);
}
