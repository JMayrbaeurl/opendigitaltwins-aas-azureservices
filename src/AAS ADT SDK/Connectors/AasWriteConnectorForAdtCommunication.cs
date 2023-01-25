using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace AAS.ADT
{
    public class AasWriteConnectorForAdtCommunication : IAasWriteConnector
    {
        private readonly DigitalTwinsClient _dtClient;
        private readonly ILogger<AasWriteConnectorForAdtCommunication> _logger;
        private readonly IAASRepo _repo;
        private readonly IMapper _mapper;

        public AasWriteConnectorForAdtCommunication(DigitalTwinsClient dtClient, ILogger<AasWriteConnectorForAdtCommunication> logger,
            IAASRepo repo, IMapper mapper)
        {
            _dtClient = dtClient;
            _logger = logger;
            _repo = repo;
            _mapper = mapper;
        }


        public async Task DoCreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twinData)
        {
            
            if (twinData == null)
            {
                return;
            }
            
            try
            {
                await _dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);

            }
            catch (RequestFailedException ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        $"Exception on creating twin with id '{twinData.Id}' and model '{twinData.Metadata.ModelId}': {ex.Message}");

                throw new ImportException(
                    $"Exception on creating twin with id '{twinData.Id}' and model '{twinData.Metadata.ModelId}': {ex.Message}",
                    ex);
            }
        }

        public async Task DoCreateOrReplaceRelationshipAsync(string sourceId,
            string relName, string targetId)
        {
            if (string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(relName) || string.IsNullOrEmpty(targetId))
            {
                return;
            }

            var relationship = new BasicRelationship
            {
                TargetId = targetId,
                Name = relName
            };

            string relId = $"{sourceId}-{relName}->{targetId}";
            try
            {
                await _dtClient.CreateOrReplaceRelationshipAsync(sourceId, relId, relationship);
                _logger.LogInformation($"Created relationship '{relId}'");
            }
            catch (RequestFailedException ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(
                        $"Exception on creating relationship with name'{relName}' for twin with id '{sourceId}' and target id '{targetId}': {ex.Message}");

                throw new ImportException(
                    $"Exception on creating relationship with name'{relName}' for twin with id '{sourceId}' and target id '{targetId}': {ex.Message}",
                    ex);
            }
        }

    }
}
