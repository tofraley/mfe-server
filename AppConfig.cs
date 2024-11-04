
public record AppConfig
{
    public VersionInfo Versions { get; init; } = new();
    public Dictionary<string, string> Paths { get; init; } = [];
}