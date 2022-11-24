using AAS.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Registry.CosmosDBImpl
{
    public class DBSubmodelDescriptor
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("submodelDesc")]
        public SubmodelDescriptor Desc { get; set; }

        public DBSubmodelDescriptor(SubmodelDescriptor submodelDesc)
        {
            Desc = submodelDesc;

            if (submodelDesc != null && submodelDesc.Identification != null)
            {
                Id = CreateDocumentId(submodelDesc.Identification);
            }
        }

        public static string CreateDocumentId(string shellId)
        {
            return shellId?.Replace('/', '-').Replace('?', '_').Replace('#', '_');
        }
    }
}
