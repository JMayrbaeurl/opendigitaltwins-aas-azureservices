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
            if (reference == null || reference.Keys.Count == 0)
            {
                return;
            }

            var refTwinData = _modelFactory.GetTwin(reference);
            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(refTwinData);

            await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(sourceTwinId, relationshipName, refTwinData.Id);
        }

        public async Task AddHasDataSpecification(string sourceTwinId,
            List<EmbeddedDataSpecification> embeddedDataSpecifications)
        {
            if (embeddedDataSpecifications == null)
            {
                return;
            }

            foreach (var dataSpecification in embeddedDataSpecifications)
            {
                if (dataSpecification.DataSpecificationContent is DataSpecificationIec61360 contentIec61360)
                {
                    var dsTwinData = _modelFactory.GetTwin(contentIec61360);

                    await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(dsTwinData);

                    await AddReference(dsTwinData.Id, contentIec61360.UnitId, "unitId");

                    await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(sourceTwinId, "dataSpecification",
                        dsTwinData.Id);
                }
                else
                {
                    throw new ArgumentException(
                        $"DataSpecificationContent of Type {dataSpecification.DataSpecificationContent.GetType()} is not supported ");
                }
            }
        }

        public async Task AddQualifiableRelations(string sourceTwinId, List<Qualifier> qualifiers)
        {
            // TODO: V3.0 has Formulas beside Qualifiers

            _logger.LogInformation($"Adding qualifiers for twin '{sourceTwinId}'");

            if (qualifiers == null)
            {
                return;
            }

            foreach (var qualifier in qualifiers)
            {
                _logger.LogInformation($"Now creating qualifier '{qualifier.ValueId}'");

                var constraintTwinData = _modelFactory.GetTwin(qualifier);
                await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(constraintTwinData);


                await AddReference(constraintTwinData.Id, qualifier.ValueId, "valueId");

                await AddReference(constraintTwinData.Id, qualifier.SemanticId, "semanticId");

                await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(sourceTwinId, "qualifier",
                    constraintTwinData.Id);
            }
        }
    }
}
