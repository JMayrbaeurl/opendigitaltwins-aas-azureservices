using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AdtFile : AdtSubmodelElement
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }
    }
}
