using AAS.API.Models;
using Azure;
using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Services.ADT
{
    public abstract class AbstractADTAASService
    {
        protected DigitalTwinsClient dtClient;

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
