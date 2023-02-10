using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.API.Repository.Adt
{
    public class AdtDefinitionsAndSemanticsModelFactory : IAdtDefinitionsAndSemanticsModelFactory
    {
        private DefinitionsAndSemantics _definitionsAndSemantics;
        private readonly AdtGeneralModelFactory _generalModelFactory;
        private readonly IMapper _mapper;

        public AdtDefinitionsAndSemanticsModelFactory(IMapper mapper)
        {
            _generalModelFactory = new AdtGeneralModelFactory();
            _mapper = mapper ??
                      throw new ArgumentNullException(nameof(mapper));
        }

        public List<Reference> GetSupplementalSemanticIdsForTwin(string twinId, DefinitionsAndSemantics definitionsAndSemantics)
        {
            _definitionsAndSemantics = definitionsAndSemantics;
            var supplementalSemanticIds = new List<Reference>();

            var adtSupplementalsSemanticIds = GetAdtSupplementalsSemanticIdsForTwin(twinId);
            if (adtSupplementalsSemanticIds.Count == 0)
            {
                return null;
            }
            foreach (var adtSupplementalsSemanticId in adtSupplementalsSemanticIds)
            {
                supplementalSemanticIds.Add(_generalModelFactory.GetSemanticId(adtSupplementalsSemanticId));

            }
            return supplementalSemanticIds;
        }

        private List<AdtReference> GetAdtSupplementalsSemanticIdsForTwin(string twinId)
        {
            var adtSupplementalSemanticIds = new List<AdtReference>();
            if (_definitionsAndSemantics.Relationships.ContainsKey(twinId))
            {
                foreach (var relationship in _definitionsAndSemantics.Relationships[twinId])
                {
                    if (relationship.Name == "supplementalSemanticId")
                    {
                        adtSupplementalSemanticIds.Add(_definitionsAndSemantics.References[relationship.TargetId]);
                    }
                }
            }

            return adtSupplementalSemanticIds;

        }

        public List<EmbeddedDataSpecification> GetEmbeddedDataSpecificationsForTwin(string dtId, DefinitionsAndSemantics definitionsAndSemantics)
        {
            _definitionsAndSemantics = definitionsAndSemantics;

            if (_definitionsAndSemantics.Relationships.ContainsKey(dtId) == false)
            {
                return null;
            }
            var twinRelationships = _definitionsAndSemantics.Relationships[dtId];

            var embeddedDataSpecifications = new List<EmbeddedDataSpecification>();

            foreach (var twinRelationship in twinRelationships)
            {
                var reference = new AdtReference();
                if (twinRelationship.Name == "semanticId" || twinRelationship.Name == "dataSpecification")
                {
                    reference = _definitionsAndSemantics.References[twinRelationship.TargetId];
                    var conceptDescription = GetConceptDescription(reference.dtId);

                    var adtDataSpecificationIec61360 = new AdtDataSpecificationIEC61360();

                    if (conceptDescription != null)
                    {
                        adtDataSpecificationIec61360 =
                            GetDataSpecificationIec61360ForTwinWithId(conceptDescription.dtId);
                    }
                    else if (_definitionsAndSemantics.Iec61360s.ContainsKey(reference.dtId))
                    {
                        adtDataSpecificationIec61360 = _definitionsAndSemantics.Iec61360s[reference.dtId];
                    }


                    if (adtDataSpecificationIec61360.PreferredName == null)
                        continue;

                    var dataSpecificationIec61360 = _mapper.Map<DataSpecificationIec61360>(adtDataSpecificationIec61360);

                    var keys = new List<Key>()
                        { new Key(KeyTypes.GlobalReference, conceptDescription.Id) };
                    var dataSpecification = new Reference(ReferenceTypes.GlobalReference, keys);
                    embeddedDataSpecifications.Add(new EmbeddedDataSpecification(dataSpecification,
                        dataSpecificationIec61360));
                }
            }
            return embeddedDataSpecifications.Count == 0 ? null : embeddedDataSpecifications;
        }

        private AdtConceptDescription GetConceptDescription(string dtId)
        {
            if (_definitionsAndSemantics.Relationships.ContainsKey(dtId))
            {

                var twinRelationships = _definitionsAndSemantics.Relationships[dtId];
                foreach (var twinRelationship in twinRelationships)
                {
                    if (twinRelationship.Name == "referredElement")
                    {
                        if (_definitionsAndSemantics.ConceptDescriptions.ContainsKey(twinRelationship.TargetId))

                        {
                            var conceptDescription =
                                _definitionsAndSemantics.ConceptDescriptions[twinRelationship.TargetId];
                            return conceptDescription;
                        }
                    }
                }
            }
            return null;
        }

        private AdtDataSpecificationIEC61360 GetDataSpecificationIec61360ForTwinWithId(string twinId)
        {
            if (_definitionsAndSemantics.Relationships.ContainsKey(twinId) == false)
            {
                return null;
            }
            var twinRelationships = this._definitionsAndSemantics.Relationships[twinId];
            foreach (var twinRelationship in twinRelationships)
            {
                if (twinRelationship.Name == "referredElement")
                {
                    return GetDataSpecificationIec61360ForTwinWithId(twinRelationship.TargetId);
                }
                else if (twinRelationship.Name == "dataSpecification")
                {
                    return _definitionsAndSemantics.Iec61360s[twinRelationship.TargetId];
                }
            }
            throw new AdtException($"Could not find DataSpecificationIec61360 for twinId {twinId}");
        }
    }
}
