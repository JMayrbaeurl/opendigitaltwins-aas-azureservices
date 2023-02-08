using AAS.ADT;
using AAS.ADT.Models;
using AAS.API.Interfaces;
using AasCore.Aas3_0_RC02;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelRepository : ISubmodelRepository
    {
        private readonly IAdtAasConnector _adtAasConnector;
        private readonly IAdtSubmodelConnector _adtSubmodelConnector;
        private readonly IAdtSubmodelModelFactory _adtSubmodelModelFactory;
        private readonly IAasWriteSubmodel _writeSubmodel;
        private readonly IAasDeleteAdt _deleteSubmodel;
        private readonly ILogger<AdtSubmodelRepository> _logger;



        public AdtSubmodelRepository(IAdtSubmodelConnector adtSubmodelConnector, IAdtAasConnector adtAasConnector,
            IAdtSubmodelModelFactory adtSubmodelModelFactory, IAasWriteSubmodel writeSubmodel, IAasDeleteAdt deleteSubmodel, ILogger<AdtSubmodelRepository> logger)
        {
            _adtAasConnector = adtAasConnector;
            _adtSubmodelConnector = adtSubmodelConnector;
            _adtSubmodelModelFactory = adtSubmodelModelFactory ??
                                       throw new ArgumentNullException(nameof(adtSubmodelModelFactory));
            _writeSubmodel = writeSubmodel;
            _deleteSubmodel = deleteSubmodel;
            _logger = logger;
        }
        public async Task<List<Submodel>> GetAllSubmodels()
        {
            var submodels = new List<Submodel>();
            var twinIds = await _adtAasConnector.GetAllSubmodelTwinIds();
            var submodelConnectorTasks = new List<Task<AdtSubmodelAndSmcInformation<AdtSubmodel>>>();
            foreach (var twinId in twinIds)
            {
                submodelConnectorTasks.Add(_adtSubmodelConnector.GetAllInformationForSubmodelWithTwinId(twinId));
            }

            while (submodelConnectorTasks.Count > 0)
            {
                Task<AdtSubmodelAndSmcInformation<AdtSubmodel>> finishedTask = await Task.WhenAny(submodelConnectorTasks);
                AdtSubmodelAndSmcInformation<AdtSubmodel> information = await finishedTask;
                submodels.Add(_adtSubmodelModelFactory.GetSubmodel(information));
                submodelConnectorTasks.Remove(finishedTask);
            }

            return submodels;
        }

        public async Task<Submodel?> GetSubmodelWithId(string submodelId)
        {
            string twinId;
            try
            {
                twinId = _adtAasConnector.GetTwinIdForElementWithId(submodelId);
            }
            catch (Exception e)
            {
                return null;
            }

            var information = await _adtSubmodelConnector.GetAllInformationForSubmodelWithTwinId(twinId);
            return _adtSubmodelModelFactory.GetSubmodel(information);
        }

        public async Task CreateSubmodelElement(string submodelIdentifier, ISubmodelElement submodelElement)
        {
            string submodelTwinIdentifier;
            try
            {
                submodelTwinIdentifier = _adtAasConnector.GetTwinIdForElementWithId(submodelIdentifier);
            }
            catch (AdtException e)
            {
                throw new ArgumentException(
                    $"Can't create SubmodelElement because Submodel with id '{submodelIdentifier} does not exist'");
            }

            await _writeSubmodel.CreateSubmodelElementForSubmodel(submodelElement, submodelTwinIdentifier);
        }

        public async Task CreateSubmodel(Submodel submodel)
        {
            if (IdentifiableAlreadyExist(submodel.Id) == false)
            {
                await _writeSubmodel.CreateSubmodel(submodel);
            }
        }

        public async Task DeleteSubmodelWithId(string submodelIdentifier)
        {
            try
            {
                var twinId = _adtAasConnector.GetTwinIdForElementWithId(submodelIdentifier);
                await _deleteSubmodel.DeleteTwin(twinId);
            }
            catch (AdtException e)
            {
                _logger.LogError(e, e.Message);
                throw new AASRepositoryException($"No Submodel with Id {submodelIdentifier} found to delete");
            }
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
