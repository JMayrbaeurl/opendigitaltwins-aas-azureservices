using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AAS.ADT
{
    public class ADTAASRepo : IAASRepo
    {
        private readonly DigitalTwinsClient _dtClient;

        private readonly ILogger _logger;

        public ADTAASRepo(DigitalTwinsClient dtClient, ILogger<ADTAASRepo> logger)
        {
            _dtClient = dtClient;
            _logger = logger;
        }


        public async Task<List<string>> FindLinkedReferences()
        {
            _logger.LogDebug("FindLinkedReferences() called");

            List<string> result = new List<string>();

            // Query for all Reference instances that are not global or a package fragment
            string queryString = $"SELECT * FROM digitaltwins where is_of_model('{AdtAasOntology.MODEL_REFERENCE}')" +
                $" and key1.type != '{KeyTypes.GlobalReference}' and key1.type != '{KeyTypes.FragmentReference}'";

            _logger.LogDebug($"Now querying for identifiable with {queryString}");

            AsyncPageable<BasicDigitalTwin> queryResult = _dtClient.QueryAsync<BasicDigitalTwin>(queryString);

            await foreach (BasicDigitalTwin twin in queryResult)
            {
                result.Add(twin.Id);
            }

            return result;
        }

        public async Task<List<string>> FindReferenceElements()
        {
            _logger.LogDebug("FindReferenceElements() called");

            List<string> result = new List<string>();

            // Query for all ReferenceElement instances that are not global or a package fragment
            string queryString = $"SELECT * FROM digitaltwins where is_of_model('{AdtAasOntology.MODEL_REFERENCEELEMENT}')" +
                $" and key1.type != '{KeyTypes.GlobalReference}' and key1.type != '{KeyTypes.FragmentReference}'";

            _logger.LogDebug($"Now querying for Reference elements with {queryString}");

            AsyncPageable<BasicDigitalTwin> queryResult = _dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in queryResult)
            {
                result.Add(twin.Id);
            }

            return result;
        }

        public async Task<string> FindTwinForReference(Reference reference)
        {

            if (reference == null)
                throw new ArgumentNullException("Parameter 'reference' must not be null");

            _logger.LogDebug($"FindTwinForReference called for reference {reference}");
            
            if (reference.Keys == null)
                throw new ArgumentNullException("Reference must contain at least one key");

            Key firstKey = reference.Keys[0];
            var identifiableElements = new List<KeyTypes>()
            {
                KeyTypes.AssetAdministrationShell,
                KeyTypes.Submodel,
                KeyTypes.ConceptDescription
            };
            if (!(identifiableElements.Contains(firstKey.Type)))
                throw new ArgumentException($"First key of reference '{firstKey}' must refer to an Identifiable element");

            _logger.LogDebug($"Trying to find Twin with keys '{reference.Keys}'");

            // Find the Identifiable first
            BasicDigitalTwin identifiableTwinData = await GetIdentifiableTwinIfPresent(firstKey);

            if (identifiableTwinData == null) 
                return null;
            
            // If there is just one Key then the identifiable is the one that is referenced
            if (reference.Keys.Count == 1)
                return identifiableTwinData.Id;

            // If there are multiple keys then the referable of the last key in the reference is referenced
            // See the Details of the Asset Administration Shell pt.1 -> References
            return await GetReferableTwinIdIfPresent(reference, identifiableTwinData.Id);
        }

        private async Task<BasicDigitalTwin> GetIdentifiableTwinIfPresent(Key referenceKey)
        {
            BasicDigitalTwin identifiableTwinData = null;
            string queryString =
                $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL('{AdtAasOntology.MODEL_IDENTIFIABLE}') " +
                $"AND id = '{referenceKey.Value}'";

            _logger.LogDebug($"Now querying for identifiable with {queryString}");

            AsyncPageable<BasicDigitalTwin> queryResult = _dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in queryResult)
            {
                identifiableTwinData = twin;
                break;
            }

            return identifiableTwinData;
        }

        private async Task<string> GetReferableTwinIdIfPresent(Reference reference, string identifiableTwinId)
        {
            string[] projections =
            {
                "identifiable", "referable1", "referable2", "referable3", "referable4", "referable5", "referable6",
                "referable7", "referable8", "referable9"
            };
            string[] usedProjections = new string[reference.Keys.Count];
            Array.Copy(projections, usedProjections, reference.Keys.Count);

            var queryString = GetQueryStringToQueryIdentifiableAndAllReferablesFromAReference(reference, identifiableTwinId,usedProjections, projections);

            _logger.LogDebug("Now querying for twins with: " + queryString);

            BasicDigitalTwin referableTwinData = null;
            var queryResult = _dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in queryResult)
            {
                referableTwinData = twin;
                break;
            }

            if (referableTwinData == null)
                return null;

            JObject referableTwin = (JObject)JsonConvert.DeserializeObject(referableTwinData
                .Contents[usedProjections[usedProjections.Length - 1]].ToString());

            _logger.LogDebug($"Found referred twin with id '{(string)referableTwin["$dtId"]}'");

            return (string)referableTwin["$dtId"];
        }

        private string GetQueryStringToQueryIdentifiableAndAllReferablesFromAReference(Reference reference,
            string identifiableTwinId, string[] usedProjections, string[] projections)
        {
            string[] relationships =
            {
                "(identifiable)", "(referable1)", "(referable2)", "(referable3)", "(referable4)", "(referable5)",
                "(referable6)", "(referable7)", "(referable8)", "(referable9)"
            };
            string[] usedRelationships = new string[reference.Keys.Count];
            Array.Copy(relationships, usedRelationships, reference.Keys.Count);

            string queryString = $"SELECT {usedProjections[^1]} FROM DIGITALTWINS " +
                                 $"MATCH{string.Join("-[]->", usedRelationships)} " +
                                 $"WHERE identifiable.$dtId = '{identifiableTwinId}'";
            for (int i = 1; i < reference.Keys.Count; i++)
            {
                queryString +=
                    $" AND {projections[i]}.idShort = '{reference.Keys[i].Value}' AND IS_OF_MODEL({projections[i]}, '{AdtAasOntology.KEYS[reference.Keys[i].Type.ToString()]}')";
            }

            return queryString;
        }


    }
}
