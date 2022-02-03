using AdminShellNS;
using Azure;
using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.ADT
{
    public abstract class AbstractADTCommand
    {
        protected readonly DigitalTwinsClient dtClient;

        public AbstractADTCommand(DigitalTwinsClient adtClient)
        {
            this.dtClient = adtClient;
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
            return rel.Name != "key";
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
