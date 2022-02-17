using AAS.AASX.CmdLine.ADT;
using AdminShellNS;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine
{
    public class ADTAASRepo : IAASRepo
    {
        private readonly DigitalTwinsClient dtClient;

        private readonly ILogger _logger;

        public ADTAASRepo(DigitalTwinsClient dtClient, ILogger<ADTAASRepo> logger)
        {
            this.dtClient = dtClient;
            _logger = logger;
        }

        public async Task<List<string>> FindLinkedReferences()
        {
            _logger.LogDebug("FindLinkedReferences() called");

            List<string> result = new List<string>();

            // Query for all Reference instances that are not global or a package fragment
            string queryString = $"SELECT * FROM digitaltwins where is_of_model('{ADTAASOntology.MODEL_REFERENCE}')" +
                $" and key1.key != '{Key.GlobalReference}' and key1.key != '{Key.FragmentReference}'";

            _logger.LogDebug($"Now querying for identifiable with {queryString}");

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
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
            string queryString = $"SELECT * FROM digitaltwins where is_of_model('{ADTAASOntology.MODEL_REFERENCEELEMENT}')" +
                $" and key1.key != '{Key.GlobalReference}' and key1.key != '{Key.FragmentReference}'";

            _logger.LogDebug($"Now querying for Reference elements with {queryString}");

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in queryResult)
            {
                result.Add(twin.Id);
            }

            return result;
        }

        public async Task<string> FindTwinForReference(AdminShellV20.Reference reference)
        {
            _logger.LogDebug($"FindTwinForReference called for reference {reference.ToString()}");

            if (reference == null)
                throw new ArgumentNullException("Parameter 'reference' must not be null");

            if (reference.Count == 0)
                throw new ArgumentException("Reference must contain at least one key");

            Key firstKey = reference.First;
            if (!(Key.IdentifiableElements.Contains(firstKey.type)))
                throw new ArgumentException($"First key of reference '{firstKey.ToString()}' must refer to an Identifiable element");

            _logger.LogDebug($"Trying to find Twin with keys '{reference.Keys.ToString()}'");

            if (!firstKey.local)
                return null;
            else
            {
                if (firstKey.idType == Key.IdShort || firstKey.idType == Key.FragmentId)
                    throw new ArgumentException($"First key of reference '{firstKey.ToString()}' must not be an IdShort or FragmentId");
                else
                {
                    foreach(var key in reference.Keys.GetRange(1, reference.Keys.Count-1))
                    {
                        if (key.idType != Key.IdShort)
                            throw new ArgumentException($"Except for first key all remaining keys have to be an IdShort. Found '{key.idType}'");
                    }
                }
            }

            // Find the Identifiable first
            BasicDigitalTwin identifiableTwinData = null;
            string keyIdType = AASUtils.URITOIRI(firstKey.idType);
            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL('{ADTAASOntology.MODEL_IDENTIFIABLE}') " +
                $"AND identification.idType = '{keyIdType}' AND identification.id = '{firstKey.value}'";

            _logger.LogDebug($"Now querying for identifiable with {queryString}");

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in queryResult)
            {
                identifiableTwinData = twin;
                break;
            }

            if (identifiableTwinData != null)
            {
                if (reference.Keys.Count == 1)
                    return identifiableTwinData.Id;
                else
                {
                    // SELECT identifiable, referable FROM DIGITALTWINS MATCH(identifiable)-[]->(referable) WHERE identifiable.$dtId = 'Shell_5a0f3d6d-c21e-4704-b09e-eae6fa7dc45b' AND referable.idShort = 'Nameplate' AND IS_OF_MODEL(referable, 'dtmi:digitaltwins:aas:Submodel;1')
                    // Now process the local keys
                    string[] projections = 
                        { "identifiable", "referable1", "referable2", "referable3", "referable4", "referable5", "referable6", "referable7", "referable8", "referable9" };
                    string[] usedProjections = new string[reference.Keys.Count]; Array.Copy(projections, usedProjections, reference.Keys.Count);
                    string[] relationships =
                        { "(identifiable)", "(referable1)", "(referable2)", "(referable3)", "(referable4)", "(referable5)", "(referable6)", "(referable7)", "(referable8)", "(referable9)" };
                    string[] usedRelationships = new string[reference.Keys.Count]; Array.Copy(relationships, usedRelationships, reference.Keys.Count);

                    queryString = $"SELECT {usedProjections[usedProjections.Length-1]} FROM DIGITALTWINS MATCH{String.Join("-[]->", usedRelationships)}";
                    for(int i = 0; i < reference.Keys.Count; i++)
                    {
                        if (i == 0)
                        {
                            queryString += $" WHERE identifiable.$dtId = '{identifiableTwinData.Id}'";
                        } else
                        {
                            queryString += $" AND {projections[i]}.idShort = '{reference.Keys[i].value}' AND IS_OF_MODEL({projections[i]}, '{ADTAASOntology.KEYS[reference.Keys[i].type]}')";
                        }
                    }

                    _logger.LogDebug("Now querying for twins with: " + queryString);

                    BasicDigitalTwin referableTwinData = null;
                    queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                    await foreach (BasicDigitalTwin twin in queryResult)
                    {
                        referableTwinData = twin;
                        break;
                    }
                    if (referableTwinData != null)
                    {
                        JObject referableTwin = (JObject)JsonConvert.DeserializeObject(referableTwinData.Contents[usedProjections[usedProjections.Length - 1]].ToString());

                        _logger.LogDebug($"Found referred twin with id '{(string)referableTwin["$dtId"]}'");

                        return (string)referableTwin["$dtId"];
                    }
                    else
                        return null;
                }
            }
            else
                return null;
        }
    
        
    }
}
