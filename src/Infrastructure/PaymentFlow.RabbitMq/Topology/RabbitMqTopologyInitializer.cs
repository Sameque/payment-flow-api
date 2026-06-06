using PaymentFlow.Contracts;
using RabbitMQ.Client;

namespace PaymentFlow.RabbitMq;

public static class RabbitMqTopologyInitializer
{
    public static void DeclarePaymentTopology(IModel channel)
    {
        channel.ExchangeDeclare(RabbitMqTopology.PaymentExchange, ExchangeType.Topic, durable: true, autoDelete: false);
        channel.ExchangeDeclare(RabbitMqTopology.DeadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false);

        DeclareQueue(
            channel,
            RabbitMqTopology.FraudQueue,
            RabbitMqTopology.FraudDeadLetterQueue,
            [RoutingKeys.PaymentTransactionCreated]);

        DeclareQueue(
            channel,
            RabbitMqTopology.ProcessorQueue,
            RabbitMqTopology.ProcessorDeadLetterQueue,
            [RoutingKeys.PaymentFraudApproved]);

        DeclareQueue(
            channel,
            RabbitMqTopology.NotificationQueue,
            RabbitMqTopology.NotificationDeadLetterQueue,
            [RoutingKeys.PaymentProcessorApproved, RoutingKeys.PaymentProcessorRejected, RoutingKeys.PaymentFraudRejected]);

        DeclareQueue(
            channel,
            RabbitMqTopology.AuditQueue,
            RabbitMqTopology.AuditDeadLetterQueue,
            ["payment.#"]);
    }

    private static void DeclareQueue(IModel channel, string queueName, string deadLetterQueueName, string[] bindings)
    {
        channel.QueueDeclare(deadLetterQueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(deadLetterQueueName, RabbitMqTopology.DeadLetterExchange, deadLetterQueueName);

        var arguments = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = RabbitMqTopology.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = deadLetterQueueName
        };

        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments);

        foreach (string binding in bindings)
        {
            channel.QueueBind(queueName, RabbitMqTopology.PaymentExchange, binding);
        }
    }
}
