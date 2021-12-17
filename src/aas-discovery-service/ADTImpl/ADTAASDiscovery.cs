using AAS.API.Models;
using AAS.API.Services.ADT;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
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

        public ADTAASDiscovery(DigitalTwinsClientFactory dtClientFactory, ILogger<ADTAASDiscovery> logger) : base(logger)
        {
            dtClient = dtClientFactory.CreateClient();
        }

        public async Task<List<string>> GetAllAssetAdministrationShellIdsByAssetLink(List<IdentifierKeyValuePair> assetIds)
        {
            List<string> result = new List<string>();

            _logger.LogDebug($"GetAllAssetAdministrationShellIdsByAssetLink(assetIds='{assetIds}') called");

            try
            {
                // First check if we should find with global Asset IDs
                if (HasGlobalAssetId(assetIds))
                {
                    List<string> globalAssetIdKeyDTID = await FindDTIdForGlobalAssetId(GetGlobalAssetId(assetIds));
                    if (globalAssetIdKeyDTID != null && globalAssetIdKeyDTID.Count > 0)
                    {
                        string queryString = $"SELECT aas FROM digitaltwins MATCH(aas) -[:assetInformation]->(aasinfo) -[:globalAssetId]->(ref) -[:key]->(key) where key.$dtId IN {ConvertStringListToQueryArrayString(globalAssetIdKeyDTID)}";
                        _logger.LogDebug($"Querying for AASs in ADT with global Asset Ids: {queryString}");

                        AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                        await foreach (BasicDigitalTwin twin in twins)
                        {
                            JsonElement propFromADT;
                            if (((JsonElement)twin.Contents["aas"]).TryGetProperty("identification", out propFromADT))
                            {
                                _logger.LogDebug($"Found AAS with global Asset Id: {twin}");
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
                        _logger.LogDebug($"Querying for AASs in ADT with specific Asset Ids: {queryString}");

                        AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                        await foreach (BasicDigitalTwin twin in twins)
                        {
                            JsonElement propFromADT;
                            if (((JsonElement)twin.Contents["aas"]).TryGetProperty("identification", out propFromADT))
                            {
                                string idstring = propFromADT.GetProperty("id").GetString();
                                if (idstring != null && !result.Contains(idstring))
                                {
                                    _logger.LogDebug($"Found AAS with specific Asset Id: {twin}");
                                    result.Add(idstring);
                                }
                            }
                        }
                    }
                }
            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");

                throw new AASDiscoveryException($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        public async Task<List<IdentifierKeyValuePair>> GetAllAssetLinksById(string aasIdentifier)
        {
            List<IdentifierKeyValuePair> result = new List<IdentifierKeyValuePair>();

            _logger.LogDebug($"GetAllAssetLinksById(aasIdentifier='{aasIdentifier}') called");

            try
            {
                string dtID = await FindDTIdForIdentification(aasIdentifier);
                if (dtID != null && dtID.Length > 0)
                {
                    // First get the Global Asset Id from the Asset information
                    string queryString = $"SELECT key FROM digitaltwins MATCH (aas)-[:assetInformation]->(aasinfo)-[:globalAssetId]->(ref)-[:key]->(key) where aas.$dtId = '{dtID}'";
                    _logger.LogDebug($"Querying for global Asset id in ADT with: {queryString}");

                    AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                    await foreach (BasicDigitalTwin twin in twins)
                    {
                        JsonElement keyTwin = (JsonElement)twin.Contents["key"];
                        JsonElement propFromADT;
                        if (keyTwin.TryGetProperty("value", out propFromADT))
                        {
                            IdentifierKeyValuePair link = new IdentifierKeyValuePair() { Value = propFromADT.GetString(), Key = "globalAssetId" };
                            if (!result.Exists(id => (id.Key == link.Key && id.Value == link.Value)))
                            {
                                _logger.LogDebug($"Found global Asset id in ADT for AAS with dtId='{dtID}': '{link.Value}'");

                                result.Add(link);
                            }
                        }
                        else
                        {
                            // Inconsistent Twin setup. 'value' is mandatory! At least log an error message
                            _logger.LogError($"*** Error - Inconsistent Twin setup. IdentifierKeyValuePair for globalAssetId has null value (Twin dtId: {dtID})");
                        }
                    }

                    // Second get all Specific Asset Ids
                    queryString = $"SELECT assetId FROM digitaltwins MATCH(aas) -[:assetInformation]->(aasinfo) -[:specificAssetId]->(assetId) where aas.$dtId = '{dtID}'";
                    _logger.LogDebug($"Querying for specific Asset ids in ADT with: {queryString}");

                    twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                    await foreach (BasicDigitalTwin twin in twins)
                    {
                        JsonElement keyTwin = (JsonElement)twin.Contents["assetId"];
                        JsonElement keyFromADT, valueFromADT;
                        if (keyTwin.TryGetProperty("key", out keyFromADT) && keyTwin.TryGetProperty("value", out valueFromADT))
                        {
                            IdentifierKeyValuePair link = new IdentifierKeyValuePair() { Value = valueFromADT.GetString(), Key = keyFromADT.GetString() };
                            JsonElement semanticIdForADT;
                            if (keyTwin.TryGetProperty("semanticId", out semanticIdForADT))
                                link.SemanticId = new GlobalReference() { Value = new List<string>() { semanticIdForADT.GetString() } };
                            // TODO: SubjectID missing
                            if (!result.Exists(id => (id.Key == link.Key && id.Value == link.Value)))
                            {
                                _logger.LogDebug($"Found specific Asset id in ADT for AAS with dtId='{dtID}': key='{link.Key}', value='{link.Value}'");
                                result.Add(link);
                            }
                        }
                        else
                        {
                            // Inconsistent Twin setup. 'key' and 'value' are mandatory! At least log an error message
                            _logger.LogError($"*** Error - Inconsistent Twin setup. IdentifierKeyValuePair for globalAssetId has null for key or value: {twin})");
                        }
                    }

                }
            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");

                throw new AASDiscoveryException($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        public async Task<List<IdentifierKeyValuePair>> CreateAllAssetLinksById(string aasIdentifier, List<IdentifierKeyValuePair> assetIds)
        {
            List<IdentifierKeyValuePair> result = new List<IdentifierKeyValuePair>();

            _logger.LogDebug($"CreateAllAssetLinksById(assetIds='{assetIds}') called");

            if (aasIdentifier == null || aasIdentifier.Length == 0)
            {
                _logger.LogError($"*** Error - Parameter 'aasIdentifier' must not be empty");
            }

            try
            {
                string aasDtId = await FindDTIdForIdentification(aasIdentifier);
                if (aasDtId == null || aasDtId.Length == 0)
                {
                    _logger.LogInformation($"Can't find AAS with identifier '{aasIdentifier}'");
                    return result;
                }

                string aasInfoDtId = await FindInfoDTIdForIdentification(aasIdentifier);
                if (aasInfoDtId == null || aasInfoDtId.Length == 0)
                {
                    _logger.LogDebug($"Can't find AAS Info Twin for AAS with identifier '{aasIdentifier}'. Please create the according AAS Info Twin first");
                    return result;
                }

                if (assetIds != null && assetIds.Count > 0)
                {
                    foreach (IdentifierKeyValuePair newAssetId in assetIds)
                    {
                        if (newAssetId.Key == "globalAssetId")
                        {
                            await CreateOrReplaceGlobalAssetIDForShell(aasDtId, newAssetId.Value, aasInfoDtId);
                            _logger.LogDebug($"Created global Asset Id '{newAssetId}' for AAS with identifier '{aasIdentifier}'");
                        }
                        else
                        {
                            await CreateOrReplaceSpecificAssetIDForShell(aasDtId, newAssetId, aasInfoDtId);
                            _logger.LogDebug($"Created specific Asset Id '{newAssetId}' for AAS with identifier '{aasIdentifier}'");
                        }
                        result.Add(newAssetId);
                    }
                }

            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");

                throw new AASDiscoveryException($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        public async Task<List<IdentifierKeyValuePair>> DeleteAllAssetLinksById(string aasIdentifier)
        {
            List<IdentifierKeyValuePair> result = new List<IdentifierKeyValuePair>();

            _logger.LogDebug($"DeleteAllAssetLinksById(aasIdentifier='{aasIdentifier}') called");

            if (aasIdentifier == null || aasIdentifier.Length == 0)
            {
                _logger.LogError($"*** Error - Parameter 'aasIdentifier' must not be empty");
            }

            try
            {
                string aasDtId = await FindDTIdForIdentification(aasIdentifier);
                if (aasDtId == null || aasDtId.Length == 0)
                {
                    _logger.LogInformation($"Can't find AAS with identifier '{aasIdentifier}'");
                    return result;
                }

                string aasInfoDtId = await FindInfoDTIdForIdentification(aasIdentifier);
                if (aasInfoDtId == null || aasInfoDtId.Length == 0)
                {
                    _logger.LogDebug($"Can't find AAS Info Twin for AAS with identifier '{aasIdentifier}'. Please create the according AAS Info Twin first");
                    return result;
                }

                AsyncPageable<BasicRelationship> rels = dtClient.GetRelationshipsAsync<BasicRelationship>(aasInfoDtId);
                await foreach (BasicRelationship outgoingRel in rels)
                {
                    if (outgoingRel.Name == "globalAssetId")
                    {
                        BasicDigitalTwin referenceTwin = await dtClient.GetDigitalTwinAsync<BasicDigitalTwin>(outgoingRel.TargetId);
                        AsyncPageable<BasicRelationship> keyRels = dtClient.GetRelationshipsAsync<BasicRelationship>(referenceTwin.Id, "key");
                        string globalAssetIdValue = "";
                        await foreach (BasicRelationship keyRel in keyRels)
                        {
                            await dtClient.DeleteRelationshipAsync(referenceTwin.Id, keyRel.Id);

                            BasicDigitalTwin keyTwin = await dtClient.GetDigitalTwinAsync<BasicDigitalTwin>(keyRel.TargetId);
                            globalAssetIdValue = keyTwin.Contents["value"].ToString();
                            await dtClient.DeleteDigitalTwinAsync(keyRel.TargetId);
                        }

                        IdentifierKeyValuePair identifierKeyValue = new IdentifierKeyValuePair()
                            { Key = "globalAssetId", Value = globalAssetIdValue };

                        await dtClient.DeleteRelationshipAsync(aasInfoDtId, outgoingRel.Id);
                        await dtClient.DeleteDigitalTwinAsync(outgoingRel.TargetId);

                        result.Add(identifierKeyValue);

                    } else if (outgoingRel.Name == "specificAssetId")
                    {
                        BasicDigitalTwin identifierTwin = await dtClient.GetDigitalTwinAsync<BasicDigitalTwin>(outgoingRel.TargetId);
                        if (identifierTwin != null)
                        {
                            IdentifierKeyValuePair identifierKeyValue = new IdentifierKeyValuePair() 
                                { Key = identifierTwin.Contents["key"].ToString(), Value = identifierTwin.Contents["value"].ToString() };
                            
                            await dtClient.DeleteRelationshipAsync(aasInfoDtId, outgoingRel.Id);
                            await dtClient.DeleteDigitalTwinAsync(outgoingRel.TargetId);

                            result.Add(identifierKeyValue);
                        }
                    }
                }

            }
            catch (RequestFailedException exc)
            {
                _logger.LogError($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");

                throw new AASDiscoveryException($"*** Error in retrieving information from ADT:{exc.Status}/{exc.Message}");
            }

            return result;
        }

    }
}
