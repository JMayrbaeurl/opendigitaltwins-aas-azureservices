using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AdtReference : AdtBase
    {

        [JsonPropertyName("key1")]
        public AdtKey? Key1 { get; set; }

        [JsonPropertyName("key2")]
        public AdtKey? Key2 { get; set; }

        [JsonPropertyName("key3")]
        public AdtKey? Key3 { get; set; }

        [JsonPropertyName("key4")]
        public AdtKey? Key4 { get; set; }

        [JsonPropertyName("key5")]
        public AdtKey? Key5 { get; set; }

        [JsonPropertyName("key6")]
        public AdtKey? Key6 { get; set; }

        [JsonPropertyName("key7")]
        public AdtKey? Key7 { get; set; }

        [JsonPropertyName("key8")]
        public AdtKey? Key8 { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
