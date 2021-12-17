using AAS.API.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Interfaces
{
    public interface BasicDiscovery
    {
        public Task<List<string>> GetAllAssetAdministrationShellIdsByAssetLink(List<IdentifierKeyValuePair> assetIds);

        public Task<List<IdentifierKeyValuePair>> GetAllAssetLinksById(string aasIdentifier);

        public Task<List<IdentifierKeyValuePair>> CreateAllAssetLinksById(string aasIdentifier, List<IdentifierKeyValuePair> assetIds);

        public Task<List<IdentifierKeyValuePair>> DeleteAllAssetLinksById(string aasIdentifier, List<IdentifierKeyValuePair> assetIds);
    }
}
