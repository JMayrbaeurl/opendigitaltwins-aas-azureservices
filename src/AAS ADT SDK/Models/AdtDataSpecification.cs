using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AdtDataSpecification : AdtBase
    {
        [JsonPropertyName("administration")]
        public AdtAdministration Administration { get; set; }

        [JsonPropertyName("id")] 
        public string Id { get; set; }

        [JsonPropertyName("description")] 
        public AdtLanguageString Description { get; set; }
    }
}
