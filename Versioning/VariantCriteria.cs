using System.Text.Json.Serialization;
public record VariantCriteria
{
    [JsonPropertyName("query_param")]
    public string? QueryParam { get; init; }
    public string? Cookie { get; init; }
    [JsonPropertyName("user_groups")]
    public string[]? UserGroups { get; init; }
}