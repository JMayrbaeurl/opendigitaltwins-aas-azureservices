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
                // First check if we should find with global Asset IDs
                if (HasGlobalAssetId(assetIds))
                {
                    List<string> globalAssetIdKeyDTID = await FindDTIdForGlobalAssetId(GetGlobalAssetId(assetIds));
                    if (globalAssetIdKeyDTID != null && globalAssetIdKeyDTID.Count > 0)
                    { 
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

                // Next use the specific Asset IDs
                List<IdentifierKeyValuePair> specificAssetIds = GetSpecificAssetIds(assetIds);
                if (specificAssetIds != null && specificAssetIds.Count > 0)
                {
                    List<string> specificAssetIdKeyDTID = await FindDTIdForSpecificAssetId(specificAssetIds);
                    if (specificAssetIdKeyDTID != null && specificAssetIdKeyDTID.Count > 0)
                    {
                        string queryString = $"SELECT aas FROM digitaltwins MATCH(aas) -[:assetInformation]->(aasinfo) -[:specificAssetId]->(assetId) where assetId.$dtId IN {ConvertStringListToQueryArrayString(specificAssetIdKeyDTID)}";
                        AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                        await foreach (BasicDigitalTwin twin in twins)
                        {
                            JsonElement propFromADT;
                            if (((JsonElement)twin.Contents["aas"]).TryGetProperty("identification", out propFromADT))
                            {
                                string idstring = propFromADT.GetProperty("id").GetString();
                                if (idstring != null && !result.Contains(idstring))
                                    result.Add(idstring);
                            }
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
