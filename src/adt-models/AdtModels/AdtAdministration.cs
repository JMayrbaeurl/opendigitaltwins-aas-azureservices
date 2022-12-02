using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AdtModels.AdtModels;

public class AdtAdministration
{
    [JsonPropertyName("revision")]
    public string? Revision { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
    public DigitalTwinMetadata Metadata { get; set; }
}