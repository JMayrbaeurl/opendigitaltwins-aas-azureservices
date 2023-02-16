using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Azure.DigitalTwins.Core;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace AAS.API.Repository.Adt.Tests
{
    [TestClass]
    public class AdtDefinitionsAndSemanticsModelFactoryTests
    {
        private Mock<IMapper> _mapper;
        private AdtDefinitionsAndSemanticsModelFactory _objectUnderTest;
        private AutoMocker _autoMocker;
        private DefinitionsAndSemantics _definitionsAndSemanticsWithEmbeddedDataSpec;
        private DefinitionsAndSemantics _definitionsAndSemanticsWithSupplementalSemanticIds;
        private DefinitionsAndSemantics _definitionsAndSemanticsWithSemanticId;


        [TestInitialize]
        public void Setup()
        {
            _autoMocker = new AutoMocker();
            _mapper = _autoMocker.GetMock<IMapper>();
            _objectUnderTest = _autoMocker.CreateInstance<AdtDefinitionsAndSemanticsModelFactory>();
            _mapper.Setup(_ => _.Map<DataSpecificationIec61360>(It.IsAny<AdtDataSpecificationIEC61360>())).Returns(
                new DataSpecificationIec61360(new List<LangString>() { new LangString("de", "TestLangString") }));

            _definitionsAndSemanticsWithEmbeddedDataSpec = new DefinitionsAndSemantics
            {
                References = new Dictionary<string, AdtReference>()
                {
                    ["TestSemanticId"] = new AdtReference
                    {
                        dtId = "TestSemanticId",
                        Key1 = new AdtKey
                        {
                            Type = "GlobalReference",
                            Value = "SemanticId"
                        }
                    }
                },
                Iec61360s = new Dictionary<string, AdtDataSpecificationIEC61360>()
                {
                    ["TestDataSpecification1"] = new()
                    {
                        PreferredName = new AdtLanguageString() { LangStrings = new Dictionary<string, string>() { ["en"] = "TestPreferredName" } }
                    }

                },
                ConceptDescriptions = new Dictionary<string, AdtConceptDescription>()
                {
                    ["TestConceptDescription1"] = new()
                    {
                        dtId = "TestConceptDescription1"
                    }
                },
                Relationships = new Dictionary<string, List<BasicRelationship>>()
                {
                    ["TestId"] = new List<BasicRelationship>()
                    {
                        new BasicRelationship()
                            { Name = "semanticId", TargetId = "TestSemanticId" }
                    },
                    ["TestSemanticId"] = new List<BasicRelationship>()
                    {
                        new BasicRelationship()
                            { Name = "referredElement", TargetId = "TestConceptDescription1" }
                    },
                    ["TestConceptDescription1"] = new List<BasicRelationship>()
                    {
                        new BasicRelationship()
                            { Name = "dataSpecification", TargetId = "TestDataSpecification1" }
                    }

                }
            };

            _definitionsAndSemanticsWithSupplementalSemanticIds = new DefinitionsAndSemantics
            {
                References = new Dictionary<string, AdtReference>()
                {
                    ["TestSupplementalIdTwin1"] = new AdtReference
                    {
                        Key1 = new AdtKey
                        {
                            Type = "GlobalReference",
                            Value = "SupplementalId1"
                        }
                    },
                    ["TestSupplementalIdTwin2"] = new AdtReference
                    {
                        Key1 = new AdtKey
                        {
                            Type = "GlobalReference",
                            Value = "SupplementalId2"
                        }
                    }
                },
                Relationships = new Dictionary<string, List<BasicRelationship>>()
                {
                    ["TestId"] = new List<BasicRelationship>()
                    {
                        new BasicRelationship()
                            { Name = "supplementalSemanticId", TargetId = "TestSupplementalIdTwin1" },
                        new BasicRelationship()
                            { Name = "supplementalSemanticId", TargetId = "TestSupplementalIdTwin2" }
                    }
                }
            };

            _definitionsAndSemanticsWithSemanticId = new DefinitionsAndSemantics
            {
                References = new Dictionary<string, AdtReference>()
                {
                    ["TestSemanticTwinId"] = new AdtReference
                    {
                        Key1 = new AdtKey() {Type = "GlobalReference", Value = "TestSemanticId" }
                    }
                },
                Relationships = new Dictionary<string, List<BasicRelationship>>()
                {
                    ["TestId"] = new List<BasicRelationship>()
                    {
                        new BasicRelationship(){TargetId = "TestSemanticTwinId",Name = "semanticId"}
                    }
                }
            };

        }

        [TestMethod]
        public void GetSemanticId_returns_correct_Reference()
        {
            AdtReference adtSemanticId = new AdtReference()
                { Key1 = new AdtKey() { Type = "GlobalReference", Value = "TestSemanticId" } };
            var actual = _objectUnderTest.GetSemanticId(adtSemanticId);
            var expected = new Reference(ReferenceTypes.GlobalReference,
                new List<Key>() { new Key(KeyTypes.GlobalReference, "TestSemanticId") });
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetSemanticId_returns_always_Reference_with_just_one_KeyValuePair()
        {
            AdtReference adtSemanticId = new AdtReference()
            {
                Key1 = new AdtKey() { Type = "GlobalReference", Value = "TestSemanticId" },
                Key2 = new AdtKey() { Type = "GlobalReference", Value = "A second Key" }
            };
            var actual = _objectUnderTest.GetSemanticId(adtSemanticId);
            actual.Keys.Count.Should().Be(1);
        }

        [TestMethod]
        public void GetSemanticId_returns_null_if_provided_with_null()
        {
            var actual = _objectUnderTest.GetSemanticId(null);
            actual.Should().BeNull();
        }

        [TestMethod]
        public void GetSemanticId_returns_null_if_provided_with_AdtReference_without_Keys()
        {
            var adtReferenceWithoutKeys = new AdtReference();
            var actual = _objectUnderTest.GetSemanticId(adtReferenceWithoutKeys);
            actual.Should().BeNull();
        }

        [TestMethod]
        public void GetSemanticIdForTwin_returns_correct_Reference_when_provided()
        {
            var actual = _objectUnderTest.GetSemanticIdForTwin("TestId", _definitionsAndSemanticsWithSemanticId);

            var expected = new Reference(ReferenceTypes.GlobalReference,
                new List<Key>() { new Key(KeyTypes.GlobalReference, "TestSemanticId") });
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetSemanticIdForTwin_null_when_no_AdtSemanticId_exists()
        {
            var actual = _objectUnderTest.GetSemanticIdForTwin("TestId", new DefinitionsAndSemantics());
            actual.Should().BeNull();
        }

        [TestMethod]
        public void GetSupplementalSemanticIdsForTwin_returns_List_of_SupplementalSemanticIds()
        {
            var actual = _objectUnderTest.GetSupplementalSemanticIdsForTwin("TestId",
                _definitionsAndSemanticsWithSupplementalSemanticIds);
            var expected = new List<Reference>
            {
                new Reference(ReferenceTypes.GlobalReference, new List<Key>()
                {
                    new Key(KeyTypes.GlobalReference, "SupplementalId1")

                }),
                new Reference(ReferenceTypes.GlobalReference, new List<Key>()
                {
                    new Key(KeyTypes.GlobalReference, "SupplementalId2")
                })
            };
            actual.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
        }

        [TestMethod]
        public void GetEmbeddedDataSpecificationsForTwin_returns_List_of_EmbeddedDataSpecifications()
        {
            var actual = _objectUnderTest.GetEmbeddedDataSpecificationsForTwin("TestId",_definitionsAndSemanticsWithEmbeddedDataSpec);
            var expected = new List<EmbeddedDataSpecification>
            {
                new EmbeddedDataSpecification(
                    new Reference(ReferenceTypes.GlobalReference,new List<Key>(){new Key(KeyTypes.GlobalReference, null)}),
                    new DataSpecificationIec61360(new List<LangString>()
                    {
                        new LangString("de", "TestLangString") 

                    }))
            };
            actual.Should().BeEquivalentTo(expected, options => options.RespectingRuntimeTypes());
        }

        [TestMethod]
        public void GetEmbeddedDataSpecifications_returns_null_when_there_are_no_relationships_for_that_twin()
        {
            var definitionsAndSemanticsWithoutRelationships = new DefinitionsAndSemantics();
            var actual = _objectUnderTest.GetEmbeddedDataSpecificationsForTwin("NotExistingTwinId",definitionsAndSemanticsWithoutRelationships);
            actual.Should().BeNull();
        }
    }
}