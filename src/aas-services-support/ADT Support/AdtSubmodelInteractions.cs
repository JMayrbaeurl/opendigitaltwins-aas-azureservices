using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AAS.AASX.CmdLine.ADT;
using AAS.API.Services.ADT;
using AdtModels.AdtModels;
using Azure;
using Azure.DigitalTwins.Core;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtSubmodelInteractions : IAdtSubmodelInteractions
    {
        private readonly DigitalTwinsClient _client;

        public AdtSubmodelInteractions(DigitalTwinsClientFactory adtClientFactory)
        {
            _client = adtClientFactory.CreateClient();
        }

       

        public async Task<AdtSubmodelInformation> GetAllInformationForSubmodelWithTwinId(string twinId)
        {
            var adtSubmodelInformation = new AdtSubmodelInformation();
            //var items = GetAllTwinsDirectlyRelatedToTwinWithId(twinId);
            string queryString = "Select twin0, rel, twin1 from digitaltwins match (twin0)-[rel]->(twin1) " +
                                 $"where twin0.$dtId='{twinId}'";
            var items= _client.Query<JsonObject>(queryString);
            foreach (var item in items)
            {
                var dataTwin1 = item["twin1"];
                if (adtSubmodelInformation.RootElement.Id == null)
                {
                    adtSubmodelInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodel>(item["twin0"].ToString());
                }

                var rel = item["rel"]["$relationshipName"].ToString();
                DeserializeAdtResponse(rel, dataTwin1, adtSubmodelInformation.ConcreteAasInformation);
                await DeserializeAdtResponseForSubmodelOrSmeCollection(rel, dataTwin1, adtSubmodelInformation);
            }

            if (adtSubmodelInformation.RootElement.Id == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                items = _client.Query<JsonObject>($"Select twin from digitaltwins twin where twin.$dtId = '{twinId}'");
                foreach (var item in items)
                {
                    adtSubmodelInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodel>(item["twin"].ToString());
                }
            }
            return adtSubmodelInformation;
        }

        private AsyncPageable<JsonObject> GetAllTwinsDirectlyRelatedToTwinWithId(string twinId)
        {
            string queryString = "Select twin0, rel, twin1 from digitaltwins match (twin0)-[rel]->(twin1) " +
                                 $"where twin0.$dtId='{twinId}'";
            return _client.QueryAsync<JsonObject>(queryString);
        }

        public AdtConcreteAasInformation DeserializeAdtResponse(string relationship, JsonNode dataTwin, AdtConcreteAasInformation information)
        {

            if (relationship == "dataSpecification")
            {
                information.dataSpecifications.Add(
                    JsonSerializer.Deserialize<AdtDataSpecification>(dataTwin));
            }
            else if (relationship == "semanticId")
            {
                information.semanticId = JsonSerializer.Deserialize<AdtReference>(dataTwin);
            }
            else if (relationship == "supplementalSemanticId")
                information.supplementalSemanticId.Add(JsonSerializer.Deserialize<AdtReference>(dataTwin));

            return information;
        }

        private async Task<AdtSubmodelAndSMCInformation<T>>
            DeserializeAdtResponseForSubmodelOrSmeCollection<T>(
            string relationship, JsonNode dataTwin, AdtSubmodelAndSMCInformation<T> information)
        {

            if (relationship == "submodelElement" || relationship == "value")
            {
                var model = dataTwin["$metadata"]["$model"].ToString();
                var sme = JsonSerializer.Deserialize<AdtSubmodelElement>(dataTwin);

                if (model == ADTAASOntology.MODEL_SUBMODELELEMENTCOLLECTION)
                    information.smeCollections.Add(await GetAllSubmodelElementCollectionInformation(dataTwin["$dtId"].ToString()));
                else if (model == ADTAASOntology.MODEL_PROPERTY)
                    information.properties.Add(JsonSerializer.Deserialize<AdtProperty>(dataTwin));
                else if (model == ADTAASOntology.MODEL_FILE)
                    information.files.Add(JsonSerializer.Deserialize<AdtFile>(dataTwin));
                else
                    throw new AdtModelNotSupported($"Unsupported AdtModel of Type {model}");

            }

            return information;
        }


        public async Task<AdtSubmodelElementCollectionInformation> GetAllSubmodelElementCollectionInformation(
            string twinId)
        {
            var adtSmeCollectionInformation = new AdtSubmodelElementCollectionInformation();

            var items = GetAllTwinsDirectlyRelatedToTwinWithId(twinId);

            await foreach (var item in items)
            {
                if (adtSmeCollectionInformation.RootElement.IdShort == null)
                {
                    adtSmeCollectionInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodelElementCollection>(item["twin0"].ToString());
                }
                var rel = item["rel"]["$relationshipName"].ToString();
                var dataTwin1 = item["twin1"];

                DeserializeAdtResponse(rel, dataTwin1, adtSmeCollectionInformation.ConcreteAasInformation);
                await DeserializeAdtResponseForSubmodelOrSmeCollection(rel, dataTwin1, adtSmeCollectionInformation);
            }

            if (adtSmeCollectionInformation.RootElement.IdShort == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                items = _client.QueryAsync<JsonObject>($"Select twin from digitaltwins twin where twin.$dtId = '{twinId}'");
                await foreach (var item in items)
                {
                    adtSmeCollectionInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodelElementCollection>(item["twin"].ToString());
                }
            }

            return adtSmeCollectionInformation;

        }
    }


    public class AdtConcreteAasInformation
    {
        public List<AdtDataSpecification> dataSpecifications =
                    new List<AdtDataSpecification>();

        public AdtReference semanticId = new AdtReference();

        public List<AdtReference> supplementalSemanticId = new List<AdtReference>();
    }

    public abstract class AdtGeneralAasInformation<T>
    {
        public T RootElement { get; set; }

        public AdtConcreteAasInformation ConcreteAasInformation = new AdtConcreteAasInformation();

    }

    public abstract class AdtSubmodelAndSMCInformation<T> : AdtGeneralAasInformation<T>
    {
        public List<AdtSubmodelElementCollectionInformation> smeCollections = new List<AdtSubmodelElementCollectionInformation>();
        public List<AdtProperty> properties = new List<AdtProperty>();
        public List<AdtFile> files = new List<AdtFile>();
    }

    public class AdtSubmodelInformation : AdtSubmodelAndSMCInformation<AdtSubmodel>
    {
        public AdtSubmodel RootElement = new AdtSubmodel();
    }

    public class AdtSubmodelElementCollectionInformation :
        AdtSubmodelAndSMCInformation<AdtSubmodelElementCollection>
    {
        public AdtSubmodelElementCollection RootElement = new AdtSubmodelElementCollection();
    }
}
