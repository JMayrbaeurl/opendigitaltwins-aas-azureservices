using System.Text.Json.Serialization;

namespace AdtModels.AdtModels;

public class AdtHasKind
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }
}