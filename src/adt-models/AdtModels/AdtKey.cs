using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtKey
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}
