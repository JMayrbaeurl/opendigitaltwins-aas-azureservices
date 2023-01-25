using System.Collections.Generic;
using System.Linq;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT
{
    public class AasWriteSubmodel : IAasWriteSubmodel
    {
        private readonly ILogger<AasWriteSubmodel> _logger;
        private readonly IAdtTwinFactory _modelFactory;
        private readonly IAasWriteConnector _aasWriteConnector;
        private readonly IAasWriteBase _writeBase;
        private readonly IAasWriteSubmodelElements _writeSubmodelElements;

        public AasWriteSubmodel(ILogger<AasWriteSubmodel> logger, IAdtTwinFactory modelFactory,
            IAasWriteSubmodelElements writeSubmodelElements, IAasWriteConnector aasWriteConnector, IAasWriteBase writeBase)
        {
            _writeBase = writeBase;
            _aasWriteConnector = aasWriteConnector;
            _modelFactory = modelFactory;
            _logger = logger;
            _writeSubmodelElements = writeSubmodelElements;
        }


        public async Task CreateSubmodel(Submodel submodel)
        {
            //_logger.LogInformation($"Now importing Submodel '{submodel.IdShort}' for shell '{shell.IdShort}' into ADT instance");

            if (submodel == null)
            {
                return;
            }
            
            var subModelTwinData = _modelFactory.GetTwin(submodel);
            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(subModelTwinData);

            await _writeBase.AddQualifiableRelations(subModelTwinData.Id, submodel.Qualifiers);

            await _writeBase.AddReference(subModelTwinData.Id, submodel.SemanticId, "semanticId");

            await _writeBase.AddHasDataSpecification(subModelTwinData.Id, submodel.EmbeddedDataSpecifications);

            await CreateSubmodelElementsForSubmodel(submodel.SubmodelElements, subModelTwinData.Id);

            // Create relationship between Shell and Submodel
            // await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(shellTwin.Id, "submodel", subModelTwinData.Id);
        }

        private async Task CreateSubmodelElementsForSubmodel(List<ISubmodelElement> submodelElements, string submodelTwinId)
        {
            if (submodelElements == null)
            {
                return;
            }

            foreach (var submodelElement in submodelElements)
            {
                await CreateSubmodelElementForSubmodel(submodelElement, submodelTwinId);
            }
        }

        public async Task CreateSubmodelElementForSubmodel(ISubmodelElement submodelElement, string submodelTwinId)
        {
            string submodelElementDtId = await _writeSubmodelElements.CreateSubmodelElement(submodelElement);
            await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(submodelTwinId, "submodelElement",
                    submodelElementDtId);
        }
    }
}
