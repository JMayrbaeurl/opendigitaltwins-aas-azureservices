using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtReferable : AdtBase
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("description")]
        public AdtLanguageString? Description { get; set; }

        [JsonPropertyName("displayName")]
        public AdtLanguageString? DisplayName { get; set; }

        [JsonPropertyName("checksum")]
        public string? Checksum { get; set; }

        [JsonPropertyName("idShort")]
        public string? IdShort { get; set; }
    }
}
