namespace PaymentFlow.Persistence;

public sealed class AuditLog
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Guid CorrelationId { get; set; }

    public Guid? PaymentId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string RoutingKey { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
}
