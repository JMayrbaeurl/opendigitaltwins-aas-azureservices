using System;
using System.Collections.Generic;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;

namespace AAS_Services_Support.ADT_Support
{
    public abstract class AdtSubmodelAndSmeCollectionBaseFactory<T> : AdtGeneralModelFactory where T : AdtBase, new()
    {
        protected AdtSubmodelAndSmcInformation<T> information;
        private readonly IAdtDefinitionsAndSemanticsModelFactory _definitionsAndSemanticsFactory;

        public AdtSubmodelAndSmeCollectionBaseFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory)
        {
            _definitionsAndSemanticsFactory =
                adtDefinitionsAndSemanticsModelFactory; 
        }

        public void Configure(AdtSubmodelAndSmcInformation<T> information)
        {
            this.information = information;
            _definitionsAndSemanticsFactory.Configure(information.definitionsAndSemantics);
        }

        protected List<ISubmodelElement> GetSubmodelElementsFromAdtSubmodelAndSMCInformation()
        {
            var submodelElements = new List<ISubmodelElement>();
            foreach (var adtProperty in information.properties)
            {
                var property = new Property(DataTypeDefXsd.String);
                property = UpdateSubmodelElementFromAdtSubmodelElement(property, adtProperty);
                property.Value = adtProperty.Value;
                property.SemanticId = GetSemanticIdForTwin(adtProperty.dtId);
                property.SupplementalSemanticIds = _definitionsAndSemanticsFactory.GetSupplementalSemanticIdsForTwin(adtProperty.dtId);
                property.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory.GetEmbeddedDataSpecificationsForTwin(adtProperty.dtId);
                submodelElements.Add(property);
            }

            foreach (var adtFile in information.files)
            {
                var file = new File(adtFile.ContentType);
                file = UpdateSubmodelElementFromAdtSubmodelElement(file, adtFile);
                file.Value = adtFile.Value;
                file.SemanticId = GetSemanticIdForTwin(adtFile.dtId);
                file.SupplementalSemanticIds = _definitionsAndSemanticsFactory.GetSupplementalSemanticIdsForTwin(adtFile.dtId);
                file.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory.GetEmbeddedDataSpecificationsForTwin(adtFile.dtId);
                submodelElements.Add(file);
            }

            foreach (var smeCollectionInformation in information.smeCollections)
            {
                var smeC = CreateSubmodelElementCollectionFromSmeCollectionInformation(smeCollectionInformation);
                submodelElements.Add(smeC);
            }


            return submodelElements;
        }

        

        protected Reference GetSemanticIdForTwin(string twinId)
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

        protected TSme UpdateSubmodelElementFromAdtSubmodelElement<TSme>(TSme sme, AdtSubmodelElement adtSubmodelElement) where TSme : ISubmodelElement
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

        private SubmodelElementCollection CreateSubmodelElementCollectionFromSmeCollectionInformation(
            AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            var factory = new AdtSmeCollectionModelFactory(_definitionsAndSemanticsFactory);
            var smeCollection = factory.GetSmeCollection(information);
            return smeCollection;
        }

        //protected DataSpecificationIec61360 GetIec61360DataSpecificationContent(AdtDataSpecificationIEC61360 adtIec61360)
        //{
        //    var preferredName = ConvertAdtLangStringToGeneraLangString(adtIec61360.PreferredName);
        //    var definition = ConvertAdtLangStringToGeneraLangString(adtIec61360.Definition);
        //    var shortName = ConvertAdtLangStringToGeneraLangString(adtIec61360.ShortName);

        //    var iec61360 = new DataSpecificationIec61360(preferredName);
        //    iec61360.ShortName = shortName;
        //    iec61360.Definition = definition;
        //    iec61360.Value = adtIec61360.Value;
        //    iec61360.SourceOfDefinition = adtIec61360.SourceOfDefinition;
        //    iec61360.Symbol = adtIec61360.Symbol;
        //    iec61360.Unit = adtIec61360.Unit;
        //    iec61360.ValueFormat = adtIec61360.ValueFormat;
        //    return iec61360;
        //}
    }
}
