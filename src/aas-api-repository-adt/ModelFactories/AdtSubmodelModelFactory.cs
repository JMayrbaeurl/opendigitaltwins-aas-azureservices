using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelModelFactory : AdtGeneralModelFactory, IAdtSubmodelModelFactory
    {
        private readonly AdtSubmodelElementFactory<AdtSubmodel> adtSubmodelElementFactory;

        public AdtSubmodelModelFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory,
            AdtSubmodelElementFactory<AdtSubmodel> adtSubmodelElementFactory)
        {
            this.adtSubmodelElementFactory = adtSubmodelElementFactory;
        }


        public async Task<Submodel> GetSubmodel(AdtSubmodelAndSmcInformation<AdtSubmodel> information)
        {
            adtSubmodelElementFactory.Configure(information);
            var submodel = CreateSubmodelFromAdtSubmodel(information.RootElement);
            submodel.SemanticId =
                adtSubmodelElementFactory.GetSemanticId(information.ConcreteAasInformation.semanticId);
            foreach (var dataSpecification in information.ConcreteAasInformation.dataSpecifications)
            {
                submodel.EmbeddedDataSpecifications.Add(
                    CreateEmbeddedDataSpecificationFromAdtDataSpecification(dataSpecification));
            }

            foreach (var supplementalSemanticId in information.ConcreteAasInformation.supplementalSemanticId)
            {
                submodel.SupplementalSemanticIds.Add(adtSubmodelElementFactory.GetSemanticId(supplementalSemanticId));
            }

            submodel.SubmodelElements = adtSubmodelElementFactory.GetSubmodelElementsFromAdtSubmodelAndSMCInformation();

            return submodel;
        }

        public void CreateSubmodelElement(ISubmodelElement submodelElement)
        {
            if (submodelElement.GetType()== typeof(Property))
            {
                Console.WriteLine("Property");
            }
            else
            {
                Console.WriteLine("no Property");
            }
        }

        private Submodel CreateSubmodelFromAdtSubmodel(AdtSubmodel adtSubmodel)
        {
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
            submodel.Administration = new AdministrativeInformation(
                null, adtSubmodel.Administration.Version, adtSubmodel.Administration.Revision);
            return submodel;
        }
    }
}
