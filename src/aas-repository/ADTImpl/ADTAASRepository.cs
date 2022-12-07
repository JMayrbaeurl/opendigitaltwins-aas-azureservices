using System;
using AAS.API.Services.ADT;
using Azure;
using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AdtModels.AdtModels;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Net.Sockets;
using AAS_Services_Support.ADT_Support;
using AasCore.Aas3_0_RC02;


namespace AAS.API.Repository
{
    public class ADTAASRepository : AASRepository
    {
        private readonly ADTAASModelFactory _modelFactory;
        private readonly IAdtInteractions _adtInteractions;

        public ADTAASRepository(DigitalTwinsClient client, IAdtInteractions adtInteractions) //: base(client)
        {
            _modelFactory = new ADTAASModelFactory(adtInteractions);
            _adtInteractions = adtInteractions;
        }


        public async Task<List<AssetAdministrationShell>> GetAllAdministrationShells()
        {
            var ids = GetAllAasIds();
            var shells = new List<AssetAdministrationShell>();
            foreach (var id in ids)
            {
                shells.Add(_modelFactory.GetAasWithId(id));
            }
            return shells;
        }

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByAssetId(List<SpecificAssetId> assetIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByIdShort(string withIdShort)
        {
            throw new NotImplementedException();
        }

        public async Task<AssetAdministrationShell> GetAssetAdministrationShellWithId(string aasIdentifier)
        {
            return _modelFactory.GetAasWithId(aasIdentifier);
        }

        public List<string> GetAllAasIds()
        {
            return _adtInteractions.GetAllAasIds();
        }

        public AssetAdministrationShell GetAdministrationShellForAasId(string aasId)
        {
            throw new NotImplementedException();
        }
    }

    

}
