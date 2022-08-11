using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AAS.ADT.Models
{
    public abstract class Identifiable : Referable
    {
        [JsonPropertyName("administration")]
        public AdministrationInformation Administration { get; set; }

        [JsonPropertyName("identification")]
        public Identifier Identification { get; set; }
    }
}
