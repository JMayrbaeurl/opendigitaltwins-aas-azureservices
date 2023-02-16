using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AAS.API.Services.ADT;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;

namespace AAS.ADT
{
    public class AasDeleteAdt : IAasDeleteAdt
    {
        private readonly DigitalTwinsClient _dtClient;
        private readonly ILogger<AasDeleteAdt> _logger;

        public AasDeleteAdt(DigitalTwinsClientFactory adtClientFactory, ILogger<AasDeleteAdt> logger)
        {
            _logger = logger;
            _dtClient = adtClientFactory.CreateClient();
        }

        public async Task DeleteTwin(string twinId)
        {
            if (string.IsNullOrEmpty(twinId))
            {
                return;
            }

            List<BasicRelationship> allRelationshipsOfTwin = new();
            try
            {
                allRelationshipsOfTwin = GetAllRelationshipsOfTwin(twinId);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error when retrieving all Relationships for twin with Id {twinId}",e);
            }

            try
            {
                await DeleteRelationships(allRelationshipsOfTwin);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error when deleting all Relationships for twin with Id {twinId}", e);
            }

            try
            {
                await _dtClient.DeleteDigitalTwinAsync(twinId);
                
            }
            catch (Exception e)
            {
                _logger.LogError($"Error when deleting twin with ID {twinId}", e);
            }

            _logger.LogInformation($"Deleted twin with Id {twinId} and all incoming and outgoing relationships");
        }

        

        private List<BasicRelationship> GetAllRelationshipsOfTwin(string twinId)
        {
            var allRelationshipsOfTwin = new List<BasicRelationship>();
            
            var incomingRelationships = _dtClient.Query<BasicRelationship>(
                $"SELECT * FROM RELATIONSHIPS r WHERE r.$targetId = '{twinId}'");
            foreach (var incomingRelationship in incomingRelationships)
            {
                allRelationshipsOfTwin.Add(incomingRelationship);
            }

            var outgoingRelationships = _dtClient.Query<BasicRelationship>(
                $"SELECT * FROM RELATIONSHIPS r WHERE r.$sourceId = '{twinId}'");
            foreach (var outgoingRelationship in outgoingRelationships)
            {
                allRelationshipsOfTwin.Add(outgoingRelationship);
            }

            return allRelationshipsOfTwin;
        }

        public async Task DeleteRelationship(string sourceTwinId, string targetTwinId, string relationshipName)
        {
            var relationships = _dtClient.Query<BasicRelationship>(
                $"SELECT * FROM RELATIONSHIPS r WHERE r.$sourceId = '{sourceTwinId}' AND " +
                $"r.$targetId = '{targetTwinId}' AND r.$relationshipName = '{relationshipName}'");
            await DeleteRelationships(relationships.ToList());
        }

        private async Task DeleteRelationships(List<BasicRelationship> relationships)
        {
            var taskList = new List<Task>();
            foreach (var relationship in relationships)
            {
                taskList.Add(_dtClient.DeleteRelationshipAsync(relationship.SourceId, relationship.Id));
            }

            await Task.WhenAll(taskList);
        }
    }
}
