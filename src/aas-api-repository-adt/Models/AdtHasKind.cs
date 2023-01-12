using System.Text.Json.Serialization;

namespace AAS.API.Repository.Adt.Models;

public class AdtHasKind
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }
}