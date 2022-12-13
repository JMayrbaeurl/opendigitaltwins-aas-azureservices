using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAS_Services_Support.ADT_Support;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.ADTImpl
{
    public class AdtSubmodelRepository : ISubmodelRepository
    {
        private readonly AdtSubmodelModelFactory _modelFactory;
        private readonly IAdtInteractions _adtInteractions;

        public AdtSubmodelRepository(IAdtSubmodelInteractions adtSubmodelInteractions, IAdtInteractions adtInteractions)
        {
            _modelFactory = new AdtSubmodelModelFactory(adtSubmodelInteractions, adtInteractions);
            _adtInteractions = adtInteractions;
        }
        public async Task<List<Submodel>> GetAllSubmodels()
        {
            var submodels = new List<Submodel>();
            var twinIds = await _adtInteractions.GetAllSubmodelTwinIds();
            foreach (var twinId in twinIds)
            {
                submodels.Add(await _modelFactory.GetSubmodelFromTwinId(twinId));
            }
            return submodels;
        }

        public async Task<Submodel> GetSubmodelWithId(string submodelId)
        {
            var twinId = _adtInteractions.GetTwinIdForElementWithId(submodelId);
            return await _modelFactory.GetSubmodelFromTwinId(twinId);
        }

    }
}
