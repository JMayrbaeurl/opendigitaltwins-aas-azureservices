using AAS.API.Models;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AAS.API.Services.ADT
{
    public abstract class AbstractADTAASService
    {
        protected DigitalTwinsClient dtClient;

        protected readonly ILogger _logger;

        public AbstractADTAASService(ILogger logger)
        {
            _logger = logger;
        }

        public AbstractADTAASService(DigitalTwinsClient client)
        {
            dtClient = client;
        }

        protected bool HasGlobalAssetId(List<IdentifierKeyValuePair> assetIds)
        {
            bool result = false;

            if (assetIds != null)
                result = assetIds.Exists(id => id.Key == ADTConstants.GLOBALASSETID);

            return result;
        }

        protected List<string> GetGlobalAssetId(List<IdentifierKeyValuePair> assetIds)
        {
            List<string> result = new List<string>();

            if (assetIds != null)
            {
                foreach (var item in (assetIds.FindAll(id => id.Key == ADTConstants.GLOBALASSETID)))
                {
                    result.Add(item.Value);
                }
            }

            return result;
        }

        protected List<IdentifierKeyValuePair> GetSpecificAssetIds(List<IdentifierKeyValuePair> assetIds)
        {
            return assetIds.FindAll(id => id.Key != ADTConstants.GLOBALASSETID);
        }

        protected async Task<string> FindDTIdForIdentification(string aasIdentifier)
        {
            string result = "";

            string queryString = $"SELECT * FROM digitaltwins WHERE IS_OF_MODEL('{ADTConstants.AAS_MODEL_NAME}') AND identification.id = '{aasIdentifier}'";

            _logger.LogDebug($"ADT query for dtId of Asset identification with: {queryString}");

            AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            IAsyncEnumerator<BasicDigitalTwin> enumerator = twins.GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync())
            {
                result = enumerator.Current.Id;

                _logger.LogDebug($"Found dtId '{result}' for Asset identifier '{aasIdentifier}'");
            }

            return result;
        }

        protected async Task<string> FindInfoDTIdForIdentification(string aasIdentifier)
        {
            string result = "";

            string aasDtId = await FindDTIdForIdentification(aasIdentifier);
            string queryString = $"SELECT aasinfo FROM digitaltwins MATCH(aas) -[:assetInformation]->(aasinfo) WHERE aas.$dtId = '{aasDtId}'";

            _logger.LogDebug($"ADT query for dtId of AssetInformation with: {queryString}");

            AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            IAsyncEnumerator<BasicDigitalTwin> enumerator = twins.GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync())
            {
                JsonElement aasInfoTwin = (JsonElement)enumerator.Current.Contents["aasinfo"];
                JsonElement propFromADT;
                if (aasInfoTwin.TryGetProperty("$dtId", out propFromADT))
                    result = propFromADT.GetString();

                _logger.LogDebug($"Found dtId '{result}' for Asset identifier '{aasIdentifier}'");
            }

            return result;
        }

        protected async Task<List<string>> FindDTIdForGlobalAssetId(List<string> idValues, string keyType = "Asset")
        {
            List<string> result = new List<string>();
            string idValueString = ConvertStringListToQueryArrayString(idValues);

            string queryString = $"SELECT * FROM digitaltwins WHERE IS_OF_MODEL('{ADTConstants.KEY_MODEL_NAME}') AND key = '{keyType}' AND value IN {idValueString}";

            AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in twins)
            {
                result.Add(twin.Id);
            }

            return result;
        }

        protected async Task<List<string>> FindDTIdForSpecificAssetId(List<IdentifierKeyValuePair> assetIds)
        {
            List<string> result = new List<string>();

            List<string> idStrings = Enumerable.Select(assetIds, id => $"(key = '{id.Key}' AND value = '{id.Value}')").ToList();
            string allIds = String.Join(" OR ", idStrings);
            string queryString = $"SELECT * FROM digitaltwins WHERE IS_OF_MODEL('{ADTConstants.IDENTIFIERKEYVALUEPAIR_MODEL_NAME}') AND ( {allIds} )";

            AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in twins)
            {
                result.Add(twin.Id);
            }

            return result;
        }

        public static string ConvertStringListToQueryArrayString(List<string> idValues)
        {
            string idValueString = "[]";

            foreach (var idValue in idValues)
            {
                if (idValueString.Length > 2)
                    idValueString = idValueString.Insert(idValueString.Length - 1, ",");
                idValueString = idValueString.Insert(idValueString.Length - 1, $"'{idValue}'");
            }

            return idValueString;
        }

        public async Task<ADTOperationsResult> CreateOrReplaceGlobalAssetIDForShell(string aasDtId, string globalAssetIDValue, string optAasInfoDtId = null)
        {
            ADTOperationsResult result = new ADTOperationsResult();

            if (aasDtId == null || aasDtId.Length == 0)
            {
                _logger.LogError($"Parameter 'aasDtId' must not be empty");
                return result;
            }

            if (globalAssetIDValue == null || globalAssetIDValue.Length == 0)
            {
                _logger.LogError($"Parameter 'globalAssetIDValue' must not be empty");
                return result;
            }

            string aasInfoDtId = optAasInfoDtId != null ? optAasInfoDtId : await FindAASInfoDtIdForAASWithDtId(aasDtId);
            if (aasInfoDtId == null)
            {
                _logger.LogDebug($"Can't find AAS Info Twin for AAS Twin with id '{aasDtId}'. Please create the according AAS Info Twin first");
                return result;
            }

            // Create a 'Reference' Twin with a 'Key' related to
            string referenceId = $"{aasDtId}_GlobalAssetID";
            var referenceInitData = new BasicDigitalTwin
            {
                Id = referenceId,
                Metadata = { ModelId = "dtmi:digitaltwins:aas:Reference;1" },
                // Initialize properties
                Contents = { }
            };

            result.AddCreatedReplacedTwin(await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(referenceId, referenceInitData));

            // Create a 'Key' Twin
            string keyId = $"{aasDtId}_GlobalAssetID_Key";
            var keyInitData = new BasicDigitalTwin
            {
                Id = keyId,
                Metadata = { ModelId = "dtmi:digitaltwins:aas:Key;1" },
                // Initialize properties
                Contents = { { "key", "Asset" }, { "value", globalAssetIDValue }, { "idType", "IRI" } }
                // TODO How to initialize the additional field 'idType'
            };

            result.AddCreatedReplacedTwin(await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(keyId, keyInitData));

            // Create relationship between Reference and Key
            string relName = "key";
            var relationship = new BasicRelationship
            {
                TargetId = keyId,
                Name = relName,
            };

            string relId = $"{referenceId}-{relName}->{keyId}";
            result.AddCreatedReplacedRelationship(await dtClient.CreateOrReplaceRelationshipAsync<BasicRelationship>(referenceId, relId, relationship));

            // Create relationship between AAS Info and Reference
            relName = "globalAssetId";
            relationship = new BasicRelationship
            {
                TargetId = referenceId,
                Name = relName,
            };

            relId = $"{aasInfoDtId}-{relName}->{referenceId}";
            result.AddCreatedReplacedRelationship(await dtClient.CreateOrReplaceRelationshipAsync<BasicRelationship>(aasInfoDtId, relId, relationship));

            return result;
        }

        protected async Task<string> FindAASInfoDtIdForAASWithDtId(string aasDtId)
        {
            string aasInfoDtId = "";

            if (aasDtId == null || aasDtId.Length == 0)
            {
                _logger.LogError($"Parameter 'aasDtId' must not be empty");
                return aasInfoDtId;
            }

            string queryString = $"SELECT aasinfo FROM digitaltwins MATCH(aas) -[:assetInformation]->(aasinfo) WHERE aas.$dtId = '{aasDtId}'";
            AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            IAsyncEnumerator<BasicDigitalTwin> enumerator = twins.GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync())
            {
                JsonElement aasInfoTwin = (JsonElement)enumerator.Current.Contents["aasinfo"];
                JsonElement propFromADT;
                if (aasInfoTwin.TryGetProperty("$dtId", out propFromADT))
                    aasInfoDtId = propFromADT.GetString();
            }

            return aasInfoDtId;
        }

        public async Task<ADTOperationsResult> CreateOrReplaceSpecificAssetIDForShell(string aasDtId, IdentifierKeyValuePair newAssetId, string optAasInfoDtId = null)
        {
            ADTOperationsResult result = new ADTOperationsResult();

            if (aasDtId == null || aasDtId.Length == 0)
            {
                _logger.LogError($"Parameter 'aasDtId' must not be empty");
                return result;
            }

            if (newAssetId == null || newAssetId.Key == null || newAssetId.Value == null)
            {
                _logger.LogError($"Parameter 'newAssetId' must not be null and must have values for key and value");
                return result;
            }

            string aasInfoDtId = optAasInfoDtId != null ? optAasInfoDtId : await FindAASInfoDtIdForAASWithDtId(aasDtId);
            if (aasInfoDtId == null)
            {
                _logger.LogDebug($"Can't find AAS Info Twin for AAS Twin with id '{aasDtId}'. Please create the according AAS Info Twin first");
                return result;
            }

            // Create a 'IdentifierKeyValuePair' Twin
            string identifierId = $"{aasDtId}_SpecificAssetID_{newAssetId.Key}";
            var identifierInitData = new BasicDigitalTwin
            {
                Id = identifierId,
                Metadata = { ModelId = "dtmi:digitaltwins:aas:IdentifierKeyValuePair;1" },
                // Initialize properties
                Contents = { {"key", newAssetId.Key}, {"value", newAssetId.Value}
                }
            };
            result.AddCreatedReplacedTwin(await dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(identifierId, identifierInitData));

            // Create relationship between AAS Info and new Specific Asset Id
            string relName = "specificAssetId";
            BasicRelationship relationship = new BasicRelationship
            {
                TargetId = identifierId,
                Name = relName,
            };

            string relId = $"{aasInfoDtId}-{relName}->{identifierId}";
            result.AddCreatedReplacedRelationship(await dtClient.CreateOrReplaceRelationshipAsync<BasicRelationship>(aasInfoDtId, relId, relationship));

            return result;
        }
    }
}
