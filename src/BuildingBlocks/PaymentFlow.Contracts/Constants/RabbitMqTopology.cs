namespace PaymentFlow.Contracts;

public static class RabbitMqTopology
{
    public const string PaymentExchange = "pf.payment.exchange";
    public const string DeadLetterExchange = "pf.payment.exchange.dlx";

    public const string FraudQueue = "pf.payment.fraud.queue";
    public const string FraudDeadLetterQueue = "pf.payment.fraud.dlq";

    public const string ProcessorQueue = "pf.payment.processor.queue";
    public const string ProcessorDeadLetterQueue = "pf.payment.processor.dlq";

    public const string NotificationQueue = "pf.payment.notification.queue";
    public const string NotificationDeadLetterQueue = "pf.payment.notification.dlq";

    public const string AuditQueue = "pf.payment.audit.queue";
    public const string AuditDeadLetterQueue = "pf.payment.audit.dlq";
}
