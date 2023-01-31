using AAS.ADT.Models;
using AAS.API.Repository.Adt.Exceptions;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Microsoft.Extensions.Logging;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelElementFactory<T> : AdtGeneralModelFactory where T : AdtBase, new()
    {
        protected AdtSubmodelAndSmcInformation<T> information;
        private readonly IAdtDefinitionsAndSemanticsModelFactory _definitionsAndSemanticsFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<AdtSubmodelElementFactory<T>> _logger;
        private readonly ILogger<AdtSmeCollectionModelFactory> _factoryLogger;

        public AdtSubmodelElementFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory,
            IMapper mapper, ILogger<AdtSubmodelElementFactory<T>> logger, ILogger<AdtSmeCollectionModelFactory> factoryLogger)
        {
            _definitionsAndSemanticsFactory =
                adtDefinitionsAndSemanticsModelFactory;
            _mapper = mapper;
            _logger = logger;
            _factoryLogger = factoryLogger;
        }

        public void Configure(AdtSubmodelAndSmcInformation<T> information)
        {
            this.information = information;
        }

        public List<ISubmodelElement> GetSubmodelElementsFromAdtSubmodelAndSMCInformation()
        {
            var submodelElements = new List<ISubmodelElement>();
            foreach (var adtProperty in information.properties)
            {
                try
                {
                    var property = _mapper.Map<Property>(adtProperty);
                    property.Value = adtProperty.Value;
                    property.SemanticId = GetSemanticIdForTwin(adtProperty.dtId);
                    property.SupplementalSemanticIds = _definitionsAndSemanticsFactory
                        .GetSupplementalSemanticIdsForTwin(adtProperty.dtId, information.definitionsAndSemantics);
                    property.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                        .GetEmbeddedDataSpecificationsForTwin(adtProperty.dtId, information.definitionsAndSemantics);
                    submodelElements.Add(property);
                }
                catch (AutoMapperMappingException e)
                {
                    _logger.LogError(e, e.Message);
                }
                catch (Exception e)
                {
                    throw new AASRepositoryException(e.Message, e);
                }


            }

            foreach (var adtFile in information.files)
            {
                try
                {
                    var file = _mapper.Map<File>(adtFile);
                    file.Value = adtFile.Value;
                    file.SemanticId = GetSemanticIdForTwin(adtFile.dtId);
                    file.SupplementalSemanticIds = _definitionsAndSemanticsFactory
                        .GetSupplementalSemanticIdsForTwin(adtFile.dtId, information.definitionsAndSemantics);
                    file.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                        .GetEmbeddedDataSpecificationsForTwin(adtFile.dtId, information.definitionsAndSemantics);
                    submodelElements.Add(file);
                }
                catch (AutoMapperMappingException e)
                {
                    _logger.LogError(e, e.Message);
                }
                catch (Exception e)
                {
                    throw new AASRepositoryException(e.Message, e);
                }
            }

            foreach (var smeCollectionInformation in information.smeCollections)
            {
                var smeC = CreateSubmodelElementCollectionFromSmeCollectionInformation(smeCollectionInformation);
                submodelElements.Add(smeC);
            }

            return submodelElements;
        }



        public Reference GetSemanticIdForTwin(string twinId)
        {
            try
            {
                var adtSemanticId = GetAdtSemanticId(twinId);
                return GetSemanticId(adtSemanticId);

            }
            catch (NoSemanticIdFound e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private AdtReference GetAdtSemanticId(string twinId)
        {
            if (information.definitionsAndSemantics.Relationships.ContainsKey(twinId))
            {
                foreach (var relationship in information.definitionsAndSemantics.Relationships[twinId])
                {
                    if (relationship.Name == "semanticId")
                    {
                        return information.definitionsAndSemantics.References[relationship.TargetId];
                    }
                }
            }

            throw new NoSemanticIdFound($"No Semantic Id found for twin with dtId {twinId}");
        }

        private SubmodelElementCollection CreateSubmodelElementCollectionFromSmeCollectionInformation(
            AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            var factory = new AdtSmeCollectionModelFactory(_definitionsAndSemanticsFactory, _mapper, _factoryLogger);
            var smeCollection = factory.GetSmeCollection(information);
            return smeCollection;
        }

    }
}
