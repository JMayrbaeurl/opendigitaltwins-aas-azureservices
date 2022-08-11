using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public class Tags
    {
        /// <summary>
        /// A component must have a property named $metadata with no client-supplied properties, to be distinguished from other properties as a component.
        /// </summary>
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public IDictionary<string, DigitalTwinPropertyMetadata> Metadata { get; set; } = new Dictionary<string, DigitalTwinPropertyMetadata>();

        [JsonPropertyName("markers")]
        public IDictionary<string, bool> Markers { get; set; }

        [JsonPropertyName("values")]
        public IDictionary<string,string> Values { get; set; }
    }
}
