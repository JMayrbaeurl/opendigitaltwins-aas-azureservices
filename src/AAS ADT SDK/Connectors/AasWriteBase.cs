using System;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AAS.ADT
{
    public class AasWriteBase : IAasWriteBase
    {
        private readonly ILogger<AasWriteBase> _logger;
        private readonly IAdtTwinFactory _modelFactory;
        private readonly IAasWriteConnector _aasWriteConnector;

        public AasWriteBase(ILogger<AasWriteBase> logger, IAdtTwinFactory modelFactory, IAasWriteConnector aasWriteConnector)
        {
            _logger = logger;
            _modelFactory = modelFactory;
            _aasWriteConnector = aasWriteConnector;
        }

        public async Task AddReference(string sourceTwinId, Reference reference, string relationshipName)
        {
            if (reference != null && reference.Keys.Count > 0)
                await CreateReferenceTwin(sourceTwinId, reference, relationshipName);
        }

        private async Task<BasicDigitalTwin> CreateReferenceTwin(string sourceTwinId, Reference reference,
            string relationshipName)
        {
            var refTwinData = _modelFactory.GetTwin(reference);
            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(refTwinData);
            
            await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(sourceTwinId, relationshipName, refTwinData.Id);

            return refTwinData;
        }

        public async Task AddHasDataSpecification(string sourceTwinId,
            List<EmbeddedDataSpecification> embeddedDataSpecifications)
        {
            if (embeddedDataSpecifications != null)
            {
                foreach (var dataSpecification in embeddedDataSpecifications)
                {
                    if (dataSpecification.DataSpecificationContent is DataSpecificationIec61360)
                    {
                        DataSpecificationIec61360 content = (DataSpecificationIec61360)dataSpecification.DataSpecificationContent;

                        var dsTwinData = _modelFactory.GetTwin(content);

                        await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(dsTwinData);

                        // Create related unitId
                        if (content.UnitId != null)
                            await AddReference(dsTwinData.Id, content.UnitId, "unitId");
                        
                        await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(sourceTwinId, "dataSpecification", dsTwinData.Id);
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"DataSpecificationContent of Type {dataSpecification.DataSpecificationContent.GetType()} is not supported ");
                    }
                }
            }
        }

        public async Task AddQualifiableRelations(string sourceTwinId, List<Qualifier> qualifiers)
        {
            // TODO: V3.0 has Formulas beside Qualifiers

            _logger.LogInformation($"Adding qualifiers for twin '{sourceTwinId}'");

            if (qualifiers != null)
            {
                foreach (var qualifier in qualifiers)
                {
                    _logger.LogInformation($"Now creating qualifier '{qualifier.ValueId}'");

                    var constraintTwinData = _modelFactory.GetTwin(qualifier);
                    await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(constraintTwinData);

                    if (qualifier.ValueId != null)
                        await AddReference(constraintTwinData.Id, qualifier.ValueId, "valueId");

                    // Create semantic Id
                    if (qualifier.SemanticId != null)
                    {
                        await AddReference(constraintTwinData.Id, qualifier.SemanticId, "semanticId");
                    }

                    await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(sourceTwinId, "qualifier", constraintTwinData.Id);
                }
            }
        }
    }
}
