using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AdtModels.AdtModels
{
    public class AdtAas : AdtBase
    {

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        // derivedFrom does not exist in the ADT Response

        [JsonPropertyName("assetInformationShort")]
        public AssetInformationShortAdt AssetInformation { get; set; }

        [JsonPropertyName("checksum")]
        public string? Checksum { get; set; }

        

        

        // Embedded Data Spesification does not exist in ADT Response

        

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
