﻿using AAS.ADT;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository.Adt
{
    public class ADTAASRepository : AASRepository
    {
        private readonly ADTAASModelFactory _modelFactory;
        private readonly IAdtAasConnector _adtAasConnector;
        private readonly ILogger<ADTAASRepository> _logger;
        private readonly IAasWriteAssetAdministrationShell _writeShell;
        private readonly IAasDeleteAdt _deleteShell;
        private readonly IAasUpdateAdt _updateShell;


        public ADTAASRepository(IAdtAasConnector adtAasConnector, IMapper mapper,
            ILogger<ADTAASRepository> logger, IAasWriteAssetAdministrationShell writeShell, IAasDeleteAdt deleteShell, IAasUpdateAdt updateShell)
        {
            _modelFactory = new ADTAASModelFactory(mapper);
            _adtAasConnector = adtAasConnector;
            _logger = logger ??
                      throw new ArgumentNullException(nameof(logger));
            _writeShell = writeShell;
            _deleteShell = deleteShell;
            _updateShell = updateShell;
        }


        public async Task DeleteAssetAdministrationShellWithId(string aasId)
        {
            try
            {
                var twinId = _adtAasConnector.GetTwinIdForElementWithId(aasId);
                await _deleteShell.DeleteTwin(twinId);
            }
            catch (AdtException e)
            {
                _logger.LogError(e, e.Message);
                throw new AASRepositoryException($"No Shell with Id {aasId} found to delete");
            }
        }

        public async Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShells()
        {
            var ids = GetAllAssetAdministrationShellIds();
            var shells = new List<AssetAdministrationShell>();
            foreach (var id in ids)
            {
                var information = _adtAasConnector.GetAllInformationForAasWithId(id);
                information.rootElement = _adtAasConnector.GetAdtAasForAasWithId(id);

                shells.Add(_modelFactory.GetAas(information));
            }
            return shells;
        }

        public async Task CreateAssetAdministrationShell(AssetAdministrationShell shell)
        {
            if (IdentifiableAlreadyExist(shell.Id))
            {
                return;

            }
            var shellTwinId = await _writeShell.CreateShell(shell);

            if (shellTwinId == null)
            {
                throw new AASRepositoryException($"Shell with Id {shell.Id} could not be created");
            }

            if (shell.Submodels != null)
            {
                foreach (var submodelRef in shell.Submodels)
                {
                    await CreateSubmodelReferenceForTwinWithId(shellTwinId, submodelRef);
                }
            }
        }

        public async Task UpdateExistingAssetAdministrationShellWithId(string aasId, AssetAdministrationShell shell)
        {
            try
            {
                var twinId = _adtAasConnector.GetTwinIdForElementWithId(aasId);
                _logger.LogInformation($"Now updating AAS with Id {aasId}");
                await _updateShell.UpdateFullShell(twinId, shell);
            }
            catch (AdtException e)
            {
                _logger.LogError(e, e.Message);
                throw new AASRepositoryException($"No AAS with Id {aasId} found to update");
            }
        }


        public async Task CreateSubmodelReference(string aasId, Reference submodelRef)
        {
            if (IdentifiableAlreadyExist(aasId) == false)
            {
                throw new AASRepositoryException(
                    $"Can't create AAS to Submodel Reference because AAS with Id {aasId} does not exist");
            }

            if (IdentifiableAlreadyExist(submodelRef.Keys[0].Value) == false)
            {
                throw new AASRepositoryException(
                    $"Can't create AAS to Submodel Reference because Submodel with Id {submodelRef.Keys[0].Value} does not exist");
            }

            var aasTwinId = _adtAasConnector.GetTwinIdForElementWithId(aasId);
            await CreateSubmodelReferenceForTwinWithId(aasTwinId, submodelRef);
        }

        private async Task CreateSubmodelReferenceForTwinWithId(string aasTwinId, Reference submodelRef)
        {
            if (submodelRef.Keys[0].Type == KeyTypes.Submodel)
            {
                var submodelId = submodelRef.Keys[0].Value;

                try
                {
                    var submodelTwinId = _adtAasConnector.GetTwinIdForElementWithId(submodelId);
                    await _writeShell.CreateSubmodelReference(aasTwinId, submodelTwinId);
                }
                catch (AdtException e)
                {
                    _logger.LogError(e, e.Message);
                }
            }

        }

        public async Task DeleteSubmodelReference(string aasId, string submodelId)
        {
            if (IdentifiableAlreadyExist(aasId) == false)
            {
                throw new AASRepositoryException(
                    $"Can't delete AAS with Id {aasId} because it does not exist");
            }
            try
            {
                var submodelTwinId = _adtAasConnector.GetTwinIdForElementWithId(submodelId);
                var aasTwinId = _adtAasConnector.GetTwinIdForElementWithId(aasId);
                await _deleteShell.DeleteRelationship(aasTwinId, submodelTwinId, "submodel");
            }
            catch (AdtException e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public async Task<AssetAdministrationShell> GetAssetAdministrationShellWithId(string aasIdentifier)
        {
            if (IdentifiableAlreadyExist(aasIdentifier)==false)
            {
                throw new AASRepositoryException($"AssetAdministrationShell with id {aasIdentifier} does not exist");
            }

            var information = _adtAasConnector.GetAllInformationForAasWithId(aasIdentifier);
            information.rootElement = _adtAasConnector.GetAdtAasForAasWithId(aasIdentifier);

            return _modelFactory.GetAas(information);
        }

        public List<string> GetAllAssetAdministrationShellIds()
        {
            return _adtAasConnector.GetAllAasIds();
        }

        private bool IdentifiableAlreadyExist(string id)
        {
            try
            {
                _adtAasConnector.GetTwinIdForElementWithId(id);
                return true;
            }
            catch (AdtException e)
            {
                return false;
            }
        }

    }
}
