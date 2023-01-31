using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository.Adt
{
    public class AdtSmeCollectionModelFactory: AdtSubmodelElementFactory<AdtSubmodelElementCollection>
    {
        private readonly IMapper _mapper;

        public AdtSmeCollectionModelFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory, 
            IMapper mapper, ILogger<AdtSmeCollectionModelFactory> logger) : 
            base(adtDefinitionsAndSemanticsModelFactory,mapper, logger,logger)
        {
            _mapper = mapper;
        }

        public SubmodelElementCollection GetSmeCollection(AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            Configure(information);
            var smeCollection = CreateSubmodelElementCollectionFromAdtSmeCollection();
            smeCollection.SemanticId =
                GetSemanticId(this.information.ConcreteAasInformation.semanticId);
            foreach (var dataSpecification in information.ConcreteAasInformation.dataSpecifications)
            {
                smeCollection.EmbeddedDataSpecifications.Add(
                    CreateEmbeddedDataSpecificationFromAdtDataSpecification(dataSpecification));
            }
            foreach (var supplementalSemanticId in information.ConcreteAasInformation.supplementalSemanticId)
            {
                smeCollection.SupplementalSemanticIds.Add(GetSemanticId(supplementalSemanticId));
            }
            smeCollection.Value = GetSubmodelElementsFromAdtSubmodelAndSMCInformation();

            return smeCollection;
        }

        private SubmodelElementCollection CreateSubmodelElementCollectionFromAdtSmeCollection()
        {
            var adtSmeCollection = information.RootElement;
            var smeCollection = new SubmodelElementCollection();
            smeCollection.SupplementalSemanticIds = new List<Reference>();
            smeCollection = _mapper.Map(adtSmeCollection, smeCollection);
            return smeCollection;
        }        
    }
}
