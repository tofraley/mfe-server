

public record VariantConfig
{
    public string Version { get; init; } = "";
    public int Weight { get; init; }
    public VariantCriteria Criteria { get; init; } = new();
}