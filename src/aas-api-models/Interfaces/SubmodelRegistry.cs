using AAS.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.API.Interfaces
{
    public interface SubmodelRegistry
    {
        public Task<List<SubmodelDescriptor>> GetAllSubmodelDescriptors(int maxItems = 100);

        public Task<SubmodelDescriptor> GetSubmodelDescriptorById(string submodelIdentifier);

        public Task<SubmodelDescriptor> CreateSubmodelDescriptor(SubmodelDescriptor submodelDescriptor);

        public Task<SubmodelDescriptor> UpdateSubmodelDescriptorById(SubmodelDescriptor submodelDescriptor);

        public Task<bool> DeleteSubmodelDescriptorById(string idsubmodelIdentifier);
    }
}
