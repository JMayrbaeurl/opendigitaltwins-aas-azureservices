using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AdtProperty : AdtSubmodelElement
    {
        

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("valueType")]
        public string? ValueType { get; set; }

        

        

    }
}
