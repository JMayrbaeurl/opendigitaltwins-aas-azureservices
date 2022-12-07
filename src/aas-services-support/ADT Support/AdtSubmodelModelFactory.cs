using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAS.API.Models;
using AdtModels.AdtModels;
using Azure.DigitalTwins.Core;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtSubmodelModelFactory : AdtGeneralModelFactory
    {
        private readonly IAdtSubmodelInteractions _adtSubmodelInteractions;
        private readonly IAdtInteractions _adtInteractions;

        public AdtSubmodelModelFactory(IAdtSubmodelInteractions adtSubmodelInteractions, IAdtInteractions adtInteractions) 
        {
            _adtSubmodelInteractions = adtSubmodelInteractions;
            _adtInteractions = adtInteractions;
        }


        public async Task<Submodel> GetSubmodelFromTwinId(string submodelTwinId)
        {
            var submodelInformation = await _adtSubmodelInteractions.GetAllInformationForSubmodelWithTwinId(submodelTwinId);
            var submodel = CreateSubmodelFromAdtSubmodel(submodelInformation.RootElement);
            
            foreach (var dataSpecification in submodelInformation.ConcreteAasInformation.dataSpecifications)
            {
                submodel.EmbeddedDataSpecifications.Add(
                    CreateEmbeddedDataSpecificationFromAdtDataSpecification(dataSpecification));
            }

            return submodel;
        }

        private Submodel CreateSubmodelFromAdtSubmodel(AdtSubmodel adtSubmodel)
        {
            Submodel submodel = new Submodel();
            submodel.Category = adtSubmodel.Category;
            submodel.Id = adtSubmodel.Id;
            submodel.Checksum = adtSubmodel.Checksum;
            submodel.IdShort = adtSubmodel.IdShort;
            submodel.Kind = adtSubmodel.Kind.Kind == "Instance" ? ModelingKind.InstanceEnum : ModelingKind.TemplateEnum;
            submodel.Description = ConvertAdtLangStringToGeneraLangString(adtSubmodel.Description);
            submodel.DisplayName = ConvertAdtLangStringToGeneraLangString(adtSubmodel.DisplayName);
            //submodel.SemanticId = GetSemanticId(adtSubmodel.dtId);
            submodel.EmbeddedDataSpecifications = new List<EmbeddedDataSpecification>();
            
            return submodel;
        }

        private List<SubmodelElement> GetAllSubmodelElementsForAdtTwinId(string adtTwinId)
        {
            List<SubmodelElement> submodelElements = new List<SubmodelElement>();
            var adtSubmodelElements = _adtInteractions.GetAdtSubmodelElementsFromParentTwinWithId(adtTwinId);
            foreach (var adtSubmodelElement in adtSubmodelElements)
            {
                submodelElements.Add(CreateSubmodelElementFromAdtSubmodelElement(adtSubmodelElement));
            }
            return submodelElements;
        }

        private SubmodelElement CreateSubmodelElementFromAdtSubmodelElement(AdtSubmodelElement adtSubmodelElement)
        {
            var sme = new SubmodelElement();
            
            var adtSmeType = adtSubmodelElement.GetType();
            if (adtSmeType == typeof(AdtProperty))
            {
                sme.ModelType = ModelType.PropertyEnum;
            }
            else if (adtSmeType == typeof(AdtSubmodelElementCollection))
            {
                sme.ModelType = ModelType.SubmodelElementCollectionEnum;
            }
            else if (adtSmeType == typeof(AdtFile))
            {
                sme.ModelType = ModelType.FileEnum;
            }
            else
            {
                throw new AdtException($"SubmodelElementType {adtSubmodelElement.GetType()} ist not supported for conversion");
            }
            sme.Kind = adtSubmodelElement.Kind.Kind == "Instance"
                ? ModelingKind.InstanceEnum
                : ModelingKind.TemplateEnum;
            sme.DisplayName = ConvertAdtLangStringToGeneraLangString(adtSubmodelElement.DisplayName);
            sme.Description = ConvertAdtLangStringToGeneraLangString(adtSubmodelElement.Description);
            sme.Category = adtSubmodelElement.Category;
            sme.Checksum = adtSubmodelElement.Checksum;
            sme.IdShort = adtSubmodelElement.IdShort;

            return sme;
        }
    }
}
