using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AdtModels.AdtModels
{
    public class AdtAas : AdtIdentifiable
    {
        [JsonPropertyName("assetInformationShort")]
        public AssetInformationShortAdt AssetInformation { get; set; }
    }

    public class AssetInformationShortAdt
    {
        [JsonPropertyName("assetKind")]
        public string? AssetKind { get; set; }

        [JsonPropertyName("globalAssetId")]
        public string? GlobalAssetId { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; }
    }
}
