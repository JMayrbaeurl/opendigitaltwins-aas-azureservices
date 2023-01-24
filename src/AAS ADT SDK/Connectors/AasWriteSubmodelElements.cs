using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAS.ADT
{
    public class AasWriteSubmodelElements : IAasWriteSubmodelElements
    {
        private readonly ILogger<AasWriteSubmodelElements> _logger;
        private readonly IAdtTwinFactory _modelFactory;
        private readonly IAasWriteConnector _aasWriteConnector;
        private readonly IAasWriteBase _writeBase;

        public AasWriteSubmodelElements(ILogger<AasWriteSubmodelElements> logger, IAdtTwinFactory modelFactory,
            IAasWriteConnector aasWriteConnector, IAasWriteBase writeBase)
        {
            _logger = logger;
            _modelFactory = modelFactory;
            _aasWriteConnector = aasWriteConnector;
            _writeBase = writeBase;
        }


        public async Task<string> CreateSubmodelElement(ISubmodelElement submodelElement)
        {
            if (submodelElement is SubmodelElementCollection)
            {
                return await CreateSubmodelElementCollection((SubmodelElementCollection)submodelElement);
            }
            else if (submodelElement is Property)
            {
                return await CreateProperty((Property)submodelElement);
            }
            else if (submodelElement is File)
            {
                return await CreateFile((File)submodelElement);
            }

            //else if (submodelElement.submodelElement.GetType() == typeof(MultiLanguageProperty))
            //{
            //    return await CreateMultiLanguageProperty(submodelElement.GetAs<MultiLanguageProperty>());
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(AdminShellV20.Range))
            //{
            //    return await CreateRange(submodelElement.GetAs<AdminShellV20.Range>(), processInfo);
            //}

            //else if (submodelElement.submodelElement.GetType() == typeof(Blob))
            //{
            //    return await CreateBlob(submodelElement.GetAs<Blob>(), processInfo);
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(ReferenceElement))
            //{
            //    return await CreateReferenceElement(submodelElement.GetAs<ReferenceElement>(), processInfo);
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(Capability))
            //{
            //    return await CreateCapability(submodelElement.GetAs<Capability>(), processInfo);
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(RelationshipElement))
            //{
            //    return await CreateRelationshipElement(submodelElement.GetAs<RelationshipElement>(), processInfo);
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(AnnotatedRelationshipElement))
            //{
            //    return await CreateAnnotatedRelationshipElement(submodelElement.GetAs<AnnotatedRelationshipElement>(), processInfo);
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(AdminShellV20.Operation))
            //{
            //    return await CreateOperation(submodelElement.GetAs<AdminShellV20.Operation>(), processInfo);
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(BasicEvent))
            //{
            //    return await CreateBasicEvent(submodelElement.GetAs<BasicEvent>(), processInfo);
            //}
            //else if (submodelElement.submodelElement.GetType() == typeof(Entity))
            //{
            //    return await CreateEntity(submodelElement.GetAs<Entity>(), processInfo);
            //}

            _logger.LogError(
                $"CreateSubmodelElement called for unsupported SubmodelElement '{submodelElement.GetType()}'");

            return null;
        }

        private async Task<string> CreateSubmodelElementCollection(SubmodelElementCollection submodelElementCollection)
        {
            _logger.LogInformation($"Now creating submodel element collection '{submodelElementCollection.IdShort}'");

            // Start by creating a twin for the Submodel Element collection
            var twin = _modelFactory.GetTwin(submodelElementCollection);

            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(twin);

            await AddSubmodelElementRelationships(twin.Id, submodelElementCollection);

            // Add all submodel elements
            if (submodelElementCollection.Value != null)
            {
                foreach (var submodelElement in submodelElementCollection.Value)
                {
                    string submodelElementDtId = await CreateSubmodelElement(submodelElement);
                    if (submodelElementDtId != null)
                    {
                        await _aasWriteConnector.DoCreateOrReplaceRelationshipAsync(twin.Id, "value", submodelElementDtId);
                    }
                }
            }

            return twin.Id;
        }

        private async Task<string> CreateProperty(Property property)
        {
            _logger.LogInformation($"Now creating property '{property.IdShort}'");

            // Start by creating a twin for the Property
            var twin = _modelFactory.GetTwin(property);
            
            if (property.ValueId != null)
                await _writeBase.AddReference(twin.Id, property.ValueId, "valueId");
            
            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(twin);
            
            await AddSubmodelElementRelationships(twin.Id, property);

            return twin.Id;
        }

        private async Task<string> CreateFile(File file)
        {
            _logger.LogInformation($"Now creating file '{file.IdShort}'");

            // Start by creating a twin for the Asset
            var twin = _modelFactory.GetTwin(file);

            await _aasWriteConnector.DoCreateOrReplaceDigitalTwinAsync(twin);

            await AddSubmodelElementRelationships(twin.Id, file);

            return twin.Id;
        }


        private async Task AddSubmodelElementRelationships(string sourceTwinId, ISubmodelElement submodelElement)
        {
            // Create semantic Id
            if (submodelElement.SemanticId != null)
            {
                await _writeBase.AddReference(sourceTwinId, submodelElement.SemanticId, "semanticId");
            }

            if (submodelElement.EmbeddedDataSpecifications != null)
                await _writeBase.AddHasDataSpecification(sourceTwinId, submodelElement);

            if (submodelElement.Qualifiers != null)
                await _writeBase.AddQualifiableRelations(sourceTwinId, submodelElement.Qualifiers);
        }
    }
}
