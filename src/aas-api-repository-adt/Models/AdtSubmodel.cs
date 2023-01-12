using System.Text.Json.Serialization;

namespace AAS.API.Repository.Adt.Models
{
    public class AdtSubmodel : AdtIdentifiable
    {
        [JsonPropertyName("kind")]
        public AdtHasKind Kind { get; set; }

    }
}
