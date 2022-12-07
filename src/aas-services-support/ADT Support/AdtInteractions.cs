using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AAS.AASX.CmdLine.ADT;
using AAS.API.Interfaces;
using AAS.API.Models;
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

        public List<AdtResponseForAllAasInformation> GetAllInformationForAasWithId(string aasId)
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

            return allAasInformation;

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

        public List<string> GetAllSubmodelTwinIds()
        {
            string queryString =
                "SELECT submodel.$dtId as submodelId FROM digitaltwins submodel where is_of_model('dtmi:digitaltwins:aas:Submodel;1')";
            var ids = _client.Query<JsonObject>(queryString);
            var submodelIds = new List<string>();
            foreach (var submodelId in ids)
                submodelIds.Add(submodelId["submodelId"].ToString());

            return submodelIds;
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
