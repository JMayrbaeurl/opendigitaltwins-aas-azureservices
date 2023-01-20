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


        public async Task<Response<BasicDigitalTwin>> DoCreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twinData)
        {
            Response<BasicDigitalTwin> result;
            try
            {
                result = await _dtClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);

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

            return result;
        }

        public async Task<Response<BasicRelationship>> DoCreateOrReplaceRelationshipAsync(string sourceId,
            string relName, string targetId)
        {
            var relationship = new BasicRelationship
            {
                TargetId = targetId,
                Name = relName
            };

            string relId = $"{sourceId}-{relName}->{targetId}";
            Response<BasicRelationship> result;
            try
            {
                result = await _dtClient.CreateOrReplaceRelationshipAsync(sourceId, relId, relationship);
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

            return result;
        }

        public async Task<List<string>> DoCreateReferrableReferenceRelationships(List<string> refList, ISet<string> filteredTwins, List<string> result)
        {
            //TODO: Check if result is necessary as input Parameter. Antipattern where result is inout variable

            if (refList == null || refList.Count == 0)
            {
                return result;
            }

            if (filteredTwins == null)
            {
                result = await DoCreateReferrableReferenceRelationships(refList);
                return result;
            }

            _logger.LogInformation($"Restricting to the following Twins with ids: {String.Join(",", filteredTwins.ToArray())}");


            var filteredRefList = new List<string>();

            foreach (var refEntry in refList)
            {
                if (filteredTwins.Contains(refEntry))
                {
                    filteredRefList.Add(refEntry);
                }
                else
                {
                    _logger.LogDebug($"Skipping Twin with id '{refEntry}'");
                }
            }

            result = await DoCreateReferrableReferenceRelationships(filteredRefList);
            return result;
        }

        private async Task<List<string>> DoCreateReferrableReferenceRelationships(List<string> refList)
        {
            _logger.LogInformation($"Found {refList.Count} Reference instances that will be processed now");

            var result = new List<string>();
            foreach (var refEntry in refList)
            {
                try
                {
                    var createdReference = await TryCreateReferrableReferenceRelationships(refEntry);
                    if (createdReference)
                        result.Add(refEntry);
                }
                catch (RequestFailedException ex)
                {
                    _logger.LogError($"Exception on processing Reference twin instance with id '{refEntry}'. " +
                                     ex.Message);
                }
            }

            return result;
        }

        private async Task<bool> TryCreateReferrableReferenceRelationships(string referableId)
        {
            AdtReference twinData = await _dtClient.GetDigitalTwinAsync<AdtReference>(referableId);
            //TODO: Check if twinData can be null. Otherwise the "if" is obsolete
            if (twinData == null)
                return false;

            _logger.LogDebug(
                $"Reference with model '{twinData.Metadata.ModelId}' and id '{twinData.dtId}' will be processed now");

            var refTwin = _mapper.Map<Reference>(twinData);
            if (refTwin == null)
                return false;

            if (refTwin.Keys[0].Type is KeyTypes.GlobalReference or KeyTypes.FragmentReference)
            {
                _logger.LogDebug($"Skipping Twin with id '{referableId}', because it's of type Global or Fragment");
                return false;
            }

            string referredTwinId = await _repo.FindTwinForReference(refTwin);
            if (referredTwinId != null)
            {
                BasicRelationship response =
                    await DoCreateOrReplaceRelationshipAsync(twinData.dtId, "referredElement", referredTwinId);
                string relId = response.Id;
                _logger.LogInformation($"Created relationship '{relId}' for reference");
                return true;

            }

            _logger.LogDebug(
                $"Cant find twin for Reference with model '{twinData.Metadata.ModelId}' and id '{twinData.dtId}'");


            return false;
        }

    }
}
