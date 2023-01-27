using System.Collections.Generic;
using Azure.DigitalTwins.Core;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using Microsoft.Extensions.Logging;

namespace AAS.ADT
{
    public class AasWriteAssetAdministrationShell : IAasWriteAssetAdministrationShell
    {
        private readonly ILogger<AasWriteAssetAdministrationShell> _logger;
        private readonly IAdtTwinFactory _modelFactory;
        private readonly IAasWriteConnector _aasWriteConnector;
        private readonly IAasWriteBase _writeBase;

        public AasWriteAssetAdministrationShell(ILogger<AasWriteAssetAdministrationShell> logger, IAdtTwinFactory modelFactory, 
            IAasWriteConnector aasWriteConnector, IAasWriteBase writeBase)
        {
            _logger = logger;
            _modelFactory = modelFactory;
            _aasWriteConnector = aasWriteConnector;
            _writeBase = writeBase;
        }

        public async Task CreateShell(AssetAdministrationShell shell)
        {
            if (shell == null)
            {
                return;
            }

            _logger.LogInformation($"Now importing Administration shell '{shell.IdShort}' into ADT instance");

            var twin = _modelFactory.GetTwin(shell);
            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(twin);

            await _writeBase.AddHasDataSpecification(twin.Id, shell.EmbeddedDataSpecifications);

            await CreateAssetInformation(shell, twin.Id);
        }

        public async Task CreateSubmodelReference(string shellTwinId, string submodelTwinId)
        {
            await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(shellTwinId, "submodel", submodelTwinId);
        }

        private async Task CreateAssetInformation(AssetAdministrationShell shell, string shellTwinId)
        {
            var assetInfoTwinData = _modelFactory.GetTwin(shell.AssetInformation);
            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(assetInfoTwinData);
            await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(shellTwinId, "assetInformation",
                assetInfoTwinData.Id);
        }

    }
}
