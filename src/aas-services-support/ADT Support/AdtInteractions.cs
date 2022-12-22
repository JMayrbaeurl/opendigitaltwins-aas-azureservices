using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AAS.AASX.CmdLine.ADT;
using AAS.API.Services.ADT;
using AdtModels.AdtModels;
using Azure.DigitalTwins.Core;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtInteractions : IAdtInteractions
    {
        public DigitalTwinsClient _client;

        public AdtInteractions(DigitalTwinsClientFactory adtClientFactory)
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

        public AdtGeneralAasInformation<AdtAas> GetAllInformationForAasWithTwinId(string aasTwinId)
        {
            throw new NotImplementedException();
        }

        public AdtAssetAdministrationShellInformation GetAllInformationForAasWithId(string aasId)
        {
            var dtId = getDtidFromAasId(aasId);
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

        private string getDtidFromAasId(string aasId)
        {
            string queryString =
                $"Select aas.$dtId as dtId from digitaltwins aas where aas.id = '{aasId}' and aas.$metadata.$model='dtmi:digitaltwins:aas:AssetAdministrationShell;1'";
            var ids = _client.Query<JsonObject>(queryString);
            var aasIds = new List<string>();
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

        public AdtSubmodel GetAdtSubmodelWithSubmodelId(string submodelId)
        {
            string queryString =
                $"Select * from digitaltwins submodel where submodel.$dtId = '{submodelId}' and submodel.$metadata.$model='dtmi:digitaltwins:aas:Submodel;1'";
            var response = _client.Query<AdtSubmodel>(queryString);
            foreach (var twin in response)
            {
                return twin;
            }

            throw new AdtException($"Could not find twinId for given aasId {submodelId}");
        }

        public List<AdtSubmodelElement> GetAdtSubmodelElementsFromParentTwinWithId(string adtTwinId)
        {
            var submodelElements = new List<AdtSubmodelElement>();

            string queryString =
                $"Select sme from digitaltwins match (parent)-[rel]->(sme) where parent.$dtId='{adtTwinId}' and rel.$relationshipName in ['submodelElement','value']";
            var response = _client.Query<JsonObject>(queryString);
            foreach (var twin in response)
            {
                var adtModel = twin["sme"]["$metadata"]["$model"].ToString();
                var twinAsString = twin["sme"].ToString();

                if (adtModel == ADTAASOntology.MODEL_PROPERTY)
                {
                    submodelElements.Add(JsonSerializer.Deserialize<AdtProperty>(twinAsString));
                }
                else if (adtModel == ADTAASOntology.MODEL_SUBMODELELEMENTCOLLECTION)
                {
                    var smeCollection = JsonSerializer.Deserialize<AdtSubmodelElementCollection>(twinAsString);
                    var twinDtId = twin["sme"]["$dtId"].ToString();
                    smeCollection.submodelElements = GetAdtSubmodelElementsFromParentTwinWithId(twinDtId);
                    submodelElements.Add(smeCollection);
                }
                else if (adtModel == ADTAASOntology.MODEL_FILE)
                {
                    submodelElements.Add(JsonSerializer.Deserialize<AdtFile>(twinAsString));
                }
            }

            return submodelElements;
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

        public AdtReference GetSemanticId(string parentTwinId)
        {
            string queryString =
                $"Select reference from digitaltwins match (twin)-[rel]->(reference) where twin.$dtId='{parentTwinId}' and rel.$relationshipName='semanticId'";
            var response = _client.Query<AdtResponseForSemanticIdReference>(queryString);
            foreach (var twin in response)
            {
                return twin.Reference;
            }

            throw new NoSemanticIdFound($"No Semantic Id present for twin with DTID {parentTwinId}");
        }
    }

    class AdtException : Exception
    {
        public AdtException(string message) : base(message)
        { }

        public AdtException(string message, Exception? innerException) : base(message, innerException)
        { }
    }

    public class AdtResponseForAllAasInformation
    {

        [JsonPropertyName("relationshipName")]
        public string RelationshipName { get; set; }

        [JsonPropertyName("twin")]
        public JsonObject? TwinJsonObject { get; set; }
    }

    public class AdtResponseForSemanticIdReference
    {
        [JsonPropertyName("reference")]
        public AdtReference Reference { get; set; }
    }


}
