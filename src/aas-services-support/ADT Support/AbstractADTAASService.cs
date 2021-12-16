using AAS.API.Models;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
