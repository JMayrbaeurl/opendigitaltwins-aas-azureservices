using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtSubmodelAndSmeCollectionBaseFactory<T> : AdtGeneralModelFactory where T : AdtBase, new()
    {
        protected AdtSubmodelAndSmcInformation<T> information;

        public AdtSubmodelAndSmeCollectionBaseFactory(AdtSubmodelAndSmcInformation<T> information)
        {
            this.information = information;
        }




        private AdtReference GetAdtSemanticId(string twinId)
        {
            if (information.definitionsAndSemantic.Relationships.ContainsKey(twinId))
            {
                foreach (var relationship in information.definitionsAndSemantic.Relationships[twinId])
                {
                    if (relationship.Name == "semanticId")
                    {
                        return information.definitionsAndSemantic.References[relationship.TargetId];
                    }
                }
            }

            throw new NoSemanticIdFound($"No Semantic Id found for twin with dtId {twinId}");
        }

        protected List<Reference> GetSupplementalSemanticIdsForTwin(string twinId)
        {
            var supplementalSemanticIds = new List<Reference>();

            var adtSupplementalsSemanticIds = GetAdtSupplementalsSemanticIdsForTwin(twinId);
            if (adtSupplementalsSemanticIds.Count == 0)
            {
                return null;
            }
            foreach (var adtSupplementalsSemanticId in adtSupplementalsSemanticIds)
            {
                supplementalSemanticIds.Add(GetSemanticId(adtSupplementalsSemanticId));

            }
            return supplementalSemanticIds;
        }

        private List<AdtReference> GetAdtSupplementalsSemanticIdsForTwin(string twinId)
        {
            var adtSupplementalSemanticIds = new List<AdtReference>();
            if (information.definitionsAndSemantic.Relationships.ContainsKey(twinId))
            {
                foreach (var relationship in information.definitionsAndSemantic.Relationships[twinId])
                {
                    if (relationship.Name == "supplementalSemanticId")
                    {
                        adtSupplementalSemanticIds.Add(information.definitionsAndSemantic.References[relationship.TargetId]);
                    }
                }
            }

            return adtSupplementalSemanticIds;

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
                property.SupplementalSemanticIds = GetSupplementalSemanticIdsForTwin(adtProperty.dtId);
                property.EmbeddedDataSpecifications = GetEmbeddedDataSpecificationsForTwin(adtProperty.dtId);
                submodelElements.Add(property);
            }

            foreach (var adtFile in information.files)
            {
                var file = new File(adtFile.ContentType);
                file = UpdateSubmodelElementFromAdtSubmodelElement(file, adtFile);
                file.Value = adtFile.Value;
                file.SemanticId = GetSemanticIdForTwin(adtFile.dtId);
                file.SupplementalSemanticIds = GetSupplementalSemanticIdsForTwin(adtFile.dtId);
                file.EmbeddedDataSpecifications = GetEmbeddedDataSpecificationsForTwin(adtFile.dtId);
                submodelElements.Add(file);
            }

            foreach (var smeCollectionInformation in information.smeCollections)
            {
                var smeC = CreateSubmodelElementCollectionFromSmeCollectionInformation(smeCollectionInformation);
                submodelElements.Add(smeC);
            }


            return submodelElements;
        }

        private List<EmbeddedDataSpecification> GetEmbeddedDataSpecificationsForTwin(string dtId)
        {
            if (information.definitionsAndSemantic.Relationships.ContainsKey(dtId) == false)
            {
                return null;
            }
            var twinRelationships = information.definitionsAndSemantic.Relationships[dtId];

            var embeddedDataSpecifications = new List<EmbeddedDataSpecification>();

            foreach (var twinRelationship in twinRelationships)
            {
                if (twinRelationship.Name == "semanticId")
                {
                    var semanticIdReference =
                        information.definitionsAndSemantic.References[twinRelationship.TargetId];
                    var conceptDescription = GetConceptDescription(semanticIdReference.dtId);

                    if (conceptDescription != null)
                    {
                        var adtDataSpecificationIec61360 =
                            GetDataSpecificationIec61360ForTwinWithId(conceptDescription.dtId);
                        var dataSpecificationIec61360 =
                            GetIec61360DataSpecificationContent(adtDataSpecificationIec61360);

                        var keys = new List<Key>()
                            { new Key(KeyTypes.GlobalReference, conceptDescription.Id) };
                        var dataSpecification = new Reference(ReferenceTypes.GlobalReference, keys);
                        embeddedDataSpecifications.Add(new EmbeddedDataSpecification(dataSpecification,
                            dataSpecificationIec61360));
                    }

                }

                else if (twinRelationship.Name == "dataSpecification")
                {
                    if (information.definitionsAndSemantic.Iec61360s.ContainsKey(twinRelationship.TargetId))
                    {
                        var adtDataSpecificationIec61360 = information.definitionsAndSemantic.Iec61360s[twinRelationship.TargetId];
                        var dataSpecificationIec61360 = GetIec61360DataSpecificationContent(adtDataSpecificationIec61360);
                        var keys = new List<Key>()
                        { new Key(KeyTypes.GlobalReference, adtDataSpecificationIec61360.UnitIdValue) };
                        var dataSpecification = new Reference(ReferenceTypes.GlobalReference, keys);
                        embeddedDataSpecifications.Add(new EmbeddedDataSpecification(dataSpecification,
                            dataSpecificationIec61360));
                    }
                }
            }
            return embeddedDataSpecifications.Count == 0 ? null : embeddedDataSpecifications;
        }

        private AdtConceptDescription GetConceptDescription(string dtId)
        {
            if (information.definitionsAndSemantic.Relationships.ContainsKey(dtId))
            {

                var twinRelationships = information.definitionsAndSemantic.Relationships[dtId];
                foreach (var twinRelationship in twinRelationships)
                {
                    if (twinRelationship.Name == "referredElement")
                    {
                        if (information.definitionsAndSemantic.ConceptDescriptions.ContainsKey(twinRelationship.TargetId))

                        {
                            var conceptDescription =
                                information.definitionsAndSemantic.ConceptDescriptions[twinRelationship.TargetId];
                            return conceptDescription;
                        }
                    }
                }
            }
            return null;
        }

        private AdtDataSpecificationIEC61360 GetDataSpecificationIec61360ForTwinWithId(string twinId)
        {
            var twinRelationships = this.information.definitionsAndSemantic.Relationships[twinId];
            foreach (var twinRelationship in twinRelationships)
            {
                if (twinRelationship.Name == "referredElement")
                {
                    return GetDataSpecificationIec61360ForTwinWithId(twinRelationship.TargetId);
                }
                else if (twinRelationship.Name == "dataSpecification")
                {
                    return information.definitionsAndSemantic.Iec61360s[twinRelationship.TargetId];
                }
            }
            throw new AdtException($"Could not find DataSpecificationIec61360 for twinId {twinId}");
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
            var factory = new AdtSmeCollectionModelFactory(information);
            var smeCollection = factory.GetSmeCollection();
            return smeCollection;
        }
    }
}
