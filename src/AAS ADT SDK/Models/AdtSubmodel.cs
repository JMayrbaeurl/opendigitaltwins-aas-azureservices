using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AdtSubmodel : AdtIdentifiable
    {
        [JsonPropertyName("kind")]
        public AdtHasKind Kind { get; set; }

    }
}
