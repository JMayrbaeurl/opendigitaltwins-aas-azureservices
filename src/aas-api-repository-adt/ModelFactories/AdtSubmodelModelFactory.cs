using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt
{
    public class AdtSubmodelModelFactory : AdtGeneralModelFactory, IAdtSubmodelModelFactory
    {
        private readonly AdtSubmodelElementFactory _adtSubmodelElementFactory;
        private readonly IAdtDefinitionsAndSemanticsModelFactory _definitionsAndSemanticsFactory;

        public AdtSubmodelModelFactory(IAdtDefinitionsAndSemanticsModelFactory definitionsAndSemanticsFactory,
            AdtSubmodelElementFactory adtSubmodelElementFactory)
        {
            _adtSubmodelElementFactory = adtSubmodelElementFactory;
            _definitionsAndSemanticsFactory = definitionsAndSemanticsFactory;
        }


        public async Task<Submodel> GetSubmodel(AdtSubmodelAndSmcInformation<AdtSubmodel> information)
        {
            var submodelTwinId = information.GeneralAasInformation.RootElement.dtId;
            
            var submodel = CreateSubmodelFromAdtSubmodel(information.GeneralAasInformation.RootElement);
            
            submodel.SemanticId =
                _definitionsAndSemanticsFactory.GetSemanticId(information.GeneralAasInformation.ConcreteAasInformation.semanticId);

            submodel.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory
                .GetEmbeddedDataSpecificationsForTwin(submodelTwinId,
                    information.GeneralAasInformation.definitionsAndSemantics);

            submodel.SupplementalSemanticIds = _definitionsAndSemanticsFactory.GetSupplementalSemanticIdsForTwin(
                submodelTwinId, information.GeneralAasInformation.definitionsAndSemantics);
            
            submodel.SubmodelElements = _adtSubmodelElementFactory.GetSubmodelElements(
                information.AdtSubmodelElements,information.GeneralAasInformation.definitionsAndSemantics);

            return submodel;
        }

        private Submodel CreateSubmodelFromAdtSubmodel(AdtSubmodel adtSubmodel)
        {
            var submodel = new Submodel(adtSubmodel.Id);
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
