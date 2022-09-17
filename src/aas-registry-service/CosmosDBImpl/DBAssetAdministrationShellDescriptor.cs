using AAS.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace AAS.API.Registry.CosmosDBImpl
{
    public class DBAssetAdministrationShellDescriptor
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("shellDesc")]
        public AssetAdministrationShellDescriptor Desc { get; set; }

        public DBAssetAdministrationShellDescriptor(AssetAdministrationShellDescriptor shellDesc)
        {
            Desc = shellDesc;

            if (shellDesc != null && shellDesc.Identification != null)
            {
                Id = CreateDocumentId(shellDesc.Identification);
            }  
        }

        public static string CreateDocumentId(string shellId)
        {
            return shellId?.Replace('/', '-').Replace('?', '_').Replace('#', '_');
        }
    }
}
