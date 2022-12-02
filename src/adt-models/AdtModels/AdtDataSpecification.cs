using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtDataSpecification
    {
        [JsonPropertyName("dataType")] 
        public string? DataType { get; set; }

        [JsonPropertyName("levelType")]
        public string? LevelType { get; set; }

        [JsonPropertyName("sourceOfDefinition")]
        public string? SourceOfDefinition { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("unitIdValue")]
        public string? UnitIdValue { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("preferredName")]
        public AdtLanguageString? PreferredName { get; set; }

        [JsonPropertyName("shortName")]
        public AdtLanguageString? ShortName { get; set; }

        [JsonPropertyName("definition")]
        public AdtLanguageString? Definition { get; set; }
    }
}
