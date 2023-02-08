using System.Collections.Generic;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository
{
    public interface ISubmodelRepository
    {
        public Task<List<Submodel>> GetAllSubmodels();
        public Task<Submodel> GetSubmodelWithId(string submodelId);
        public Task CreateSubmodelElement(string submodelIdentifier, ISubmodelElement submodelElement);
        public Task CreateSubmodel(Submodel submodel);
        public Task DeleteSubmodelWithId(string submodelIdentifier);

    }


}
