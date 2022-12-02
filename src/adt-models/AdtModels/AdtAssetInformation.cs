using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtAssetInformation
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string dtId { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; }
        [JsonPropertyName("globalAssetIdValue")]
        public string? GlobalAssetId { get; set; }

        [JsonPropertyName("specificAssetIdValues")]
        public string? SpecificAssetId { get; set; }

        [JsonPropertyName("assetKind")]
        public AdtAssetKind? AssetKind { get; set; }
    }

    public class AdtAssetKind
    {
        [JsonPropertyName("assetKind")]
        public string? AssetKind { get; set; }
    }
}
