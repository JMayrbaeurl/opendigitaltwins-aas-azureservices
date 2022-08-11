using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public abstract class Referable
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string Id { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; } = new DigitalTwinMetadata();

        [JsonPropertyName("idShort")]
        public string IdShort { get; set; }

        [JsonPropertyName("displayName")]
        public LangStringSet DisplayName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("description")]
        public LangStringSet Description { get; set; }

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; }
    }
}
