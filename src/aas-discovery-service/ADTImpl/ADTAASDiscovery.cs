using AAS.API.Models;
using AAS.API.Services.ADT;
using Azure;
using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AAS.API.Discovery
{
    public class ADTAASDiscovery : AbstractADTAASService, AASDiscovery
    {
        public ADTAASDiscovery(DigitalTwinsClient client) : base(client)
        {
        }

        public async Task<List<string>> GetAllAssetAdministrationShellIdsByAssetLink(List<IdentifierKeyValuePair> assetIds)
        {
            List<string> result = new List<string>();

            try
            {
                if (HasGlobalAssetId(assetIds))
                {
                    List<string> globalAssetIdKeyDTID = await FindDTIdForGlobalAssetId(GetGlobalAssetId(assetIds));
                    string queryString = $"SELECT aas FROM digitaltwins MATCH(aas) -[:assetInformation]->(aasinfo) -[:globalAssetId]->(ref) -[:key]->(key) where key.$dtId IN {ConvertStringListToQueryArrayString(globalAssetIdKeyDTID)}";
                    AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                    await foreach (BasicDigitalTwin twin in twins)
                    {
                        JsonElement propFromADT;
                        if (((JsonElement)twin.Contents["aas"]).TryGetProperty("identification", out propFromADT))
                        {
                            result.Add(propFromADT.GetProperty("id").GetString());
                        }  
                    }
                }
            }
            catch (RequestFailedException exc)
            {
                //log.LogError($"*** Error in retrieving parent:{exc.Status}/{exc.Message}");
                throw new AASDiscoveryException($"*** Error in retrieving AAS:{exc.Status}/{exc.Message}");
            }

            return result;
        }
    }
}
