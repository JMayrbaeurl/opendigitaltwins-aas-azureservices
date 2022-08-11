using AAS.ADT.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAS.ADT
{
    public interface IAASADTRepository
    {
        public Task<AssetAdministrationShell> ReadAASWithDtIdAsync(string dtId);

        public Task<AssetInformation> ReadAssetInformationForAAS(string aasDtId);
    }
}
