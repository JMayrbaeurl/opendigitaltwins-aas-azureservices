using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class AssetKind
    {
        /// <summary>
        /// A component must have a property named $metadata with no client-supplied properties, to be distinguished from other properties as a component.
        /// </summary>
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public IDictionary<string, DigitalTwinPropertyMetadata> Metadata { get; set; } = new Dictionary<string, DigitalTwinPropertyMetadata>();

        [JsonPropertyName("assetKind")]
        public string kind { get; set; }
    }
}
