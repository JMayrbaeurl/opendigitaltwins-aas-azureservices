using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AdtModels.AdtModels
{
    public class AdtProperty : AdtSubmodelElement
    {
        

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("valueType")]
        public string? ValueType { get; set; }

        

        

    }
}
