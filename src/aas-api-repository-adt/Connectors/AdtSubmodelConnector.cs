using System.Text.Json;
using System.Text.Json.Nodes;
using AAS.ADT;
using AAS.ADT.Models;
using AAS.API.Repository.Adt.Exceptions;
using AAS.API.Services.ADT;
using AasCore.Aas3_0_RC02;
using Azure;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelConnector : IAdtSubmodelConnector
    {
        private readonly DigitalTwinsClient _client;


        public AdtSubmodelConnector(DigitalTwinsClientFactory adtClientFactory)
        {
            _client = adtClientFactory.CreateClient();
        }

        public async Task<AdtGeneralAasInformation<T>> GetAdtGeneralAasInformationForTwinWithId<T>(string twinId)
            where T : AdtIdentifiable, new()
        {
            var adtGeneralInformation = new AdtGeneralAasInformation<T>();
            string queryString = "Select twin0, rel, twin1 from digitaltwins match (twin0)-[rel]->(twin1) " +
                                 $"where twin0.$dtId='{twinId}'";
            var items = _client.Query<JsonObject>(queryString);
            foreach (var item in items)
            {
                var dataTwin1 = item["twin1"];
                if (adtGeneralInformation.RootElement.Id == null)
                {
                    adtGeneralInformation.RootElement = JsonSerializer.Deserialize<T>(item["twin0"].ToString());
                }

                var rel = item["rel"]["$relationshipName"].ToString();
                DeserializeAdtResponse(rel, dataTwin1, adtGeneralInformation.ConcreteAasInformation);
                adtGeneralInformation.relatedTwins.Add((dataTwin1, rel));
            }

            if (adtGeneralInformation.RootElement.Id == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                items = _client.Query<JsonObject>($"Select twin from digitaltwins twin where twin.$dtId = '{twinId}'");
                foreach (var item in items)
                {
                    adtGeneralInformation.RootElement = JsonSerializer.Deserialize<T>(item["twin"].ToString());
                }
            }

            adtGeneralInformation.definitionsAndSemantics = await
                GetAllDescriptionsForSubmodelElements(twinId);
            return adtGeneralInformation;
        }

        public async Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId)
        {
            var generalInformation = await GetAdtGeneralAasInformationForTwinWithId<AdtSubmodel>(twinId);
            var adtSubmodelInformation = new AdtSubmodelAndSmcInformation<AdtSubmodel>();

            adtSubmodelInformation.GeneralAasInformation.definitionsAndSemantics = generalInformation.definitionsAndSemantics;
            adtSubmodelInformation.GeneralAasInformation.ConcreteAasInformation = generalInformation.ConcreteAasInformation;
            adtSubmodelInformation.GeneralAasInformation.RootElement = generalInformation.RootElement;
            foreach (var (relatedTwin, rel) in generalInformation.relatedTwins)
            {
                adtSubmodelInformation = await DeserializeAdtResponseForSubmodelOrSmeCollection(rel, relatedTwin, adtSubmodelInformation);
            }

            return adtSubmodelInformation;
        }

        //public async Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId)
        //{
        //    var adtSubmodelInformation = new AdtSubmodelAndSmcInformation<AdtSubmodel>();
        //    string queryString = "Select twin0, rel, twin1 from digitaltwins match (twin0)-[rel]->(twin1) " +
        //                         $"where twin0.$dtId='{twinId}'";
        //    var items = _client.Query<JsonObject>(queryString);
        //    foreach (var item in items)
        //    {
        //        var dataTwin1 = item["twin1"];
        //        if (adtSubmodelInformation.RootElement.Id == null)
        //        {
        //            adtSubmodelInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodel>(item["twin0"].ToString());
        //        }

        //        var rel = item["rel"]["$relationshipName"].ToString();
        //        DeserializeAdtResponse(rel, dataTwin1, adtSubmodelInformation.ConcreteAasInformation);
        //        adtSubmodelInformation =  await DeserializeAdtResponseForSubmodelOrSmeCollection(rel, dataTwin1, adtSubmodelInformation);
        //    }

        //    if (adtSubmodelInformation.RootElement.Id == null)
        //    {
        //        // no Twins related to this Submodel -> the Query Response was Empty
        //        items = _client.Query<JsonObject>($"Select twin from digitaltwins twin where twin.$dtId = '{twinId}'");
        //        foreach (var item in items)
        //        {
        //            adtSubmodelInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodel>(item["twin"].ToString());
        //        }
        //    }

        //    adtSubmodelInformation.definitionsAndSemantic = await
        //        GetAllDescriptionsForSubmodelElements(twinId);
        //    return adtSubmodelInformation;
        //}



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

        private async Task<AdtSubmodelAndSmcInformation<T>>
            DeserializeAdtResponseForSubmodelOrSmeCollection<T>(
            string relationship, JsonNode dataTwin, AdtSubmodelAndSmcInformation<T> information) where T : AdtBase, new()
        {

            if (relationship == "submodelElement" || relationship == "value")
            {
                var model = dataTwin["$metadata"]["$model"].ToString();
                var sme = JsonSerializer.Deserialize<AdtSubmodelElement>(dataTwin);

                if (model == AdtAasOntology.MODEL_SUBMODELELEMENTCOLLECTION)
                    information.AdtSubmodelElements.smeCollections.Add(await GetAllSubmodelElementCollectionInformation(dataTwin["$dtId"].ToString()));
                else if (model == AdtAasOntology.MODEL_PROPERTY)
                    information.AdtSubmodelElements.properties.Add(JsonSerializer.Deserialize<AdtProperty>(dataTwin));
                else if (model == AdtAasOntology.MODEL_FILE)
                    information.AdtSubmodelElements.files.Add(JsonSerializer.Deserialize<AdtFile>(dataTwin));
                else
                    throw new AdtModelNotSupported($"Unsupported AdtModel of Type {model}");
            }

            return information;
        }


        public async Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAllSubmodelElementCollectionInformation(
            string twinId)
        {
            var adtSmeCollectionInformation = new AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>();

            var items = GetAllTwinsDirectlyRelatedToTwinWithId(twinId);

            await foreach (var item in items)
            {
                if (adtSmeCollectionInformation.GeneralAasInformation.RootElement.IdShort == null)
                {
                    adtSmeCollectionInformation.GeneralAasInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodelElementCollection>(item["twin0"].ToString());
                }
                var rel = item["rel"]["$relationshipName"].ToString();
                var dataTwin1 = item["twin1"];

                DeserializeAdtResponse(rel, dataTwin1, adtSmeCollectionInformation.GeneralAasInformation.ConcreteAasInformation);
                await DeserializeAdtResponseForSubmodelOrSmeCollection(rel, dataTwin1, adtSmeCollectionInformation);
            }

            if (adtSmeCollectionInformation.GeneralAasInformation.RootElement.IdShort == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                items = _client.QueryAsync<JsonObject>($"Select twin from digitaltwins twin where twin.$dtId = '{twinId}'");
                await foreach (var item in items)
                {
                    adtSmeCollectionInformation.GeneralAasInformation.RootElement = JsonSerializer.Deserialize<AdtSubmodelElementCollection>(item["twin"].ToString());
                }
            }

            adtSmeCollectionInformation.GeneralAasInformation.definitionsAndSemantics = await
                GetAllDescriptionsForSubmodelElements(twinId);

            return adtSmeCollectionInformation;

        }

        public async Task<DefinitionsAndSemantics> GetAllDescriptionsForSubmodelElements(string rootTwinId)
        {
            var definitionsAndSemantik = new DefinitionsAndSemantics();
            string queryString = "Select sme.$dtId as smeTwinId, twin2 from digitaltwins match (twin0)-[rel1]->(sme)-[*..9]->(twin2) " +
                                 $"where twin0.$dtId='{rootTwinId}' and " +
                                 "rel1.$relationshipName in ['submodelElement','value']  and " +
                                 "not sme.$metadata.$model='dtmi:digitaltwins:aas:SubmodelElementCollection;1'";
            var twinIds = new List<string>();
            var tempDictToCheckDuplicates = new Dictionary<string, string>();

            var items = _client.QueryAsync<JsonObject>(queryString);
            await foreach (var item in items)
            {
                var type = item["twin2"]["$metadata"]["$model"].ToString();
                var twinId = item["twin2"]["$dtId"].ToString();
                var smeTwinId = item["smeTwinId"].ToString();
                twinIds.Add(twinId);
                if (!tempDictToCheckDuplicates.ContainsKey(smeTwinId))
                {
                    twinIds.Add(smeTwinId);
                    tempDictToCheckDuplicates.Add(smeTwinId, "");
                }

                var data = item["twin2"].ToString();
                if (type == AdtAasOntology.MODEL_REFERENCE)
                {
                    if (definitionsAndSemantik.References.ContainsKey(twinId) == false)
                    {
                        var adtModel = JsonSerializer.Deserialize<AdtReference>(data);
                        definitionsAndSemantik.References.Add(twinId, adtModel);
                    }
                }
                else if (type == AdtAasOntology.MODEL_DATASPECIEC61360)
                {
                    if (definitionsAndSemantik.Iec61360s.ContainsKey(twinId) == false)
                    {

                        definitionsAndSemantik.Iec61360s.Add(twinId,
                            JsonSerializer.Deserialize<AdtDataSpecificationIEC61360>(data));
                    }
                }
                else if (type == AdtAasOntology.MODEL_CONCEPTDESCRIPTION)
                {
                    if (definitionsAndSemantik.ConceptDescriptions.ContainsKey(twinId) == false)
                    {
                        definitionsAndSemantik.ConceptDescriptions.Add(twinId,
                            JsonSerializer.Deserialize<AdtConceptDescription>(data));
                    }

                }

            }

            definitionsAndSemantik.Relationships = await GetAllRelationshipsForTwinIds(twinIds);

            return definitionsAndSemantik;
        }


        public async Task<Dictionary<string, List<BasicRelationship>>> GetAllRelationshipsForTwinIds(List<string> twinIds)
        {
            var relationshipDict = new Dictionary<string, List<BasicRelationship>>();


            int j = 0;
            int FirstIndexOfThisIteration = 0;
            while (j < twinIds.Count)

            {
                FirstIndexOfThisIteration = j;
                var relationshipIdsForAdt = "'" + twinIds[j] + "'";
                for (j++; j < twinIds.Count && QueryContentWithinBoundaries(relationshipIdsForAdt, twinIds[j], j - FirstIndexOfThisIteration); j++)
                {
                    relationshipIdsForAdt += ",'" + twinIds[j] + "'";
                }

                var queryString = $"Select * from Relationships r where r.$sourceId In [{relationshipIdsForAdt}]";

                var relationships = _client.QueryAsync<BasicRelationship>(queryString);
                await foreach (var relationship in relationships)
                {
                    if (relationshipDict.ContainsKey(relationship.SourceId))
                    {
                        relationshipDict[relationship.SourceId].Add(relationship);
                    }
                    else
                    {
                        relationshipDict.Add(relationship.SourceId,
                            new List<BasicRelationship> { relationship });
                    }
                }
            }
            return relationshipDict;
        }

        private bool QueryContentWithinBoundaries(string currentElements, string newElement, int index)
        {
            var MAX_STRING_LENGTH = 7500;
            var MAX_NUMBER_OF_ELEMENTS_PER_IN_OPERATION = 100;
            return index < 100 && currentElements.Length + newElement.Length < MAX_STRING_LENGTH;
        }
    }



}
