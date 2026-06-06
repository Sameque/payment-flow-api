namespace PaymentFlow.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string ConnectionString { get; set; } = string.Empty;
}
