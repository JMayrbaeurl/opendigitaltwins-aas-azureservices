using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelRepository : ISubmodelRepository
    {
        private readonly IAdtInteractions _adtInteractions;
        private readonly IAdtSubmodelInteractions _adtSubmodelInteractions;
        private readonly IAdtSubmodelModelFactory _adtSubmodelModelFactory;

        public AdtSubmodelRepository(IAdtSubmodelInteractions adtSubmodelInteractions, IAdtInteractions adtInteractions, IAdtSubmodelModelFactory adtSubmodelModelFactory)
        {
            _adtInteractions = adtInteractions;
            _adtSubmodelInteractions = adtSubmodelInteractions;
            _adtSubmodelModelFactory = adtSubmodelModelFactory ?? 
                                       throw new ArgumentNullException(nameof(adtSubmodelModelFactory));
        }
        public async Task<List<Submodel>> GetAllSubmodels()
        {
            var submodels = new List<Submodel>();
            var twinIds = await _adtInteractions.GetAllSubmodelTwinIds();
            foreach (var twinId in twinIds)
            {
                var information = await _adtSubmodelInteractions.GetAllInformationForSubmodelWithTwinId(twinId);
                submodels.Add(await _adtSubmodelModelFactory.GetSubmodel(information));
            }
            return submodels;
        }

        public async Task<Submodel> GetSubmodelWithId(string submodelId)
        {
            var twinId = _adtInteractions.GetTwinIdForElementWithId(submodelId);
            var information = await _adtSubmodelInteractions.GetAllInformationForSubmodelWithTwinId(twinId);
            return await _adtSubmodelModelFactory.GetSubmodel(information);
        }

    }
}
