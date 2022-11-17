using AAS.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.API.Interfaces
{
    public interface Registry
    {
        public Task<List<AssetAdministrationShellDescriptor>> GetAllAssetAdministrationShellDescriptors(int maxItems = 100);

        public Task<AssetAdministrationShellDescriptor> GetAssetAdministrationShellDescriptorById(string aasIdentifier);

        public Task<AssetAdministrationShellDescriptor> CreateAssetAdministrationShellDescriptor(AssetAdministrationShellDescriptor aasDesc);

        public Task<AssetAdministrationShellDescriptor> UpdateAssetAdministrationShellDescriptorById(AssetAdministrationShellDescriptor aasDesc);

        public Task<bool> DeleteAssetAdministrationShellDescriptorById(string aasIdentifier);
    }
}
