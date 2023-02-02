using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.API.Repository.Adt
{
    public class AdtDefinitionsAndSemanticsModelFactory : IAdtDefinitionsAndSemanticsModelFactory
    {
        private DefinitionsAndSemantics _definitionsAndSemantics;
        private readonly IMapper _mapper;

        public AdtDefinitionsAndSemanticsModelFactory(IMapper mapper)
        {
            _mapper = mapper ??
                      throw new ArgumentNullException(nameof(mapper));
        }

        public Reference? GetSemanticIdForTwin(string twinId, DefinitionsAndSemantics definitionsAndSemantics)
        {
            var adtSemanticId = GetAdtSemanticId(twinId, definitionsAndSemantics);
            return GetSemanticId(adtSemanticId);
        }

        private AdtReference? GetAdtSemanticId(string twinId, DefinitionsAndSemantics definitionsAndSemantics)
        {
            if (!definitionsAndSemantics.Relationships.ContainsKey(twinId))
            {
                return null;
            }

            foreach (var relationship in definitionsAndSemantics.Relationships[twinId])
            {
                if (relationship.Name == "semanticId")
                {
                    return definitionsAndSemantics.References[relationship.TargetId];
                }
            }

            return null;
        }

        public Reference? GetSemanticId(AdtReference? adtReference)
        {
            if (adtReference==null || adtReference.Key1 == null)
            {
                return null;
            }

            var semanticId = ConvertAdtReferenceToGeneralReference(adtReference);
            semanticId.Keys = new List<Key>() { semanticId.Keys[0] };
            return semanticId;
        }

        private Reference ConvertAdtReferenceToGeneralReference(AdtReference adtReference)
        {
            var referenceType = adtReference.Type == "ModelReference"
                ? ReferenceTypes.ModelReference
                : ReferenceTypes.GlobalReference;
            var reference = new Reference(referenceType, new List<Key>());
            reference.Keys = new List<Key>();

            for (int i = 0; i < 8; i++)
            {
                var adtKey = (AdtKey)adtReference.GetType().GetProperty($"Key{i + 1}")!.GetValue(adtReference);
                if (adtKey != null && adtKey.Type != null)
                {
                    reference.Keys.Add(ConvertAdtKeyToGeneralKey(adtKey));
                }
            }

            return reference;
        }

        private Key ConvertAdtKeyToGeneralKey(AdtKey adtKey)
        {
            return new Key((KeyTypes)Enum.Parse(typeof(KeyTypes), adtKey.Type), adtKey.Value);
        }

        public List<Reference>? GetSupplementalSemanticIdsForTwin(string twinId, DefinitionsAndSemantics definitionsAndSemantics)
        {
            _definitionsAndSemantics = definitionsAndSemantics;
            var supplementalSemanticIds = new List<Reference>();

            var adtSupplementalSemanticIds = GetAdtSupplementalSemanticIdsForTwin(twinId);
            if (adtSupplementalSemanticIds.Count == 0)
            {
                return null;
            }
            foreach (var adtSupplementalSemanticId in adtSupplementalSemanticIds)
            {
                var semanticId = GetSemanticId(adtSupplementalSemanticId);
                if (semanticId!= null)
                {
                    supplementalSemanticIds.Add(semanticId);
                }
            }
            return supplementalSemanticIds;
        }

        private List<AdtReference> GetAdtSupplementalSemanticIdsForTwin(string twinId)
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

        public List<EmbeddedDataSpecification>? GetEmbeddedDataSpecificationsForTwin(string dtId, DefinitionsAndSemantics definitionsAndSemantics)
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
                if (twinRelationship.Name != "semanticId" && twinRelationship.Name != "dataSpecification")
                {
                    continue;
                }

                if (_definitionsAndSemantics.References.ContainsKey(twinRelationship.TargetId)==false)
                {
                    continue;
                }
                var reference = _definitionsAndSemantics.References[twinRelationship.TargetId];
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
            return embeddedDataSpecifications.Count == 0 ? null : embeddedDataSpecifications;
        }

        private AdtConceptDescription? GetConceptDescription(string dtId)
        {
            if (!_definitionsAndSemantics.Relationships.ContainsKey(dtId))
            {
                return null;
            }
            var twinRelationships = _definitionsAndSemantics.Relationships[dtId];
            foreach (var twinRelationship in twinRelationships)
            {
                if (twinRelationship.Name != "referredElement")
                {
                    continue;
                }

                if (!_definitionsAndSemantics.ConceptDescriptions.ContainsKey(twinRelationship.TargetId))
                {
                    continue;
                }
                var conceptDescription =
                    _definitionsAndSemantics.ConceptDescriptions[twinRelationship.TargetId];
                return conceptDescription;
            }
            return null;
        }

        private AdtDataSpecificationIEC61360 GetDataSpecificationIec61360ForTwinWithId(string twinId)
        {
            if (_definitionsAndSemantics.Relationships.ContainsKey(twinId) == false)
            {
                return null;
            }
            var twinRelationships = _definitionsAndSemantics.Relationships[twinId];
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
