using System.Text.Json.Serialization;

namespace AAS.ADT.Models;

public class AdtHasKind
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }
}