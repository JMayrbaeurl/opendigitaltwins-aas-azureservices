using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;
using Moq;

namespace AAS.ADT.Tests.Connectors
{
    [TestClass]
    public class AasWriteBaseTests
    {
        private AasWriteBase _objectUnderTest;
        private Mock<ILogger<AasWriteBase>> _loggerMock;
        private Mock<IAdtTwinFactory> _adtTwinFactoryMock;
        private Mock<IAasWriteConnector> _writeConnectorMock;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<AasWriteBase>>();
            _adtTwinFactoryMock = new Mock<IAdtTwinFactory>();
            _writeConnectorMock = new Mock<IAasWriteConnector>();

            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<IDataSpecificationContent>())).Returns(
                new BasicDigitalTwin
                {
                    Id = "testDataSpecContentTwinId"
                });

            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<Reference>())).Returns(new BasicDigitalTwin
            {
                Id = "testRefTwinId"
            });

            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<Qualifier>())).Returns(
                new BasicDigitalTwin
                {
                    Id = "testQualifierTwinId"
                });

            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<EmbeddedDataSpecification>())).Returns(
                new BasicDigitalTwin
                {
                    Id = "testDataSpecificationTwinId"
                });

            _objectUnderTest =
                new AasWriteBase(_loggerMock.Object, _adtTwinFactoryMock.Object, _writeConnectorMock.Object);
        }

        [TestMethod]
        public async Task AddReference_does_nothing_when_reference_is_null()
        {

            await _objectUnderTest.AddReference("testSourceTwinId", null, "testRelationshipName");

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Reference>()), Times.Never);
        }

        [TestMethod]
        public async Task AddReference_creates_reference_when_keys_present()
        {
            var exemplaryReference = new Reference(ReferenceTypes.GlobalReference, new List<Key>()
            {
                new Key(KeyTypes.AnnotatedRelationshipElement,"testValue")
            });
            await _objectUnderTest.AddReference("testSourceTwinId", exemplaryReference, "testRelationshipName");

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Reference>()), Times.Once);
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()), Times.Once());
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(
                    "testSourceTwinId", "testRelationshipName", "testRefTwinId"), Times.Once());
        }

        [TestMethod]
        public async Task AddReference_does_nothing_when_No_keys_present()
        {
            var exemplaryReferenceWithoutKeys = new Reference(ReferenceTypes.GlobalReference, new List<Key>());

            await _objectUnderTest.AddReference("testSourceTwinId", exemplaryReferenceWithoutKeys, "testRelationshipName");

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Reference>()), Times.Never);
        }

        [TestMethod]
        public async Task AddHasDataSpecification_does_nothing_if_embeddedDataSpecification_is_null()
        {
            await _objectUnderTest.AddHasDataSpecification("testSourceTwinId", null);

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Property>()), Times.Never);
        }

        [TestMethod]
        public async Task AddHasDataSpecification_does_nothing_if_no_embeddedDataSpecification_present()
        {
            var exemplaryInputWithoutEmbeddedDataSpec = new Property(DataTypeDefXsd.Boolean);
            await _objectUnderTest.AddHasDataSpecification("testSourceTwinId", exemplaryInputWithoutEmbeddedDataSpec.EmbeddedDataSpecifications);

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Property>()), Times.Never);
        }

        [TestMethod]
        public async Task AddHasDataSpecification_creates_Two_twins_if_two_embeddedDataSpecs_present()
        {
            var exemplaryInputWithTwoEmbeddedDataSpecs = new Property(DataTypeDefXsd.Boolean,
                embeddedDataSpecifications: new List<EmbeddedDataSpecification>()
                {
                    new EmbeddedDataSpecification(new Reference(ReferenceTypes.GlobalReference, new List<Key>()),
                        new DataSpecificationIec61360(new List<LangString>())),
                    new EmbeddedDataSpecification(new Reference(ReferenceTypes.GlobalReference, new List<Key>()),
                        new DataSpecificationIec61360(new List<LangString>()))

                });

            await _objectUnderTest.AddHasDataSpecification("testSourceTwinId", exemplaryInputWithTwoEmbeddedDataSpecs.EmbeddedDataSpecifications);

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<IDataSpecificationContent>()), Times.Exactly(2));
            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<EmbeddedDataSpecification>()), Times.Exactly(2));
            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Reference>()), Times.Never);
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Exactly(4));
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(
                "testSourceTwinId", "dataSpecificationRef", "testDataSpecificationTwinId"), Times.Exactly(2));
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(
                "testDataSpecificationTwinId", "hasContent", "testDataSpecContentTwinId"), Times.Exactly(2));
        }

        [TestMethod]
        public async Task AddHasDataSpecification_creates_UnitId_if_present()
        {
            var exemplaryInputWithUnitId = new List<EmbeddedDataSpecification>()
                {
                    new EmbeddedDataSpecification(new Reference(ReferenceTypes.GlobalReference, new List<Key>()),
                        new DataSpecificationIec61360(new List<LangString>(),
                            unitId: new Reference(ReferenceTypes.GlobalReference,
                                new List<Key>(){new Key(KeyTypes.Blob,"testValue")})))

                };
            await _objectUnderTest.AddHasDataSpecification("testSourceTwinId", exemplaryInputWithUnitId);

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Reference>()), Times.Once);
        }

        [TestMethod]
        public async Task AddQualifiableRelations_does_nothing_if_qualifiers_is_null()
        {
            await _objectUnderTest.AddQualifiableRelations("testSourceTwinId", null);

            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AddQualifiableRelations_does_nothing_if_no_qualifiers_present()
        {
            var emptyQualifiers = new List<Qualifier>();

            await _objectUnderTest.AddQualifiableRelations("testSourceTwinId", emptyQualifiers);

            _writeConnectorMock.Verify(_=>_.DoCreateOrReplaceRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()),Times.Never);
        }

        [TestMethod]
        public async Task AddQualifiableRelations_creates_two_twins_if_two_qualifiers_present()
        {
            var qualifierListWithTwoQualifiers = new List<Qualifier>()
            {
                new Qualifier("testType", DataTypeDefXsd.Boolean),
                new Qualifier("testType", DataTypeDefXsd.String)
            };

            await _objectUnderTest.AddQualifiableRelations("testSourceTwinId", qualifierListWithTwoQualifiers);

            _writeConnectorMock.Verify(_=>_.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()));
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync("testSourceTwinId", "qualifier",
                "testQualifierTwinId"), Times.Exactly(2));
        }

        [TestMethod]
        public async Task AddQualifiableRelations_creates_semanticId_if_present()
        {
            var qualifierListWithSemanticId = new List<Qualifier>()
            {
                new Qualifier("testType", DataTypeDefXsd.Boolean, new Reference(
                    ReferenceTypes.GlobalReference,new List<Key>(){new Key(KeyTypes.GlobalReference,"testSemanticId")}))
            };

            await _objectUnderTest.AddQualifiableRelations("testSourceTwinId", qualifierListWithSemanticId);

            _writeConnectorMock.Verify(
                _ => _.DoCreateOrReplaceRelationshipAsync("testQualifierTwinId", "semanticId", "testRefTwinId"), Times.Once);

            _writeConnectorMock.Verify(_=>_.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),Times.Exactly(2));
        }

        [TestMethod]
        public async Task AddQualifiableRelations_creates_valueId_if_present()
        {
            var qualifierListWithSemanticId = new List<Qualifier>()
            {
                new Qualifier("testType", DataTypeDefXsd.Boolean,valueId: new Reference(
                    ReferenceTypes.GlobalReference,
                    new List<Key>() { new Key(KeyTypes.GlobalReference, "testValueId") }))
            };

            await _objectUnderTest.AddQualifiableRelations("testSourceTwinId", qualifierListWithSemanticId);

            _writeConnectorMock.Verify(_=>_.DoCreateOrReplaceRelationshipAsync("testQualifierTwinId","valueId","testRefTwinId"),Times.Once);
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Exactly(2));
        }

    }
}
