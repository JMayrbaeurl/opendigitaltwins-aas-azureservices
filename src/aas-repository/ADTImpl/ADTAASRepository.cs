using AAS.API.Models;
using AAS.API.Services.ADT;
using Azure;
using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.API.Repository
{
    public class ADTAASRepository : AbstractADTAASService, AASRepository
    {
        private ADTAASModelFactory modelFactory = new ADTAASModelFactory();

        public ADTAASRepository(DigitalTwinsClient client) : base(client)
        {
        }

        public async Task<List<AssetAdministrationShell>> GetAllAdministrationShells()
        {
            return await ReadAASFromQuery(GetStdAASQueryString());
        }

        public async Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByIdShort(string withIdShort)
        {
            string query = GetStdAASQueryString() + $" AND idShort = '{withIdShort}'";

            return await ReadAASFromQuery(query);
        }

        public async Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByAssetId(List<IdentifierKeyValuePair> assetIds)
        {
            List<AssetAdministrationShell> result = new List<AssetAdministrationShell>();

            foreach (IdentifierKeyValuePair identifier in assetIds)
            {
                if (identifier.Key != null && identifier.Value != null)
                {
                    List<string> idDTIds = await ReadIdentifers(identifier.Key, identifier.Value);
                    foreach (string idDTId in idDTIds)
                    {
                        result.AddRange(await ReadAllAASwithIdentifierKeyValuePairInstance(idDTId));
                    }
                }
            }

            return result;
        }

        private async Task<List<AssetAdministrationShell>> ReadAASFromQuery(string queryString)
        {
            List<AssetAdministrationShell> result = new List<AssetAdministrationShell>();

            try
            {
                AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                await foreach (BasicDigitalTwin twin in twins)
                {
                    result.Add(modelFactory.CreateAASFromBasicDigitalTwin(twin));
                }
            }
            catch (RequestFailedException exc)
            {
                //log.LogError($"*** Error in retrieving parent:{exc.Status}/{exc.Message}");
                throw new AASRepositoryException($"*** Error in retrieving AAS:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        private async Task<List<AssetAdministrationShell>> ReadAllAASwithIdentifierKeyValuePairInstance(string dtId)
        {
            List<AssetAdministrationShell> result = new List<AssetAdministrationShell>();

            string queryString = $"SELECT AAS FROM digitaltwins AAS JOIN AASInfo RELATED AAS.assetInformation JOIN AASID RELATED AASInfo.specificAssetId WHERE AASID.$dtId = '{dtId}'";

            try
            {
                AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                await foreach (BasicDigitalTwin twin in twins)
                {
                    result.Add(modelFactory.CreateAASFromJsonElement((System.Text.Json.JsonElement)twin.Contents["AAS"]));
                }
            }
            catch (RequestFailedException exc)
            {
                //log.LogError($"*** Error in retrieving parent:{exc.Status}/{exc.Message}");
                throw new AASRepositoryException($"*** Error in retrieving AAS:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        private async Task<List<string>> ReadIdentifers(string key, string value)
        {
            List<string> result = new List<string>();

            string queryString = $"SELECT * FROM digitaltwins WHERE IS_OF_MODEL('dtmi:digitaltwins:aas:IdentifierKeyValuePair;1') and key = '{key}' and value = '{value}'";

            try
            {
                AsyncPageable<BasicDigitalTwin> twins = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                await foreach (BasicDigitalTwin twin in twins)
                {
                    result.Add(twin.Id);
                }
            }
            catch (RequestFailedException exc)
            {
                //log.LogError($"*** Error in retrieving parent:{exc.Status}/{exc.Message}");
                throw new AASRepositoryException($"*** Error in retrieving AAS:{exc.Status}/{exc.Message}");
            }

            return result;
        }

        private string GetStdAASQueryString()
        {
            return $"SELECT * FROM digitaltwins WHERE IS_OF_MODEL('{ADTConstants.AAS_MODEL_NAME}')";
        }


    }
}
