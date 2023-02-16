using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Moq;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.ADT.Tests
{
    [TestClass]
    public class AasWriteSubmodelElementsTests
    {
        private AasWriteSubmodelElements _objectUnderTest;
        private Mock<ILogger<AasWriteSubmodelElements>> _loggerMock;
        private Mock<IAdtTwinFactory> _adtTwinFactoryMock;
        private Mock<IAasWriteConnector> _writeConnectorMock;
        private Mock<IAasWriteBase> _writeBaseMock;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<AasWriteSubmodelElements>>();
            _writeConnectorMock = new Mock<IAasWriteConnector>();
            _adtTwinFactoryMock = new Mock<IAdtTwinFactory>();
            _writeBaseMock = new Mock<IAasWriteBase>();

            _writeConnectorMock.Setup(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()))
                .ReturnsAsync("BasicDigitalTwinId");

            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<ISubmodelElement>())).Returns(
                new BasicDigitalTwin
                {
                    Id = "testSubmodelElementTwinId"
                });

            _objectUnderTest = new AasWriteSubmodelElements(_loggerMock.Object, _adtTwinFactoryMock.Object,
                _writeConnectorMock.Object, _writeBaseMock.Object);
        }

        [TestMethod]
        public async Task CreateSubmodelElement_Creates_Property()
        {
            var minimalProperty = new Property(DataTypeDefXsd.Boolean);
            await _objectUnderTest.CreateSubmodelElement(minimalProperty);

            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<Property>()), Times.Once());
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Once());
        }

        [TestMethod]
        public async Task CreateSubmodelElement_adds_SmeRelationships_for_full_Property()
        {
            var fullProperty = new Property(DataTypeDefXsd.Boolean,
                qualifiers: new List<Qualifier>() { new Qualifier("testType", DataTypeDefXsd.Boolean) },
                semanticId: new Reference(ReferenceTypes.GlobalReference, new List<Key>()),
                embeddedDataSpecifications: new List<EmbeddedDataSpecification>()
                {
                    new EmbeddedDataSpecification(
                        new Reference(ReferenceTypes.GlobalReference,
                            new List<Key>()),new DataSpecificationIec61360(new List<LangString>()))
                });


            await _objectUnderTest.CreateSubmodelElement(fullProperty);

            _writeBaseMock.Verify(_ => _.AddHasDataSpecification(It.IsAny<string>(), It.IsAny<List<EmbeddedDataSpecification>>())
            , Times.Once);
            _writeBaseMock.Verify(_ => _.AddQualifiableRelations(It.IsAny<string>(), It.IsAny<List<Qualifier>>()),
            Times.Once);
            _writeBaseMock.Verify(_ => _.AddReference(It.IsAny<string>(), It.IsAny<Reference>(), "semanticId"),
            Times.Once);
        }

        [TestMethod]
        public async Task CreateSubmodelElement_adds_valueId_if_present()
        {
            var propertyWithValueId = new Property(DataTypeDefXsd.Boolean,
                valueId: new Reference(ReferenceTypes.GlobalReference, new List<Key>()));

            await _objectUnderTest.CreateSubmodelElement(propertyWithValueId);
            _writeBaseMock.Verify(_ => _.AddReference(It.IsAny<string>(), It.IsAny<Reference>(), "valueId"), Times.Once);
        }

        [TestMethod]
        public async Task CreateSubmodelElement_Creates_SubmodelElementCollection()
        {
            var minimalSubmodelElementCollection = new SubmodelElementCollection();
            await _objectUnderTest.CreateSubmodelElement(minimalSubmodelElementCollection);

            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Once());
        }

        [TestMethod]
        public async Task CreateSubmodelElement_adds_SmeRelationships_for_full_SubmodelElementCollection()
        {
            var fullSmeCollection = new SubmodelElementCollection(
                qualifiers: new List<Qualifier>() { new Qualifier("testType", DataTypeDefXsd.Boolean) },
                semanticId: new Reference(ReferenceTypes.GlobalReference, new List<Key>()),
                embeddedDataSpecifications: new List<EmbeddedDataSpecification>()
                {
                    new EmbeddedDataSpecification(
                        new Reference(ReferenceTypes.GlobalReference,
                            new List<Key>()), new DataSpecificationIec61360(new List<LangString>()))
                });

            await _objectUnderTest.CreateSubmodelElement(fullSmeCollection);

            _writeBaseMock.Verify(_ => _.AddHasDataSpecification(It.IsAny<string>(), It.IsAny<List<EmbeddedDataSpecification>>())
                , Times.Once);
            _writeBaseMock.Verify(_ => _.AddQualifiableRelations(It.IsAny<string>(), It.IsAny<List<Qualifier>>()),
                Times.Once);
            _writeBaseMock.Verify(_ => _.AddReference(It.IsAny<string>(), It.IsAny<Reference>(), "semanticId"),
                Times.Once);
        }

        [TestMethod]
        public async Task CreateSubmodelElement_creates_two_twins_for_two_contained_SubmodelElements()
        {
            var SmeCollectionWithSubmodelElements = new SubmodelElementCollection(
                value: new List<ISubmodelElement>()
                    { new Property(DataTypeDefXsd.Boolean), new SubmodelElementCollection() });
            await _objectUnderTest.CreateSubmodelElement(SmeCollectionWithSubmodelElements);

            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Exactly(3));
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync("BasicDigitalTwinId", "value", It.IsAny<string>()));

        }

        [TestMethod]
        public async Task CreateSubmodelElement_Creates_File()
        {
            var minimalFile = new File("testContentType");
            await _objectUnderTest.CreateSubmodelElement(minimalFile);

            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Once());
        }

        [TestMethod]
        public async Task CreateSubmodelElement_adds_SmeRelationships_for_full_File()
        {
            var fullFile = new File("testContentType",
            qualifiers:
            new List<Qualifier>() { new Qualifier("testType", DataTypeDefXsd.Boolean) },
            semanticId:
            new Reference(ReferenceTypes.GlobalReference, new List<Key>()),
            embeddedDataSpecifications:
            new List<EmbeddedDataSpecification>()
            {
                new EmbeddedDataSpecification(
                    new Reference(ReferenceTypes.GlobalReference,
                        new List<Key>()), new DataSpecificationIec61360(new List<LangString>()))
            });
            
            await _objectUnderTest.CreateSubmodelElement(fullFile);

            _writeBaseMock.Verify(_ => _.AddHasDataSpecification(It.IsAny<string>(), It.IsAny<List<EmbeddedDataSpecification>>())
                , Times.Once);
            _writeBaseMock.Verify(_ => _.AddQualifiableRelations(It.IsAny<string>(), It.IsAny<List<Qualifier>>()),
                Times.Once);
            _writeBaseMock.Verify(_ => _.AddReference(It.IsAny<string>(), It.IsAny<Reference>(), "semanticId"), Times.Once);
        }



    }
}
