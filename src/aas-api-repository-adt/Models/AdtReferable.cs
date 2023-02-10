using System.Text.Json.Serialization;

namespace AAS.API.Repository.Adt.Models
{
    public class AdtReferable : AdtBase
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("description")]
        public AdtLanguageString? Description { get; set; }

        [JsonPropertyName("displayName")]
        public AdtLanguageString? DisplayName { get; set; }

        [JsonPropertyName("checksum")]
        public string? Checksum { get; set; }

        [JsonPropertyName("idShort")]
        public string? IdShort { get; set; }
    }
}
