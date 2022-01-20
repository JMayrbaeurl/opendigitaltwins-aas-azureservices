using AAS.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.API.Interfaces
{
    public interface Registry
    {
        public Task<List<AssetAdministrationShellDescriptor>> GetAllAssetAdministrationShellDescriptors();

        public Task<AssetAdministrationShellDescriptor> GetAssetAdministrationShellDescriptorById(string aasIdentifier);

        public Task<AssetAdministrationShellDescriptor> CreateAssetAdministrationShellDescriptor(AssetAdministrationShellDescriptor aasDesc);

        public Task UpdateAssetAdministrationShellDescriptorById(AssetAdministrationShellDescriptor aasDesc, string aasIdentifier);

        public Task DeleteAssetAdministrationShellDescriptorById(string aasIdentifier);
    }
}
