using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AAS.ADT.Models
{
    public class AdtBase
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string dtId { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; }
    }
}
