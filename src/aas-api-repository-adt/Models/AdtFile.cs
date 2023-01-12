using System.Text.Json.Serialization;

namespace AAS.API.Repository.Adt.Models
{
    public class AdtFile : AdtSubmodelElement
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }
    }
}
