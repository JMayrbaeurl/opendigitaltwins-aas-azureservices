using AAS.ADT.Models;
using AAS.API.Repository.Adt.Exceptions;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Microsoft.Extensions.Logging;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelElementFactory : AdtGeneralModelFactory
    {
        private readonly IAdtDefinitionsAndSemanticsModelFactory _definitionsAndSemanticsFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<AdtSubmodelElementFactory> _logger;

        public AdtSubmodelElementFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory,
            IMapper mapper, ILogger<AdtSubmodelElementFactory> logger)
        {
            _definitionsAndSemanticsFactory =
                adtDefinitionsAndSemanticsModelFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public List<ISubmodelElement> GetSubmodelElements(
            AdtSubmodelElements adtSubmodelElements, DefinitionsAndSemantics definitionsAndSemantics)
        {
            var submodelElements = new List<ISubmodelElement>();
            foreach (var adtProperty in adtSubmodelElements.properties)
            {
                var property = GetPropertyForTwin(adtProperty, definitionsAndSemantics);
                if (property == null)
                {
                    continue;
                }
                submodelElements.Add(property);
            }

            foreach (var adtFile in adtSubmodelElements.files)
            {
                var file = GetFileForTwin(adtFile, definitionsAndSemantics);
                if (file == null)
                {
                    continue;
                }
                submodelElements.Add(file);

            }

            foreach (var smeCollectionInformation in adtSubmodelElements.smeCollections)
            {
                var smeC = CreateSubmodelElementCollectionFromSmeCollectionInformation(smeCollectionInformation);
                submodelElements.Add(smeC);
            }

            return submodelElements;
        }

        private Property? GetPropertyForTwin(AdtProperty adtProperty, DefinitionsAndSemantics definitionsAndSemantics)
        {
            Property property = new Property(DataTypeDefXsd.Boolean);
            try
            {
                property = _mapper.Map<Property>(adtProperty);

            }
            catch (AutoMapperMappingException e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }
            catch (Exception e)
            {
                throw new AASRepositoryException(e.Message, e);
            }

            property.SemanticId = _definitionsAndSemanticsFactory.GetSemanticIdForTwin(adtProperty.dtId, definitionsAndSemantics);
            property.SupplementalSemanticIds = _definitionsAndSemanticsFactory
                .GetSupplementalSemanticIdsForTwin(adtProperty.dtId, definitionsAndSemantics);
            property.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                .GetEmbeddedDataSpecificationsForTwin(adtProperty.dtId, definitionsAndSemantics);
            return property;
        }

        private File? GetFileForTwin(AdtFile adtFile, DefinitionsAndSemantics definitionsAndSemantics)
        {
            File file = new File("dummyContentType");
            try
            {
                file = _mapper.Map<File>(adtFile);
            }
            catch (AutoMapperMappingException e)
            {
                _logger.LogError(e, e.Message);
            }
            catch (Exception e)
            {
                throw new AASRepositoryException(e.Message, e);
            }

            file.SemanticId = _definitionsAndSemanticsFactory.GetSemanticIdForTwin(adtFile.dtId, definitionsAndSemantics);
            file.SupplementalSemanticIds = _definitionsAndSemanticsFactory
                .GetSupplementalSemanticIdsForTwin(adtFile.dtId, definitionsAndSemantics);
            file.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                .GetEmbeddedDataSpecificationsForTwin(adtFile.dtId, definitionsAndSemantics);
            return file;
        }



        private SubmodelElementCollection CreateSubmodelElementCollectionFromSmeCollectionInformation(
            AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            var factory = new AdtSubmodelElementFactory(_definitionsAndSemanticsFactory, _mapper, _logger);
            var smeCollection = factory.GetSmeCollection(information);
            return smeCollection;
        }

        private SubmodelElementCollection GetSmeCollection(
            AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            var smeDtId = information.GeneralAasInformation.RootElement.dtId;
            var smeCollection = CreateSubmodelElementCollectionFromAdtSmeCollection(information);
            
            smeCollection.SemanticId =
                _definitionsAndSemanticsFactory.GetSemanticId(information.GeneralAasInformation.ConcreteAasInformation.semanticId);

            smeCollection.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                .GetEmbeddedDataSpecificationsForTwin(smeDtId, information.GeneralAasInformation.definitionsAndSemantics);
            
            smeCollection.SupplementalSemanticIds = _definitionsAndSemanticsFactory.GetSupplementalSemanticIdsForTwin(
                smeDtId,information.GeneralAasInformation.definitionsAndSemantics);
            
            smeCollection.Value = GetSubmodelElements(
                information.AdtSubmodelElements, information.GeneralAasInformation.definitionsAndSemantics);

            return smeCollection;
        }

        private SubmodelElementCollection CreateSubmodelElementCollectionFromAdtSmeCollection(
            AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            var adtSmeCollection = information.GeneralAasInformation.RootElement;
            var smeCollection = new SubmodelElementCollection();
            smeCollection.SupplementalSemanticIds = new List<Reference>();
            smeCollection = _mapper.Map(adtSmeCollection, smeCollection);
            return smeCollection;
        }

    }
}
