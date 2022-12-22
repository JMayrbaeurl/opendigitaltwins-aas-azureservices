using System;
using System.Collections.Generic;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;
using AutoMapper;

namespace AAS_Services_Support.ADT_Support
{
    public class AdtSubmodelElementFactory<T> : AdtGeneralModelFactory where T : AdtBase, new()
    {
        protected AdtSubmodelAndSmcInformation<T> information;
        private readonly IAdtDefinitionsAndSemanticsModelFactory _definitionsAndSemanticsFactory;
        private readonly IMapper _mapper;

        public AdtSubmodelElementFactory(IAdtDefinitionsAndSemanticsModelFactory adtDefinitionsAndSemanticsModelFactory, IMapper mapper)
        {
            _definitionsAndSemanticsFactory =
                adtDefinitionsAndSemanticsModelFactory;
            _mapper = mapper;
        }

        public void Configure(AdtSubmodelAndSmcInformation<T> information)
        {
            this.information = information;
            _definitionsAndSemanticsFactory.Configure(information.definitionsAndSemantics);
        }

        public List<ISubmodelElement> GetSubmodelElementsFromAdtSubmodelAndSMCInformation()
        {
            var submodelElements = new List<ISubmodelElement>();
            foreach (var adtProperty in information.properties)
            {
                var property = _mapper.Map<Property>(adtProperty);
                property.Value = adtProperty.Value;
                property.SemanticId = GetSemanticIdForTwin(adtProperty.dtId);
                property.SupplementalSemanticIds = _definitionsAndSemanticsFactory.GetSupplementalSemanticIdsForTwin(adtProperty.dtId);
                property.EmbeddedDataSpecifications = _definitionsAndSemanticsFactory.GetEmbeddedDataSpecificationsForTwin(adtProperty.dtId);
                submodelElements.Add(property);
            }

            foreach (var adtFile in information.files)
            {
                var file = _mapper.Map<File>(adtFile);
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

        

        public Reference GetSemanticIdForTwin(string twinId)
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

        private SubmodelElementCollection CreateSubmodelElementCollectionFromSmeCollectionInformation(
            AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> information)
        {
            var factory = new AdtSmeCollectionModelFactory(_definitionsAndSemanticsFactory,_mapper);
            var smeCollection = factory.GetSmeCollection(information);
            return smeCollection;
        }

    }
}
