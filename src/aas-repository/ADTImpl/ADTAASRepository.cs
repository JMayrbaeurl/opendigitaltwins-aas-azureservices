using System;
using AAS.API.Services.ADT;
using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAS_Services_Support.ADT_Support;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Microsoft.Extensions.Logging;


namespace AAS.API.Repository
{
    public class ADTAASRepository : AASRepository
    {
        private readonly ADTAASModelFactory _modelFactory;
        private readonly IAdtInteractions _adtInteractions;
        private readonly ILogger _logger;

        public ADTAASRepository(DigitalTwinsClient client, IAdtInteractions adtInteractions, IMapper mapper,
            ILogger logger)
        {
            _modelFactory = new ADTAASModelFactory(mapper);
            _adtInteractions = adtInteractions;
            _logger = logger;
        }


        public async Task<List<AssetAdministrationShell>> GetAllAdministrationShells()
        {
            var ids = GetAllAasIds();
            var shells = new List<AssetAdministrationShell>();
            foreach (var id in ids)
            {
                var information = _adtInteractions.GetAllInformationForAasWithId(id);
                information.RootElement = _adtInteractions.GetAdtAasForAasWithId(id);
                
                shells.Add(_modelFactory.GetAas(information));
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
            var information = _adtInteractions.GetAllInformationForAasWithId(aasIdentifier);
            information.RootElement = _adtInteractions.GetAdtAasForAasWithId(aasIdentifier);

            

            return _modelFactory.GetAas(information);
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
