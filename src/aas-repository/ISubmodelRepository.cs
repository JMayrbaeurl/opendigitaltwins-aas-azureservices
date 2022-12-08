using System.Collections.Generic;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository
{
    public interface ISubmodelRepository
    {
        public Task<List<Submodel>> GetAllSubmodels();
        public Task<Submodel> GetSubmodelWithId(string submodelId);
    }

    
}
