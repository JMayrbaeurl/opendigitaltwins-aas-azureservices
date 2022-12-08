using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
//using AAS.API.Models;
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

            submodel.SubmodelElements = GetSubmodelElementsFromAdtSubmodelAndSMCInformation(submodelInformation);

            return submodel;
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
            //submodel.SemanticId = GetSemanticId(adtSubmodel.dtId);
            submodel.EmbeddedDataSpecifications = new List<EmbeddedDataSpecification>();
            submodel.SubmodelElements = new List<ISubmodelElement>();
            return submodel;
        }
        

        private T UpdateSubmodelElementFromAdtSubmodelElement<T>(T sme, AdtSubmodelElement adtSubmodelElement) where T : ISubmodelElement
        {

            sme.Kind = adtSubmodelElement.Kind.Kind == "Instance"
                ? ModelingKind.Instance
                : ModelingKind.Template;
            sme.DisplayName = ConvertAdtLangStringToGeneraLangString(adtSubmodelElement.DisplayName);
            sme.Description = ConvertAdtLangStringToGeneraLangString(adtSubmodelElement.Description);
            sme.Category = adtSubmodelElement.Category;
            sme.Checksum = adtSubmodelElement.Checksum;
            sme.IdShort = adtSubmodelElement.IdShort;

            return sme;
        }

        private List<ISubmodelElement> GetSubmodelElementsFromAdtSubmodelAndSMCInformation<T>(AdtSubmodelAndSMCInformation<T> information)
        {
            var submodelElements = new List<ISubmodelElement>();
            foreach (var adtProperty in information.properties)
            {
                // TODO support more than just string Properties
                var property = new Property(DataTypeDefXsd.String);
                property = UpdateSubmodelElementFromAdtSubmodelElement(property, adtProperty);
                property.Value = adtProperty.Value;
                submodelElements.Add(property);
            }

            foreach (var adtFile in information.files)
            {
                var file = new File(adtFile.ContentType);
                file = UpdateSubmodelElementFromAdtSubmodelElement(file, adtFile);
                file.Value = adtFile.Value;
                submodelElements.Add(file);
            }

            foreach (var smeCollectionInformation in information.smeCollections)
            {
                var smeC = CreateSubmodelElementCollectionFromSmeCollectionInformation(smeCollectionInformation);
                submodelElements.Add(smeC);
            }

            return submodelElements;
        }

        private SubmodelElementCollection CreateSubmodelElementCollectionFromSmeCollectionInformation(
            AdtSubmodelElementCollectionInformation information)
        {
            var smeCollection = new SubmodelElementCollection();
            smeCollection = UpdateSubmodelElementFromAdtSubmodelElement(smeCollection, information.RootElement);
            smeCollection.Value = GetSubmodelElementsFromAdtSubmodelAndSMCInformation(information);

            return smeCollection;
        }

        
    }
}
