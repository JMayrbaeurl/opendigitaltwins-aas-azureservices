using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;
using Azure.DigitalTwins.Core;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtSubmodelModelFactory : AdtSubmodelAndSmeCollectionBaseFactory<AdtSubmodel>
    {
        //private AdtSubmodelAndSmcInformation<AdtSubmodel> information;

        public AdtSubmodelModelFactory(AdtSubmodelAndSmcInformation<AdtSubmodel> information) : base(information)
        {
        }


        public async Task<Submodel> GetSubmodel()
        {
            var submodel = CreateSubmodelFromAdtSubmodel();
            submodel.SemanticId =
                GetSemanticId(this.information.ConcreteAasInformation.semanticId);
            foreach (var dataSpecification in information.ConcreteAasInformation.dataSpecifications)
            {
                submodel.EmbeddedDataSpecifications.Add(
                    CreateEmbeddedDataSpecificationFromAdtDataSpecification(dataSpecification));
            }

            foreach (var supplementalSemanticId in information.ConcreteAasInformation.supplementalSemanticId)
            {
                submodel.SupplementalSemanticIds.Add(GetSemanticId(supplementalSemanticId));
            }

            submodel.SubmodelElements = GetSubmodelElementsFromAdtSubmodelAndSMCInformation();

            return submodel;
        }

        private Submodel CreateSubmodelFromAdtSubmodel()
        {
            var adtSubmodel = this.information.RootElement;
            Submodel submodel = new Submodel(adtSubmodel.Id);
            submodel.Category = adtSubmodel.Category;
            submodel.Checksum = adtSubmodel.Checksum;
            submodel.IdShort = adtSubmodel.IdShort;
            submodel.Kind = adtSubmodel.Kind.Kind == "Instance" ? ModelingKind.Instance : ModelingKind.Template;
            submodel.Description = ConvertAdtLangStringToGeneraLangString(adtSubmodel.Description);
            submodel.DisplayName = ConvertAdtLangStringToGeneraLangString(adtSubmodel.DisplayName);
            submodel.EmbeddedDataSpecifications = new List<EmbeddedDataSpecification>();
            submodel.SubmodelElements = new List<ISubmodelElement>();
            submodel.SupplementalSemanticIds = new List<Reference>();
            return submodel;
        }
    }
}
