using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Microsoft.Extensions.Logging;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelElementFactory : IAdtSubmodelElementFactory
    {
        private readonly IAdtDefinitionsAndSemanticsModelFactory _definitionsAndSemanticsFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<AdtSubmodelElementFactory> _logger;
        private readonly List<ISubmodelElement> _submodelElements;


        public AdtSubmodelElementFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory,
            IMapper mapper, ILogger<AdtSubmodelElementFactory> logger)
        {
            _definitionsAndSemanticsFactory =
                adtDefinitionsAndSemanticsModelFactory;
            _mapper = mapper;
            _logger = logger;
            _submodelElements = new List<ISubmodelElement>();
        }

        public List<ISubmodelElement> GetSubmodelElements(
            AdtSubmodelElements adtSubmodelElements, DefinitionsAndSemantics definitionsAndSemantics)
        {
            foreach (var adtProperty in adtSubmodelElements.properties)
            {
                AddDataElement<Property>(adtProperty, definitionsAndSemantics);
            }

            foreach (var adtFile in adtSubmodelElements.files)
            {
                AddDataElement<File>(adtFile, definitionsAndSemantics);
            }

            foreach (var smeCollectionInformation in adtSubmodelElements.smeCollections)
            {
                AddSubmodelElementCollection(smeCollectionInformation);
            }

            return _submodelElements;
        }

        private void AddDataElement<T>(AdtBase twin, DefinitionsAndSemantics definitionsAndSemantics) where T : IDataElement
        {
            var currentSme = GetSubmodelElementForTwin<T>(twin, definitionsAndSemantics);
            if (currentSme == null)
            {
                return;
            }
            _submodelElements.Add(currentSme);
        }


        private T? GetSubmodelElementForTwin<T>(AdtBase twin, DefinitionsAndSemantics definitionsAndSemantics) where T : ISubmodelElement
        {
            T currentSme;

            try
            {
                currentSme = _mapper.Map<T>(twin);
            }
            catch (AutoMapperMappingException e)
            {
                _logger.LogError(e, e.Message);
                return default(T);
            }

            currentSme.SemanticId =
                _definitionsAndSemanticsFactory.GetSemanticIdForTwin(twin.dtId, definitionsAndSemantics);
            currentSme.SupplementalSemanticIds = _definitionsAndSemanticsFactory
                .GetSupplementalSemanticIdsForTwin(twin.dtId, definitionsAndSemantics);
            currentSme.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                .GetEmbeddedDataSpecificationsForTwin(twin.dtId, definitionsAndSemantics);

            return currentSme;
        }

        private void AddSubmodelElementCollection(
            AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> smeCollectionInformation)
        {
            var adtSmeCollection = smeCollectionInformation.RootElement;
            var smeCollectionDefinitionsAndSemantics =
                smeCollectionInformation.DefinitionsAndSemantics;

            var currentSme =
                GetSubmodelElementForTwin<SubmodelElementCollection>(adtSmeCollection,
                    smeCollectionDefinitionsAndSemantics);
            if (currentSme == null)
            {
                return;
            }

            var smeFactory = new AdtSubmodelElementFactory(_definitionsAndSemanticsFactory, _mapper, _logger);
            currentSme.Value = smeFactory.GetSubmodelElements(
                smeCollectionInformation.AdtSubmodelElements, smeCollectionDefinitionsAndSemantics);
            _submodelElements.Add(currentSme);
        }

    }
}
