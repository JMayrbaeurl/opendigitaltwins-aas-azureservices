using AdminShellNS;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.ADT
{
    public abstract class AbstractADTCommand
    {
        protected readonly DigitalTwinsClient dtClient;

        protected readonly ILogger _logger;

        public AbstractADTCommand(DigitalTwinsClient adtClient, ILogger log)
        {
            this.dtClient = adtClient;
            this._logger = log;
        }

        public async Task<bool> DeleteShell(AdministrationShell shell)
        {
            bool result = false;

            string shellDtId = await FindDtIdForShell(shell);
            if (shellDtId != null)
            {
                await DeleteCompositeTwin(shellDtId).ConfigureAwait(false);

                result = true;
            }

            return result;
        }

        public async Task DeleteCompositeTwin(string dtId)
        {
            // Delete all incoming relationships
            await FindAndDeleteIncomingRelationshipsAsync(dtId);

            // Delete other composite relationships
            AsyncPageable<BasicRelationship> rels = dtClient.GetRelationshipsAsync<BasicRelationship>(dtId);

            List<string> componentDtIds = new List<string>();
            await foreach (BasicRelationship rel in rels)
            {
                await dtClient.DeleteRelationshipAsync(dtId, rel.Id).ConfigureAwait(false);
                if (IsCompositeRelationship(rel))
                {
                    componentDtIds.Add(rel.TargetId);
                }
            }
            foreach (var componentDtId in componentDtIds)
            {
                await DeleteCompositeTwin(componentDtId).ConfigureAwait(false);
            }

            // Delete shell twin
            await dtClient.DeleteDigitalTwinAsync(dtId);
        }

        protected bool IsCompositeRelationship(BasicRelationship rel)
        {
            // TODO: Currently only well known Relationship names from the AAS Ontology
            // (Reference->Key, RelationshipElement->Referable) are checked. Should be extended in the future
            return rel.Name != "key" && rel.Name != "first" && rel.Name != "second";
        }

        public async Task<bool> ConceptDescriptionExists(ConceptDescription conceptDescription)
        {
            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL(dt, '{ADTAASOntology.MODEL_CONCEPTDESCRIPTION}') " +
                $"AND dt.identification.id = '{conceptDescription.identification.id}' " +
                $"AND dt.identification.idType = '{conceptDescription.identification.idType}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            bool result = await queryResult.GetAsyncEnumerator().MoveNextAsync();

            return result;
        }

        public async Task<bool> ShellExists(AdministrationShell shell)
        {
            string dtId = await FindDtIdForShell(shell);

            return dtId != null;
        }

        public async Task<string> FindDtIdForShell(AdministrationShell shell)
        {
            string result = null;

            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL(dt, '{ADTAASOntology.MODEL_SHELL}') " +
                $"AND dt.identification.id = '{shell.identification.id}' " +
                $"AND dt.identification.idType = '{shell.identification.idType}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            var enumerator = queryResult.GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync())
                result = enumerator.Current.Id;

            return result;
        }

        public async Task<bool> AssetExists(Asset asset)
        {
            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL(dt, '{ADTAASOntology.MODEL_ASSET}') " +
                $"AND dt.identification.id = '{asset.identification.id}' " +
                $"AND dt.identification.idType = '{asset.identification.idType}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            return await queryResult.GetAsyncEnumerator().MoveNextAsync();
        }

        public async Task<string> KeyExists(Key key)
        {
            string result = null;

            string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL('{ADTAASOntology.MODEL_KEY}') " +
                $"AND key = '{key.type}' " + $"AND idType = '{URITOIRI(key.idType)}' " + $"AND value = '{key.value}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            await foreach (BasicDigitalTwin twin in queryResult)
            {
                result = twin.Id;
                break;
            }

            return result;
        }

        public async Task<BasicDigitalTwin> FindTwinForReference(Reference reference)
        {
            if (reference == null)
                throw new ArgumentNullException("Parameter 'reference' must not be null");

            if (reference.Keys.Count > 1)
            {
                _logger.LogWarning($"FindTwinForReference called with Reference that contains multiple keys. Only one key supported.");
            }

            Key key = reference.First;

            _logger.LogDebug($"Trying to find Twin with key '{key}'");

            if (!key.local)
                return null;

            if (key.IsIdType(Key.IdShort))
            {
                string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL('{ADTAASOntology.MODEL_REFERABLE}') " +
                    $"AND idShort = '{key.value}' ";
                AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                await foreach (BasicDigitalTwin twin in queryResult)
                {
                    // TODO: See if there is more than one Referable with the same IdShort
                    return twin;
                }
            } 
            else if (key.IsIdType(Key.FragmentId))
            {
                return null;
            }
            else
            {
                string keyIdType = URITOIRI(key.idType);
                string queryString = $"SELECT * FROM digitaltwins dt WHERE IS_OF_MODEL('{ADTAASOntology.MODEL_IDENTIFIABLE}') " +
                    $"AND identification.idType = '{keyIdType}' AND identification.id = '{key.value}'";
                AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
                await foreach (BasicDigitalTwin twin in queryResult)
                {
                    // TODO: See if there is more than one Referable with the same IdShort
                    return twin;
                }
            }

            return null;
        }

        public static string DescToString(Description desc)
        {
            if (desc == null)
                return default;
            else
            {
                string result = "";
                foreach (var entry in desc.langString)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string LangStringSetIEC61360ToString(LangStringSetIEC61360 langStrs)
        {
            if (langStrs == null)
                return default;
            else
            {
                string result = "";
                foreach (var entry in langStrs)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string LangStringSetToString(LangStringSet langStrs)
        {
            if (langStrs == null)
                return default;
            else
            {
                string result = "";
                foreach (var entry in langStrs.langString)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string StripInvalidTwinIdCharacters(string dtIdProposal)
        {
            string result = dtIdProposal.Trim();

            result = result.Replace(" ", "");
            result = result.Replace("/", "");

            return result;
        }

        public static string URITOIRI(string idType)
        {
            if ("URI".Equals(idType))
                return "IRI";
            else
                return idType;
        }

        protected async Task FindAndDeleteIncomingRelationshipsAsync(string dtId)
        {
            // Find the relationships for the twin

            // GetRelationshipsAsync will throw an error if a problem occurs
            AsyncPageable<IncomingRelationship> incomingRels = dtClient.GetIncomingRelationshipsAsync(dtId);

            await foreach (IncomingRelationship incomingRel in incomingRels)
            {
                await dtClient.DeleteRelationshipAsync(incomingRel.SourceId, incomingRel.RelationshipId).ConfigureAwait(false);
            }
            
        }
    }
}
