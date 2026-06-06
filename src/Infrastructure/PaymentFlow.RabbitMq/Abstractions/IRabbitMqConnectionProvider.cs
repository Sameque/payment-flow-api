using RabbitMQ.Client;

namespace PaymentFlow.RabbitMq;

public interface IRabbitMqConnectionProvider
{
    IModel CreateChannel();
}
