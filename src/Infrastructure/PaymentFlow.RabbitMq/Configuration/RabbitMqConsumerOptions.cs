namespace PaymentFlow.RabbitMq;

public sealed class RabbitMqConsumerOptions
{
    public string QueueName { get; set; } = string.Empty;

    public string DeadLetterQueueName { get; set; } = string.Empty;

    public string[] Bindings { get; set; } = [];

    public string ConsumerName { get; set; } = string.Empty;
}
