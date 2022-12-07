using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtBase
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string dtId { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("displayName")]
        public AdtLanguageString? DisplayName { get; set; }

        [JsonPropertyName("idShort")]
        public string? IdShort { get; set; }

        [JsonPropertyName("administration")]
        public AdtAdministration? Administration { get; set; }

        [JsonPropertyName("description")]
        public AdtLanguageString? Description { get; set; }
    }
}
