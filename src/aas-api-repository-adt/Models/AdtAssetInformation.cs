using System.Text.Json.Serialization;

namespace AAS.API.Repository.Adt.Models
{
    public class AdtAssetInformation : AdtBase
    {
        
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
