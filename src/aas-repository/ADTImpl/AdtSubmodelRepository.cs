using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAS_Services_Support.ADT_Support;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.ADTImpl
{
    public class AdtSubmodelRepository : ISubmodelRepository
    {
        private readonly IAdtInteractions _adtInteractions;
        private readonly IAdtSubmodelInteractions _adtSubmodelInteractions;

        public AdtSubmodelRepository(IAdtSubmodelInteractions adtSubmodelInteractions, IAdtInteractions adtInteractions)
        {
            _adtInteractions = adtInteractions;
            _adtSubmodelInteractions = adtSubmodelInteractions;
        }
        public async Task<List<Submodel>> GetAllSubmodels()
        {
            var submodels = new List<Submodel>();
            var twinIds = await _adtInteractions.GetAllSubmodelTwinIds();
            foreach (var twinId in twinIds)
            {
                var information = await _adtSubmodelInteractions.GetAllInformationForSubmodelWithTwinId(twinId);
                var modelFactory = new AdtSubmodelModelFactory(information);
                submodels.Add(await modelFactory.GetSubmodel());
            }
            return submodels;
        }

        public async Task<Submodel> GetSubmodelWithId(string submodelId)
        {
            var twinId = _adtInteractions.GetTwinIdForElementWithId(submodelId);
            var information = await _adtSubmodelInteractions.GetAllInformationForSubmodelWithTwinId(twinId);
            var modelFactory = new AdtSubmodelModelFactory(information);
            return await modelFactory.GetSubmodel();
        }

    }
}
