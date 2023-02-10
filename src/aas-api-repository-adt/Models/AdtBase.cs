using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt.Models
{
    public class AdtBase
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string dtId { get; set; }
    }
}
