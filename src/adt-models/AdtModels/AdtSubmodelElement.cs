using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtSubmodelElement : AdtReferable
    {
        [JsonPropertyName("kind")]
        public AdtHasKind? Kind { get; set; }

        [JsonPropertyName("semanticIdValue")]
        public string? SemanticIdValue { get; set; }
    }
}
