using System.Text.Json;
using System.Text.Json.Nodes;
using AAS.ADT;
using AAS.ADT.Models;
using AAS.API.Services.ADT;
using Azure;
using Azure.DigitalTwins.Core;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelConnector : IAdtSubmodelConnector
    {
        private readonly DigitalTwinsClient _client;
        private readonly DefinitionsAndSemantics _definitionsAndSemantics = new();
        private readonly AdtSubmodelElements _adtSubmodelElements = new();


        public AdtSubmodelConnector(DigitalTwinsClientFactory adtClientFactory)
        {
            _client = adtClientFactory.CreateClient();
        }

        private AdtSubmodelConnector(DigitalTwinsClient client)
        {
            _client = client;
        }

        public async Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId)
        {
            var rootElement = await GetAdtSubmodelInformation(twinId);
            var adtSubmodelInformation = new AdtSubmodelAndSmcInformation<AdtSubmodel>();

            adtSubmodelInformation.RootElement = rootElement;
            adtSubmodelInformation.DefinitionsAndSemantics = _definitionsAndSemantics;
            adtSubmodelInformation.AdtSubmodelElements = _adtSubmodelElements;
            return adtSubmodelInformation;
        }

        private async Task<AdtSubmodel> GetAdtSubmodelInformation(string twinId) 
        {
            var items = GetAllTwinsDirectlyRelatedToTwinWithId(twinId);

            var rootElement = await DeserializeRootElementAnswer<AdtSubmodel>(items);

            if (rootElement.Id == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                rootElement = await GetTwinFromSingleQuery<AdtSubmodel>(twinId);
            }

            await AddAllSmesExceptSmeCollections(twinId);
            return rootElement;
        }

        


        private AsyncPageable<JsonObject> GetAllTwinsDirectlyRelatedToTwinWithId(string twinId)
        {
            string queryString = "Select twin0, rel, twin1 from digitaltwins match (twin0)-[rel]->(twin1) " +
                                 $"where twin0.$dtId='{twinId}'";
            return _client.QueryAsync<JsonObject>(queryString);
        }

        private async Task<T> DeserializeRootElementAnswer<T>(AsyncPageable<JsonObject> items)
            where T : AdtReferable, new()
        {
            T rootElement = new();

            var i = 0;
            await foreach (var item in items)
            {
                var dataTwin1 = item["twin1"];
                if (i == 0)
                {
                    rootElement = JsonSerializer.Deserialize<T>(item["twin0"].ToString());
                }

                var rel = item["rel"]["$relationshipName"].ToString();
                AddTwinAndRelationshipRelatedToRootElement(item);
                await AddSubmodelElement(rel, dataTwin1);
                i++;
            }

            return rootElement;
        }

        private void AddTwinAndRelationshipRelatedToRootElement(JsonObject? item)
        {
            AddTwin(item["twin1"]);
            AddRelationship(JsonSerializer.Deserialize<BasicRelationship>(item["rel"].ToString()));
        }

        private void AddTwin(JsonNode twin)
        {
            var type = twin["$metadata"]["$model"].ToString();
            var data = twin.ToString();
            var twinId = twin["$dtId"].ToString();
            if (type == AdtAasOntology.MODEL_REFERENCE)
            {
                if (_definitionsAndSemantics.References.ContainsKey(twinId) == false)
                {
                    var adtModel = JsonSerializer.Deserialize<AdtReference>(data);
                    _definitionsAndSemantics.References.Add(twinId, adtModel);
                }
            }
            else if (type == AdtAasOntology.MODEL_DATASPECIEC61360)
            {
                if (_definitionsAndSemantics.Iec61360s.ContainsKey(twinId) == false)
                {

                    _definitionsAndSemantics.Iec61360s.Add(twinId,
                        JsonSerializer.Deserialize<AdtDataSpecificationIEC61360>(data));
                }
            }
            else if (type == AdtAasOntology.MODEL_CONCEPTDESCRIPTION)
            {
                if (_definitionsAndSemantics.ConceptDescriptions.ContainsKey(twinId) == false)
                {
                    _definitionsAndSemantics.ConceptDescriptions.Add(twinId,
                        JsonSerializer.Deserialize<AdtConceptDescription>(data));
                }
            }
        }

        private void AddRelationship(BasicRelationship? relationship)
        {
            if (relationship == null || relationship.SourceId == null)
            {
                return;
            }
            if (_definitionsAndSemantics.Relationships.ContainsKey(relationship.SourceId))
            {
                _definitionsAndSemantics.Relationships[relationship.SourceId].Add(relationship);
            }
            else
            {
                _definitionsAndSemantics.Relationships.Add(relationship.SourceId,
                    new List<BasicRelationship> { relationship });
            }
        }

        private async Task AddSubmodelElement(string relationship, JsonNode dataTwin) 
        {
            if (relationship == "submodelElement" || relationship == "value")
            {
                var model = dataTwin["$metadata"]["$model"].ToString();

                if (model == AdtAasOntology.MODEL_SUBMODELELEMENTCOLLECTION)
                    _adtSubmodelElements.smeCollections.Add(await GetAllSubmodelElementCollectionInformation(dataTwin["$dtId"].ToString()));
                else if (model == AdtAasOntology.MODEL_PROPERTY)
                    _adtSubmodelElements.properties.Add(JsonSerializer.Deserialize<AdtProperty>(dataTwin));
                else if (model == AdtAasOntology.MODEL_FILE)
                    _adtSubmodelElements.files.Add(JsonSerializer.Deserialize<AdtFile>(dataTwin));
                else
                    throw new AdtModelNotSupported($"Unsupported AdtModel of Type {model}");
            }
        }

        private async Task<T> GetTwinFromSingleQuery<T>(string twinId) where T : AdtBase, new()
        {
            AsyncPageable<JsonObject> items;
            items = _client.QueryAsync<JsonObject>($"Select twin from digitaltwins twin where twin.$dtId = '{twinId}'");
            await foreach (var item in items)
            {
                var jsonTwin = item["twin"].ToString();
                if (jsonTwin == null)
                {
                    continue;
                }

                var twin = JsonSerializer.Deserialize<T>(jsonTwin);
                if (twin == null)
                {
                    continue;
                }

                return twin;
            }

            throw new ArgumentException($"Twin with Id {twinId} does not exist, can't query for");
        }

        public async Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAllSubmodelElementCollectionInformation(
            string twinId)
        {
            var submodelElementCollectionConnector = new AdtSubmodelConnector(_client);

            return await submodelElementCollectionConnector.GetAdtSmeCollectionInformation(twinId);

        }

        private async Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAdtSmeCollectionInformation(string twinId)
        {
            var items = GetAllTwinsDirectlyRelatedToTwinWithId(twinId);

            var rootElement = await DeserializeRootElementAnswer<AdtSubmodelElementCollection>(items);

            if (rootElement.IdShort == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                rootElement = await GetTwinFromSingleQuery<AdtSubmodelElementCollection>(twinId);
            }

            await AddAllSmesExceptSmeCollections(twinId);
            var adtSmeCollectionInformation = new AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>();
            adtSmeCollectionInformation.RootElement = rootElement;
            adtSmeCollectionInformation.DefinitionsAndSemantics = _definitionsAndSemantics;
            adtSmeCollectionInformation.AdtSubmodelElements = _adtSubmodelElements;
            return adtSmeCollectionInformation;
        }

        private async Task AddAllSmesExceptSmeCollections(string rootTwinId)
        {
            //var definitionsAndSemantik = new DefinitionsAndSemantics();
            string queryString = "Select sme.$dtId as smeTwinId, twin2 from digitaltwins match (twin0)-[rel1]->(sme)-[*..9]->(twin2) " +
                                 $"where twin0.$dtId='{rootTwinId}' and " +
                                 "rel1.$relationshipName in ['submodelElement','value']  and " +
                                 "not sme.$metadata.$model='dtmi:digitaltwins:aas:SubmodelElementCollection;1'";
            var twinIds = new List<string>();
            var tempDictToCheckDuplicates = new Dictionary<string, string>();

            var items = _client.QueryAsync<JsonObject>(queryString);
            await foreach (var item in items)
            {
                var twinId = item["twin2"]["$dtId"].ToString();
                var smeTwinId = item["smeTwinId"].ToString();
                twinIds.Add(twinId);
                if (!tempDictToCheckDuplicates.ContainsKey(smeTwinId))
                {
                    twinIds.Add(smeTwinId);
                    tempDictToCheckDuplicates.Add(smeTwinId, "");
                }
                ;
                AddTwin(item["twin2"]);
            }

            await AddAllRelationshipsForTwinIds(twinIds);
        }

        private async Task AddAllRelationshipsForTwinIds(List<string> twinIds)
        {
            int j = 0;
            int FirstIndexOfThisIteration;
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
                    AddRelationship(relationship);
                }
            }
        }

        private bool QueryContentWithinBoundaries(string currentElements, string newElement, int index)
        {
            var MAX_STRING_LENGTH = 7500;
            var MAX_NUMBER_OF_ELEMENTS_PER_IN_OPERATION = 100;
            return index < 100 && currentElements.Length + newElement.Length < MAX_STRING_LENGTH;
        }
    }
}
