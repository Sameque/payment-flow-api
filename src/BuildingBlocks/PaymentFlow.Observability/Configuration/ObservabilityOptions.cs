namespace PaymentFlow.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "PaymentFlow";

    public bool UseConsoleExporter { get; set; } = true;
}
