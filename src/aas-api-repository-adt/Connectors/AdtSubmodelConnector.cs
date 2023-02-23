using System.Text.Json;
using System.Text.Json.Nodes;
using AAS.ADT;
using AAS.ADT.Models;
using AAS.API.Services.ADT;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelConnector : IAdtSubmodelConnector
    {
        private readonly DigitalTwinsClient _client;
        private readonly ILogger<AdtSubmodelConnector> _logger;


        public AdtSubmodelConnector(DigitalTwinsClientFactory adtClientFactory, ILogger<AdtSubmodelConnector> logger)
        {
            _logger = logger ??
                      throw new ArgumentNullException(nameof(logger));
            _client = adtClientFactory.CreateClient();
        }

        private AdtSubmodelConnector(DigitalTwinsClient client, ILogger<AdtSubmodelConnector> logger)
        {
            _client = client;
            _logger = logger ??
                      throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> GetAllInformationForSubmodelWithTwinId(string twinId)
        {
            var adtSubmodelInformation = new AdtSubmodelAndSmcInformation<AdtSubmodel>();
            await GetAdtSubmodelInformation(twinId, adtSubmodelInformation);

            return adtSubmodelInformation;
        }

        private async Task<AdtSubmodel> GetAdtSubmodelInformation(string twinId,
            AdtSubmodelAndSmcInformation<AdtSubmodel> information)
        {
            var items = GetAllTwinsDirectlyRelatedToTwinWithId(twinId);

            var rootElement = await DeserializeRootElementAnswer<AdtSubmodel>(items, information.DefinitionsAndSemantics, information.AdtSubmodelElements);

            if (rootElement.Id == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                rootElement = await GetTwinFromSingleQuery<AdtSubmodel>(twinId);
            }

            await AddAllSmesExceptSmeCollections(twinId, information.DefinitionsAndSemantics);
            information.RootElement = rootElement;

            return rootElement;

        }

        private AsyncPageable<JsonObject> GetAllTwinsDirectlyRelatedToTwinWithId(string twinId)
        {
            string queryString = "Select twin0, rel, twin1 from digitaltwins match (twin0)-[rel]->(twin1) " +
                                 $"where twin0.$dtId='{twinId}'";
            return _client.QueryAsync<JsonObject>(queryString);
        }

        private async Task<T> DeserializeRootElementAnswer<T>(AsyncPageable<JsonObject> items,
            DefinitionsAndSemantics definitionsAndSemantics, AdtSubmodelElements adtSubmodelElements)
            where T : AdtReferable, new()
        {
            T rootElement = new();
            var i = 0;
            var smeCollectionTasks = new List<Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>>>();
            await foreach (var item in items)
            {
                var dataTwin1 = item["twin1"];
                if (i == 0)
                {
                    rootElement = JsonSerializer.Deserialize<T>(item["twin0"].ToString());
                }

                var rel = item["rel"]["$relationshipName"].ToString();
                AddDefinitionsAndSemanticsIfApplicable(item, definitionsAndSemantics);
                AddSubmodelElementIfApplicable(dataTwin1, adtSubmodelElements);

                var model = dataTwin1["$metadata"]["$model"].ToString();

                if (model == AdtAasOntology.MODEL_SUBMODELELEMENTCOLLECTION)
                    smeCollectionTasks.Add(GetAllSubmodelElementCollectionInformation(dataTwin1["$dtId"].ToString()));
                i++;
            }

            while (smeCollectionTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(smeCollectionTasks);
                var adtSmeCollection = await finishedTask;
                adtSubmodelElements.smeCollections.Add(adtSmeCollection);
                smeCollectionTasks.Remove(finishedTask);
            }

            return rootElement;
        }

        private void AddDefinitionsAndSemanticsIfApplicable(JsonObject? item,
            DefinitionsAndSemantics definitionsAndSemantics)
        {
            AddTwinIfItIsPartOfDefinitionsAndSemantics(item["twin1"], definitionsAndSemantics);
            AddRelationship(JsonSerializer.Deserialize<BasicRelationship>(item["rel"].ToString()), definitionsAndSemantics);
        }

        private void AddTwinIfItIsPartOfDefinitionsAndSemantics(JsonNode twin, DefinitionsAndSemantics definitionsAndSemantics)
        {
            var type = twin["$metadata"]["$model"].ToString();
            var data = twin.ToString();
            var twinId = twin["$dtId"].ToString();
            if (type == AdtAasOntology.MODEL_REFERENCE)
            {
                if (definitionsAndSemantics.References.ContainsKey(twinId) == false)
                {
                    var adtModel = JsonSerializer.Deserialize<AdtReference>(data);
                    definitionsAndSemantics.References.Add(twinId, adtModel);
                }
            }
            else if (type == AdtAasOntology.MODEL_DATASPECIEC61360)
            {
                if (definitionsAndSemantics.Iec61360s.ContainsKey(twinId) == false)
                {

                    definitionsAndSemantics.Iec61360s.Add(twinId,
                        JsonSerializer.Deserialize<AdtDataSpecificationIEC61360>(data));
                }
            }
            else if (type == AdtAasOntology.MODEL_CONCEPTDESCRIPTION)
            {
                if (definitionsAndSemantics.ConceptDescriptions.ContainsKey(twinId) == false)
                {
                    definitionsAndSemantics.ConceptDescriptions.Add(twinId,
                        JsonSerializer.Deserialize<AdtConceptDescription>(data));
                }
            }
        }

        private void AddRelationship(BasicRelationship? relationship, DefinitionsAndSemantics definitionsAndSemantics)
        {
            if (relationship == null || relationship.SourceId == null)
            {
                return;
            }
            if (definitionsAndSemantics.Relationships.ContainsKey(relationship.SourceId))
            {
                definitionsAndSemantics.Relationships[relationship.SourceId].Add(relationship);
            }
            else
            {
                definitionsAndSemantics.Relationships.Add(relationship.SourceId,
                    new List<BasicRelationship> { relationship });
            }
        }

        private void AddSubmodelElementIfApplicable(JsonNode dataTwin, AdtSubmodelElements adtSubmodelElements)
        {
            var model = dataTwin["$metadata"]["$model"].ToString();
            if (model == AdtAasOntology.MODEL_PROPERTY)
                adtSubmodelElements.properties.Add(JsonSerializer.Deserialize<AdtProperty>(dataTwin));
            else if (model == AdtAasOntology.MODEL_FILE)
                adtSubmodelElements.files.Add(JsonSerializer.Deserialize<AdtFile>(dataTwin));
        }

        private async Task<T> GetTwinFromSingleQuery<T>(string twinId) where T : AdtBase, new()
        {
            AsyncPageable<JsonObject> items = _client.QueryAsync<JsonObject>($"Select twin from digitaltwins twin where twin.$dtId = '{twinId}'");
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

        private async Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAllSubmodelElementCollectionInformation(
            string twinId)
        {
            var submodelElementCollectionConnector = new AdtSubmodelConnector(_client, _logger);

            return await submodelElementCollectionConnector.GetAdtSmeCollectionInformation(twinId);

        }

        private async Task<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> GetAdtSmeCollectionInformation(string twinId)
        {
            var information = new AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>();
            var items = GetAllTwinsDirectlyRelatedToTwinWithId(twinId);
            var rootElement = await DeserializeRootElementAnswer<AdtSubmodelElementCollection>(items, information.DefinitionsAndSemantics, information.AdtSubmodelElements);

            if (rootElement.IdShort == null)
            {
                // no Twins related to this Submodel -> the Query Response was Empty
                rootElement = await GetTwinFromSingleQuery<AdtSubmodelElementCollection>(twinId);
            }

            await AddAllSmesExceptSmeCollections(twinId, information.DefinitionsAndSemantics);
            information.RootElement = rootElement;
            return information;
        }

        private async Task AddAllSmesExceptSmeCollections(string rootTwinId,
            DefinitionsAndSemantics definitionsAndSemantics)
        {
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

                AddTwinIfItIsPartOfDefinitionsAndSemantics(item["twin2"], definitionsAndSemantics);
            }

            await AddAllRelationshipsForTwinIds(twinIds, definitionsAndSemantics);
        }

        private async Task AddAllRelationshipsForTwinIds(List<string> twinIds,
            DefinitionsAndSemantics definitionsAndSemantics)
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
                    AddRelationship(relationship, definitionsAndSemantics);
                }
            }
        }

        private bool QueryContentWithinBoundaries(string currentElements, string newElement, int index)
        {
            var MAX_STRING_LENGTH = 7500;
            var MAX_NUMBER_OF_ELEMENTS_PER_IN_OPERATION = 100;
            return index < MAX_NUMBER_OF_ELEMENTS_PER_IN_OPERATION && currentElements.Length + newElement.Length < MAX_STRING_LENGTH;
        }
    }
}
