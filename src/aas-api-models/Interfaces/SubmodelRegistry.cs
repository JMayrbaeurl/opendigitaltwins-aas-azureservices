using AAS.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.API.Interfaces
{
    public interface SubmodelRegistry
    {
        public Task<List<SubmodelDescriptor>> GetAllSubmodelDescriptors();

        public Task<SubmodelDescriptor> GetSubmodelDescriptorById(string submodelIdentifier);

        public Task<SubmodelDescriptor> CreateSubmodelDescriptor(SubmodelDescriptor submodelDescriptor);

        public Task UpdateSubmodelDescriptorById(string submodelIdentifier, SubmodelDescriptor submodelDescriptor);

        public Task DeleteSubmodelDescriptorById(string idsubmodelIdentifier);
    }
}
