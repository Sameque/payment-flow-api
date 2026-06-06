namespace PaymentFlow.Persistence;

public sealed class ProcessedMessage
{
    public Guid MessageId { get; set; }

    public string ConsumerName { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAtUtc { get; set; }
}
