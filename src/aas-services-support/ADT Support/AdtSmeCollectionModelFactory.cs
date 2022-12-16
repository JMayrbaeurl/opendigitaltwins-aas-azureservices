using System.Collections.Generic;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtSmeCollectionModelFactory: AdtSubmodelAndSmeCollectionBaseFactory<AdtSubmodelElementCollection> 
    {
        private readonly IAdtSubmodelInteractions _adtSubmodelInteractions;
        private readonly IAdtInteractions _adtInteractions;

        public AdtSmeCollectionModelFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory) : base(adtDefinitionsAndSemanticsModelFactory)
        {
        }


        public SubmodelElementCollection GetSmeCollection(AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            Configure(information);
            var smeCollection = CreateSubmodelElementCollectionFromADTSmeCollection();
            //smeCollection.EmbeddedDataSpecifications = GetEmbeddedDataSpecifications(information);
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

        private SubmodelElementCollection CreateSubmodelElementCollectionFromADTSmeCollection()
        {
            var adtSmeCollection = information.RootElement;
            var smeCollection = new SubmodelElementCollection();
            smeCollection.SupplementalSemanticIds = new List<Reference>();
            smeCollection = UpdateSubmodelElementFromAdtSubmodelElement(smeCollection, adtSmeCollection);
            return smeCollection;
        }        
    }
}
