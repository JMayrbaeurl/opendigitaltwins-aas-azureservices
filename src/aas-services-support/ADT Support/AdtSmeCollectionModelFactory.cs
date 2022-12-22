using System.Collections.Generic;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;
using AutoMapper;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtSmeCollectionModelFactory: AdtSubmodelElementFactory<AdtSubmodelElementCollection>
    {
        private readonly IMapper _mapper;

        public AdtSmeCollectionModelFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory, IMapper mapper) : base(adtDefinitionsAndSemanticsModelFactory,mapper)
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
