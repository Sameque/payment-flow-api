namespace PaymentFlow.Messaging;

public sealed record MessageContext(
    string ConsumerName,
    string RoutingKey,
    string EventType,
    Guid MessageId,
    Guid CorrelationId);
