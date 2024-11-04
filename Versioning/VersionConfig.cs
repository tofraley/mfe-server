
public record VersionConfig
{
    public Dictionary<string, AppConfig> Apps { get; init; } = [];
}