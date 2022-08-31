using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine.ADT
{
    public abstract class AbstractADTCommand
    {
        protected readonly DigitalTwinsClient dtClient;

        protected readonly ILogger _logger;

        protected readonly IAASRepo aasRepo;

        public AbstractADTCommand(DigitalTwinsClient adtClient, ILogger log, IAASRepo repo)
        {
            this.dtClient = adtClient;
            this._logger = log;
            this.aasRepo = repo;    
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
                $"AND dt.id = '{conceptDescription.identification.id}'";

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
                $"AND dt.id = '{shell.identification.id}'";

            AsyncPageable<BasicDigitalTwin> queryResult = dtClient.QueryAsync<BasicDigitalTwin>(queryString);
            var enumerator = queryResult.GetAsyncEnumerator();
            if (await enumerator.MoveNextAsync())
                result = enumerator.Current.Id;

            return result;
        }

        public async Task<string> FindTwinForReference(Reference reference)
        {
            return await this.aasRepo.FindTwinForReference(reference);
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
