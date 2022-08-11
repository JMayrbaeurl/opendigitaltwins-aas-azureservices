using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AssetInformation
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string Id { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; } = new DigitalTwinMetadata();

        [JsonPropertyName("assetKind")]
        public AssetKind Kind { get; set; }
    }
}
