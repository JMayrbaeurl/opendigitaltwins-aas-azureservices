using AAS.ADT;
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
        private readonly IAasWriteAssetAdministrationShell _writeShell;

        public ADTAASRepository(DigitalTwinsClient client, IAdtAasConnector adtAasConnector, IMapper mapper,
            ILogger logger, IAasWriteAssetAdministrationShell writeShell)
        {
            _modelFactory = new ADTAASModelFactory(mapper);
            _adtAasConnector = adtAasConnector;
            _logger = logger;
            _writeShell = writeShell;
        }


        public async Task<List<AssetAdministrationShell>> GetAllAssetAdministrationShells()
        {
            var ids = GetAllAssetAdministrationShellIds();
            var shells = new List<AssetAdministrationShell>();
            foreach (var id in ids)
            {
                var information = _adtAasConnector.GetAllInformationForAasWithId(id);
                information.RootElement = _adtAasConnector.GetAdtAasForAasWithId(id);

                shells.Add(_modelFactory.GetAas(information));
            }
            return shells;
        }

        public async Task CreateAssetAdministrationShell(AssetAdministrationShell shell)
        {


            if (IdentifiableAlreadyExist(shell.Id) == false)
            {
                await _writeShell.CreateShell(shell);
            }

            if (shell.Submodels!= null)
            {
                foreach (var submodelRef in shell.Submodels)
                {
                    await CreateSubmodelReference(shell.Id, submodelRef);
                }
            }
        }

        public async Task CreateSubmodelReference(string aasId, Reference submodelRef)
        {
            var shellTwinId = "";
            try
            {
                shellTwinId = _adtAasConnector.GetTwinIdForElementWithId(aasId);
            }
            catch (AdtException e)
            {
                return;
            }
            if (submodelRef.Keys[0].Type == KeyTypes.Submodel)
            {
                var submodelId = submodelRef.Keys[0].Value;

                try
                {
                    var submodelTwinId = _adtAasConnector.GetTwinIdForElementWithId(submodelId);
                    await _writeShell.CreateSubmodelReference(shellTwinId,submodelTwinId);
                }
                catch (AdtException e)
                {
                }
            }
        }

        public async Task<AssetAdministrationShell> GetAssetAdministrationShellWithId(string aasIdentifier)
        {
            var information = _adtAasConnector.GetAllInformationForAasWithId(aasIdentifier);
            information.RootElement = _adtAasConnector.GetAdtAasForAasWithId(aasIdentifier);



            return _modelFactory.GetAas(information);
        }

        public List<string> GetAllAssetAdministrationShellIds()
        {
            return _adtAasConnector.GetAllAasIds();
        }

        private bool IdentifiableAlreadyExist(string id)
        {
            try
            {
                _adtAasConnector.GetTwinIdForElementWithId(id);
                return true;
            }
            catch (AdtException e)
            {
                return false;
            }
        }

    }
}
