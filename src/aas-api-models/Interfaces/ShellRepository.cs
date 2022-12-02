using AAS.API.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Interfaces
{
    public interface  ShellRepository
    {
        public Task<List<AssetAdministrationShell>> GetAllAdministrationShells();

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByAssetId(List<IdentifierKeyValuePair> assetIds);

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByIdShort(string withIdShort);
    }
}
