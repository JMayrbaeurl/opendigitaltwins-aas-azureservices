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
    public class AdtSmeCollectionModelFactory: AdtSubmodelAndSmeCollectionBaseFactory<AdtSubmodelElementCollection> 
    {
        private readonly IAdtSubmodelInteractions _adtSubmodelInteractions;
        private readonly IAdtInteractions _adtInteractions;
        //private AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information;

        public AdtSmeCollectionModelFactory(AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information) : base(information)
        {
            //this.information = information;
        }


        public SubmodelElementCollection GetSmeCollection()
        {
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


        

        

        //private List<EmbeddedDataSpecification> GetEmbeddedDataSpecifications<T>(AdtSubmodelAndSmcInformation<T> information) where T : AdtBase, new()
        //{
        //    var embeddedDataSpecifications = new List<EmbeddedDataSpecification>();
        //    List<BasicRelationship> twinRelationships;
        //    try
        //    {
        //        twinRelationships = information.definitionsAndSemantic.Relationships[information.RootElement.dtId];


        //        foreach (var twinRelationship in twinRelationships)
        //        {
        //            if (twinRelationship.Name == "semanticId")
        //            {
        //                var semanticIdReference =
        //                    information.definitionsAndSemantic.References[twinRelationship.TargetId];
        //                var conceptDescription = GetConceptDescription(semanticIdReference.dtId);

        //                if (conceptDescription != null)
        //                {
        //                    var adtDataSpecificationIec61360 =
        //                        GetDataSpecificationIec61360ForTwinWithId(conceptDescription.dtId);
        //                    var dataSpecificationIec61360 =
        //                        GetIec61360DataSpecificationContent(adtDataSpecificationIec61360);

        //                    var keys = new List<Key>()
        //                    { new Key(KeyTypes.GlobalReference, conceptDescription.Id) };
        //                    var dataSpecification = new Reference(ReferenceTypes.GlobalReference, keys);
        //                    embeddedDataSpecifications.Add(new EmbeddedDataSpecification(dataSpecification,
        //                        dataSpecificationIec61360));
        //                }

        //            }

        //            else if (twinRelationship.Name == "dataSpecification")
        //            {
        //                var adtDataSpecificationIec61360 = information.definitionsAndSemantic.Iec61360s[twinRelationship.TargetId];
        //                var dataSpecificationIec61360 = GetIec61360DataSpecificationContent(adtDataSpecificationIec61360);
        //                var keys = new List<Key>()
        //                { new Key(KeyTypes.GlobalReference, adtDataSpecificationIec61360.UnitIdValue) };
        //                var dataSpecification = new Reference(ReferenceTypes.GlobalReference, keys);
        //                embeddedDataSpecifications.Add(new EmbeddedDataSpecification(dataSpecification,
        //                    dataSpecificationIec61360));
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //    }

        //    return embeddedDataSpecifications;
        //}

        //private AdtConceptDescription GetConceptDescription(string dtId)
        //{
        //    var twinRelationships = this.information.definitionsAndSemantic.Relationships[dtId];
        //    foreach (var twinRelationship in twinRelationships)
        //    {
        //        if (twinRelationship.Name == "referredElement")
        //        {
        //            try

        //            {
        //                var conceptDescription =
        //                    information.definitionsAndSemantic.ConceptDescriptions[twinRelationship.TargetId];
        //                return conceptDescription;
        //            }
        //            catch (Exception e)
        //            {

        //            }

        //        }
        //    }
        //    return null;
        //}
        //private AdtDataSpecificationIEC61360 GetDataSpecificationIec61360ForTwinWithId(string twinId)
        //{
        //    var twinRelationships = this.information.definitionsAndSemantic.Relationships[twinId];
        //    foreach (var twinRelationship in twinRelationships)
        //    {
        //        if (twinRelationship.Name == "referredElement")
        //        {
        //            return GetDataSpecificationIec61360ForTwinWithId(twinRelationship.TargetId);
        //        }
        //        else if (twinRelationship.Name == "dataSpecification")
        //        {
        //            return information.definitionsAndSemantic.Iec61360s[twinRelationship.TargetId];
        //        }
        //    }
        //    throw new AdtException($"Could not find DataSpecificationIec61360 for twinId {twinId}");
        //}

        
    }
}
