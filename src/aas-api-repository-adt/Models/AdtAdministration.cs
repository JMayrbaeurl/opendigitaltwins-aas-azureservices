using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt.Models;

public class AdtAdministration
{
    [JsonPropertyName("revision")]
    public string? Revision { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
    public DigitalTwinMetadata Metadata { get; set; }
}