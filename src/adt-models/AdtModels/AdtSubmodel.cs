using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AAS.API.Models;

namespace AdtModels.AdtModels
{
    public class AdtSubmodel : AdtIdentifiable
    {
        [JsonPropertyName("kind")]
        public AdtHasKind Kind { get; set; }

    }
}
