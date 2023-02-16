using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AdtKey
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}
