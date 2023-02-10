using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt.Models
{
    public class AdtAas : AdtIdentifiable
    {
        [JsonPropertyName("assetInformationShort")]
        public AdtAssetInformationShort AssetInformation { get; set; }
    }

    public class AdtAssetInformationShort
    {
        [JsonPropertyName("assetKind")]
        public string? AssetKind { get; set; }

        [JsonPropertyName("globalAssetId")]
        public string? GlobalAssetId { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetadata Metadata { get; set; }
    }
}
