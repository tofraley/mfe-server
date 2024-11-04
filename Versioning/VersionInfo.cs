public record VersionInfo
{
    public string Default { get; init; } = "";
    public Dictionary<string, VariantConfig> Variants { get; init; } = [];
}