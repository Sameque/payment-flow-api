namespace PaymentFlow.Outbox;

public sealed class OutboxDispatcherOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; set; } = 25;

    public int PollIntervalSeconds { get; set; } = 5;
}
