using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AAS.ADT;
using AAS.ADT.Models;
using AAS.API.Services.ADT;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt
{
    public class AdtAasConnector : IAdtAasConnector
    {
        private readonly DigitalTwinsClient _client;

        public AdtAasConnector(DigitalTwinsClientFactory adtClientFactory)
        {
            _client = adtClientFactory.CreateClient();
        }

        public List<string> GetAllAasIds()
        {
            string queryString =
                "SELECT aas.id as aasId FROM digitaltwins aas where is_of_model('dtmi:digitaltwins:aas:AssetAdministrationShell;1')";
            var ids = _client.Query<JsonObject>(queryString);
            var aasIds = new List<string>();
            foreach (var id in ids)
            {
                aasIds.Add(id["aasId"].ToString());
            }

            return aasIds;
        }

        public AdtAas GetAdtAasForAasWithId(string aasId)
        {
            string queryString =
                $"Select * from digitaltwins aas where aas.id = '{aasId}' and aas.$metadata.$model='dtmi:digitaltwins:aas:AssetAdministrationShell;1'";
            var response = _client.Query<AdtAas>(queryString);
            foreach (var twin in response)
            {
                return twin;
            }

            throw new AdtException($"Could not find twinId for given aasId {aasId}");
        }


        public AdtAssetAdministrationShellInformation GetAllInformationForAasWithId(string aasId)
        {
            var dtId = GetDtidFromAasId(aasId);
            string queryString =
                $"SELECT rel.$relationshipName as relationshipName, twin from digitaltwins match (aas)-[rel]->(twin) where aas.$dtId='{dtId}'";
            var response = _client.Query<AdtResponseForAllAasInformation>(queryString);
            var allAasInformation = new List<AdtResponseForAllAasInformation>();
            foreach (var aasInformation in response)
            {
                allAasInformation.Add(aasInformation);
            }

            return GetAasInformationFromAdtResponse(allAasInformation);
        }

        private AdtAssetAdministrationShellInformation GetAasInformationFromAdtResponse(List<AdtResponseForAllAasInformation> response)
        {
            var information = new AdtAssetAdministrationShellInformation();
            foreach (var aasInformation in response)
            {
                if (aasInformation.RelationshipName == "assetInformation")
                {
                    information.AssetInformation = JsonSerializer.Deserialize<AdtAssetInformation>(aasInformation.TwinJsonObject.ToString());
                }

                else if (aasInformation.RelationshipName == "submodel")
                {
                    information.Submodels.Add(
                        JsonSerializer.Deserialize<AdtSubmodel>(aasInformation.TwinJsonObject.ToString()));
                }
                else if (aasInformation.RelationshipName == "derivedFrom")
                {
                    information.DerivedFrom =
                        JsonSerializer.Deserialize<AdtAas>(aasInformation.TwinJsonObject.ToString());
                }
            }

            return information;
        }

        private string GetDtidFromAasId(string aasId)
        {
            string queryString =
                $"Select aas.$dtId as dtId from digitaltwins aas where aas.id = '{aasId}' and aas.$metadata.$model='dtmi:digitaltwins:aas:AssetAdministrationShell;1'";
            var ids = _client.Query<JsonObject>(queryString);
            foreach (var id in ids)
            {
                return (id["dtId"].ToString());
            }

            throw new AdtException($"Could not find twinId for given aasId {aasId}");
        }

        public async Task<List<string>> GetAllSubmodelTwinIds()
        {
            var submodelTwinIds = new List<string>();
            var queryString =
                "Select twin.$dtId as dtId from digitaltwins twin where is_of_model('dtmi:digitaltwins:aas:Submodel') ";
            var items = _client.QueryAsync<JsonObject>(queryString);
            await foreach (var item in items)
            {
                submodelTwinIds.Add(item["dtId"].ToString());
            }
            return submodelTwinIds;
        }

        public string GetTwinIdForElementWithId(string Id)
        {
            string queryString =
                $"Select twin.$dtId as dtId from digitaltwins twin where twin.id='{Id}'";
            var response = _client.Query<JsonObject>(queryString);
            foreach (var twin in response)
            {
                return twin["dtId"].ToString();
            }

            throw new AdtException($"No Object with Id {Id} found.");
        }
    }
    
    public class AdtResponseForAllAasInformation
    {
        [JsonPropertyName("relationshipName")]
        public string RelationshipName { get; set; }

        [JsonPropertyName("twin")]
        public JsonObject? TwinJsonObject { get; set; }
    }


}
