using System.Text.Json.Serialization;

namespace AAS.API.Repository.Adt.Models
{
    public class AdtProperty : AdtSubmodelElement
    {
        

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("valueType")]
        public string? ValueType { get; set; }

        

        

    }
}
