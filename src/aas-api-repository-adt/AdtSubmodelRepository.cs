using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelRepository : ISubmodelRepository
    {
        private readonly IAdtAasConnector _adtAasConnector;
        private readonly IAdtSubmodelConnector _adtSubmodelConnector;
        private readonly IAdtSubmodelModelFactory _adtSubmodelModelFactory;

        public AdtSubmodelRepository(IAdtSubmodelConnector adtSubmodelConnector, IAdtAasConnector adtAasConnector, IAdtSubmodelModelFactory adtSubmodelModelFactory)
        {
            _adtAasConnector = adtAasConnector;
            _adtSubmodelConnector = adtSubmodelConnector;
            _adtSubmodelModelFactory = adtSubmodelModelFactory ?? 
                                       throw new ArgumentNullException(nameof(adtSubmodelModelFactory));
        }
        public async Task<List<Submodel>> GetAllSubmodels()
        {
            var submodels = new List<Submodel>();
            var twinIds = await _adtAasConnector.GetAllSubmodelTwinIds();
            foreach (var twinId in twinIds)
            {
                var information = await _adtSubmodelConnector.GetAllInformationForSubmodelWithTwinId(twinId);
                submodels.Add(await _adtSubmodelModelFactory.GetSubmodel(information));
            }
            return submodels;
        }

        public async Task<Submodel> GetSubmodelWithId(string submodelId)
        {
            var twinId = _adtAasConnector.GetTwinIdForElementWithId(submodelId);
            var information = await _adtSubmodelConnector.GetAllInformationForSubmodelWithTwinId(twinId);
            return await _adtSubmodelModelFactory.GetSubmodel(information);
        }

    }
}
