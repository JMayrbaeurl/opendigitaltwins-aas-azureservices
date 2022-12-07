using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtFile : AdtSubmodelElement
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }
    }
}
