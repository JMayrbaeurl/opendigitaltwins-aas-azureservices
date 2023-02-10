using AAS.API.Repository.Adt.Models;
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


        [TestInitialize]
        public void Setup()
        {
            _autoMocker = new AutoMocker();
            _mapper = _autoMocker.GetMock<IMapper>();
            _objectUnderTest = _autoMocker.CreateInstance<AdtDefinitionsAndSemanticsModelFactory>();
            _mapper.Setup(_ => _.Map<DataSpecificationIec61360>(It.IsAny<AdtDataSpecificationIEC61360>())).Returns(
                new DataSpecificationIec61360(new List<LangString>() { new LangString("de", "TestLangString") }));
        }

        [TestMethod]
        public void GetSupplementalSemanticIdsForTwin_returns_List_of_SupplementalSemanticIds()
        {
            var definitionsAndSemantics = new DefinitionsAndSemantics
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
                Iec61360s = null,
                ConceptDescriptions = null,
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
            var actual = _objectUnderTest.GetSupplementalSemanticIdsForTwin("TestId", definitionsAndSemantics);
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
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetEmbeddedDataSpecificationsForTwin_returns_List_of_EmbeddedDataSpecifications()
        {
            var definitionsAndSemantics = new DefinitionsAndSemantics
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
                        PreferredName = new AdtLanguageString(){LangStrings = new Dictionary<string, string>() { ["en"]="TestPreferredName"}}
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
            
            
            var actual = _objectUnderTest.GetEmbeddedDataSpecificationsForTwin("TestId",definitionsAndSemantics);
            var expected = new List<EmbeddedDataSpecification>
            {
                new EmbeddedDataSpecification(

                    new Reference(ReferenceTypes.GlobalReference,new List<Key>(){new Key(KeyTypes.GlobalReference, null)}),
                    new DataSpecificationIec61360(new List<LangString>()
                    {
                        new LangString("de", "TestLangString") 

                    }))
            };
            actual.Should().BeEquivalentTo(expected);

        }

        [TestMethod]
        public void GetEmbeddedDataSpecifications_returns_null_when_there_are_no_relationships_for_that_twin()
        {
            var definitionsAndSemantics = new DefinitionsAndSemantics();
            var actual = _objectUnderTest.GetEmbeddedDataSpecificationsForTwin("NotExistingTwinId",definitionsAndSemantics);
            actual.Should().BeNull();
        }
    }
}