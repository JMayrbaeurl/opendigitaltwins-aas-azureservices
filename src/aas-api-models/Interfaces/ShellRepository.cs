using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;


namespace AAS.API.Interfaces
{
    public interface  ShellRepository
    {
        public Task<List<AssetAdministrationShell>> GetAllAdministrationShells();

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByAssetId(List<SpecificAssetId> assetIds);

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByIdShort(string withIdShort);

        public Task<AssetAdministrationShell> GetAssetAdministrationShellWithId(string aasIdentifier);
    }
}
