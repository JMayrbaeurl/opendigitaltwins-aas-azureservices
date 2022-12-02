using System;
using AAS.API.Models;
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


namespace AAS.API.Repository
{
    public class ADTAASRepository : AASRepository
    {
        private readonly ADTAASModelFactory _modelFactory;
        private readonly AdtInteractions _adtInteractions;

        public ADTAASRepository(DigitalTwinsClient client) //: base(client)
        {
            _modelFactory = new ADTAASModelFactory(client);
            _adtInteractions = new AdtInteractions(client);
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

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByAssetId(List<IdentifierKeyValuePair> assetIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShellsByIdShort(string withIdShort)
        {
            throw new NotImplementedException();
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
