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

            var submodelTwinId = await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(submodelTwinData);

            var tasks = new List<Task>
            {
                _writeBase.AddQualifiableRelations(submodelTwinId, submodel.Qualifiers),
                _writeBase.AddReference(submodelTwinId, submodel.SemanticId, "semanticId"),
                _writeBase.AddHasDataSpecification(submodelTwinId, submodel.EmbeddedDataSpecifications),
                CreateSubmodelElementsForSubmodel(submodelTwinId,submodel.SubmodelElements)
            };

            await Task.WhenAll(tasks);

            return submodelTwinData.Id;
        }

        private async Task CreateSubmodelElementsForSubmodel(string submodelTwinId,List<ISubmodelElement> submodelElements )
        {
            if (submodelElements == null)
            {
                return;
            }

            var tasks = new List<Task>();
            foreach (var submodelElement in submodelElements)
            {
                tasks.Add(CreateSubmodelElementForSubmodel(submodelElement, submodelTwinId));
            }
            await Task.WhenAll(tasks);
        }

        public async Task CreateSubmodelElementForSubmodel(ISubmodelElement submodelElement, string submodelTwinId)
        {
            string submodelElementDtId = await _writeSubmodelElements.CreateSubmodelElement(submodelElement);
            await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(submodelTwinId, "submodelElement",
                    submodelElementDtId);
        }
    }
}
