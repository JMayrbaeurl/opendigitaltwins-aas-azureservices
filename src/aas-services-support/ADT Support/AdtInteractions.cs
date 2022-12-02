using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AAS.API.Interfaces;
using AAS.API.Services.ADT;
using AdtModels.AdtModels;
using Azure.DigitalTwins.Core;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtInteractions
    {
        private readonly DigitalTwinsClient _client;

        public AdtInteractions(DigitalTwinsClient client)
        {
            _client = client;
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


}
