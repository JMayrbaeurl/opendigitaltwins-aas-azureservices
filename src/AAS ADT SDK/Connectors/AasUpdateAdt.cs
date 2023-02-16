using System.Collections.Generic;
using System.Threading.Tasks;
using AAS.API.Services.ADT;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;

namespace AAS.ADT
{
    public class AasUpdateAdt : IAasUpdateAdt
    {
        private readonly ILogger<AasUpdateAdt> _logger;
        private readonly IAasWriteSubmodel _writeSubmodel;
        private readonly IAasDeleteAdt _deleteAdt;
        private readonly DigitalTwinsClient _digitalTwinsClient;
        private readonly IAasWriteConnector _aasWriteConnector;
        private readonly IAasWriteAssetAdministrationShell _writeShell;


        public AasUpdateAdt(ILogger<AasUpdateAdt> logger, IAasWriteSubmodel writeSubmodel, IAasDeleteAdt deleteAdt,
            DigitalTwinsClientFactory clientFactory, IAasWriteConnector aasWriteConnector, IAasWriteAssetAdministrationShell writeShell)
        {
            _logger = logger;
            _writeSubmodel = writeSubmodel;
            _deleteAdt = deleteAdt;
            _aasWriteConnector = aasWriteConnector;
            _writeShell = writeShell;
            _digitalTwinsClient = clientFactory.CreateClient();
        }

        public async Task UpdateFullSubmodel(string submodelTwinId, Submodel submodel)
        {
            var incomingRelationships = GetAllIncomingRelationships(submodelTwinId);

            await _deleteAdt.DeleteTwin(submodelTwinId);
            var newSubmodelTwinId = await _writeSubmodel.CreateSubmodel(submodel);

            _logger.LogInformation($"Adding again Relationships to updated Submodel {submodel.Id}");

            await RecreateIncomingRelationshipsForTwin(newSubmodelTwinId, incomingRelationships);

        }

        

        private List<BasicRelationship> GetAllIncomingRelationships(string twinId)
        {
            var queryResult = _digitalTwinsClient.Query<BasicRelationship>(
                            $"SELECT * FROM RELATIONSHIPS r WHERE r.$targetId = '{twinId}'");
            return GetRelationshipsFromQueryResult(queryResult);
        }

        private List<BasicRelationship> GetRelationshipsFromQueryResult(Azure.Pageable<BasicRelationship> queryResult)
        {
            var incomingRelationships = new List<BasicRelationship>();
            foreach (var tmp in queryResult)
            {
                incomingRelationships.Add(tmp);
            }

            return incomingRelationships;
        }

        private async Task RecreateIncomingRelationshipsForTwin(string targetTwinId, List<BasicRelationship> relationships)
        {
            var tasks = new List<Task>();
            foreach (var incomingRelationship in relationships)
            {
                tasks.Add(_aasWriteConnector.DoCreateOrReplaceRelationshipAsync(incomingRelationship.SourceId,
                    incomingRelationship.Name, targetTwinId));
            }

            await Task.WhenAll(tasks);
        }

        public async Task UpdateFullShell(string shellTwinId, AssetAdministrationShell shell)
        {
            var submodelRelationships = GetAllRelationshipsFromShellToSubmodels(shellTwinId);
            await _deleteAdt.DeleteTwin(shellTwinId);
            var newShellTwinId = await _writeShell.CreateShell(shell);
            await TaskRecreateSubmodelRelationships(newShellTwinId, submodelRelationships);
        }

        private List<BasicRelationship> GetAllRelationshipsFromShellToSubmodels(string shellTwinId)
        {
            var queryResult = _digitalTwinsClient.Query<BasicRelationship>(
                $"SELECT * FROM RELATIONSHIPS r WHERE r.$sourceId = '{shellTwinId}' and r.$relationshipName='submodel'");
            return GetRelationshipsFromQueryResult(queryResult);
        }

        private async Task TaskRecreateSubmodelRelationships(string shellTwinId, List<BasicRelationship> relationships)
        {
            var tasks = new List<Task>();
            foreach (var relationship in relationships)
            {
                tasks.Add(_writeShell.CreateSubmodelReference(shellTwinId, relationship.TargetId));
            }

            await Task.WhenAll(tasks);
        }


    }
}
