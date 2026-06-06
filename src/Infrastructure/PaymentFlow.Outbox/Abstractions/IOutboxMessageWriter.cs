using PaymentFlow.Contracts;

namespace PaymentFlow.Outbox;

public interface IOutboxMessageWriter
{
    void Add(IntegrationEvent integrationEvent, string routingKey);
}
