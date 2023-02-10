using System.Collections.Generic;
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
        
        public async Task<string> CreateSubmodel(Submodel submodel)
        {
            if (submodel == null)
            {
                return null;
            }
            
            var submodelTwinData = _modelFactory.GetTwin(submodel);
            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(submodelTwinData);

            await _writeBase.AddQualifiableRelations(submodelTwinData.Id, submodel.Qualifiers);

            await _writeBase.AddReference(submodelTwinData.Id, submodel.SemanticId, "semanticId");

            await _writeBase.AddHasDataSpecification(submodelTwinData.Id, submodel.EmbeddedDataSpecifications);

            await CreateSubmodelElementsForSubmodel(submodel.SubmodelElements, submodelTwinData.Id);

            return submodelTwinData.Id;
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
