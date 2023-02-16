using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AAS.ADT.Models;

public class AdtAdministration : AdtBase
{
    [JsonPropertyName("revision")]
    public string? Revision { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}