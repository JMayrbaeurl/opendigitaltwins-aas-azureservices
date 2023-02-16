using System.Collections.Generic;
using System.Text.Json.Serialization;
using Azure.DigitalTwins.Core;

namespace AAS.ADT.Models;

public class AdtLanguageString
{
    [JsonPropertyName("langString")] 
    public Dictionary<string, string>? LangStrings { get; set; }

    [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
    public DigitalTwinMetadata Metadata { get; set; }
}