using AasCore.Aas3_0_RC02;
using AutoMapper;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository.Adt
{
    public class ADTAASRepository : AASRepository
    {
        private readonly ADTAASModelFactory _modelFactory;
        private readonly IAdtAasConnector _adtAasConnector;
        private readonly ILogger _logger;

        public ADTAASRepository(DigitalTwinsClient client, IAdtAasConnector adtAasConnector, IMapper mapper,
            ILogger logger)
        {
            _modelFactory = new ADTAASModelFactory(mapper);
            _adtAasConnector = adtAasConnector;
            _logger = logger;
        }


        public async Task<List<AssetAdministrationShell>> GetAllAdministrationShells()
        {
            var ids = GetAllAasIds();
            var shells = new List<AssetAdministrationShell>();
            foreach (var id in ids)
            {
                var information = _adtAasConnector.GetAllInformationForAasWithId(id);
                information.RootElement = _adtAasConnector.GetAdtAasForAasWithId(id);
                
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
            var information = _adtAasConnector.GetAllInformationForAasWithId(aasIdentifier);
            information.RootElement = _adtAasConnector.GetAdtAasForAasWithId(aasIdentifier);

            

            return _modelFactory.GetAas(information);
        }

        public List<string> GetAllAasIds()
        {
            return _adtAasConnector.GetAllAasIds();
        }

        public AssetAdministrationShell GetAdministrationShellForAasId(string aasId)
        {
            throw new NotImplementedException();
        }

        

    }
}
