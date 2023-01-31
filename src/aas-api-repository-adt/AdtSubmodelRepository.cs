using AAS.ADT;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelRepository : ISubmodelRepository
    {
        private readonly IAdtAasConnector _adtAasConnector;
        private readonly IAdtSubmodelConnector _adtSubmodelConnector;
        private readonly IAdtSubmodelModelFactory _adtSubmodelModelFactory;
        private readonly IAasWriteSubmodel _writeSubmodel;

        public AdtSubmodelRepository(IAdtSubmodelConnector adtSubmodelConnector, IAdtAasConnector adtAasConnector,
            IAdtSubmodelModelFactory adtSubmodelModelFactory, IAasWriteSubmodel writeSubmodel)
        {
            _adtAasConnector = adtAasConnector;
            _adtSubmodelConnector = adtSubmodelConnector;
            _adtSubmodelModelFactory = adtSubmodelModelFactory ??
                                       throw new ArgumentNullException(nameof(adtSubmodelModelFactory));
            _writeSubmodel = writeSubmodel;
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
                submodels.Add(await _adtSubmodelModelFactory.GetSubmodel(information));
                submodelConnectorTasks.Remove(finishedTask);
            }




            return submodels;
        }

        public async Task<Submodel> GetSubmodelWithId(string submodelId)
        {
            var twinId = _adtAasConnector.GetTwinIdForElementWithId(submodelId);
            var information = await _adtSubmodelConnector.GetAllInformationForSubmodelWithTwinId(twinId);
            return await _adtSubmodelModelFactory.GetSubmodel(information);
        }

        public async Task CreateSubmodelElement(string submodelIdentifier, ISubmodelElement submodelElement)
        {
            var submodelTwinIdentifier = _adtAasConnector.GetTwinIdForElementWithId(submodelIdentifier);
            await _writeSubmodel.CreateSubmodelElementForSubmodel(submodelElement, submodelTwinIdentifier);
        }

        public async Task CreateSubmodel(Submodel submodel)
        {
            if (IdentifiableAlreadyExist(submodel.Id) == false)
            {
                await _writeSubmodel.CreateSubmodel(submodel);
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
