namespace PaymentFlow.Messaging;

public sealed record IntegrationEventMetadata(
    string EventType,
    string RoutingKey,
    Guid EventId,
    Guid CorrelationId);
