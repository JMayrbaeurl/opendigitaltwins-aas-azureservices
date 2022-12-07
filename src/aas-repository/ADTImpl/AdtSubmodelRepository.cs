using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAS_Services_Support.ADT_Support;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.ADTImpl
{
    public class AdtSubmodelRepository : ISubmodelRepository
    {
        private readonly AdtSubmodelModelFactory _modelFactory;

        public AdtSubmodelRepository(IAdtSubmodelInteractions adtSubmodelInteractions, IAdtInteractions adtInteractions)
        {
            _modelFactory = new AdtSubmodelModelFactory(adtSubmodelInteractions, adtInteractions);


        }
        public async Task<List<Submodel>> GetAllSubmodels()
        {
            await GetSubmodelWithId("id1");
            return new List<Submodel>();
        }

        public async Task<Submodel> GetSubmodelWithId(string submodelId)
        {
            return await _modelFactory.GetSubmodelFromTwinId(GetTwinIdFromSubmodelId(submodelId));
        }

        private string GetTwinIdFromSubmodelId(string submodelId)
        {
            return "Submodel_c56a1167-0c64-4425-a371-97adff92d92e";
        }
    }
}
