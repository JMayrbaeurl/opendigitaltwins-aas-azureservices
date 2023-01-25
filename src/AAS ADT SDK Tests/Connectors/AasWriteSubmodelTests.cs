using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Moq;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.ADT.Tests.Connectors
{
    [TestClass]
    public class AasWriteSubmodelTests
    {
        private AasWriteSubmodel _objectUnderTest;
        private Mock<ILogger<AasWriteSubmodel>> _loggerMock;
        private Mock<IAdtTwinFactory> _adtTwinFactoryMock;
        private Mock<IAasWriteConnector> _writeConnectorMock;
        private Mock<IAasWriteBase> _writeBaseMock;
        private Mock<IAasWriteSubmodelElements> _writeSmeMock;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<AasWriteSubmodel>>();
            _writeConnectorMock = new Mock<IAasWriteConnector>();
            _adtTwinFactoryMock = new Mock<IAdtTwinFactory>();
            _writeBaseMock = new Mock<IAasWriteBase>();
            _writeSmeMock = new Mock<IAasWriteSubmodelElements>();

            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<Submodel>())).Returns(
                new BasicDigitalTwin
                {
                    Id = "testSubmodelTwinId"
                });

            _objectUnderTest = new AasWriteSubmodel(_loggerMock.Object, _adtTwinFactoryMock.Object, _writeSmeMock.Object,
                _writeConnectorMock.Object, _writeBaseMock.Object);
        }

        [TestMethod]
        public async Task CreateSubmodel_just_returns_if_given_submodel_is_Null()
        {
            await _objectUnderTest.CreateSubmodel(null);
        }

        [TestMethod]
        public async Task CreateSubmodel_calls_for_adding_Qualifiers_DataSpecification_and_SemanticId()
        {
            await _objectUnderTest.CreateSubmodel(new Submodel("testSubmodelId"));

            _writeBaseMock.Verify(_=>_.AddHasDataSpecification("testSubmodelTwinId",null),Times.Once);
            _writeBaseMock.Verify(_ => _.AddReference("testSubmodelTwinId",null,"semanticId"), Times.Once);
            _writeBaseMock.Verify(_ => _.AddQualifiableRelations("testSubmodelTwinId", null), Times.Once);
        }

        [TestMethod]
        public async Task CreateSubmodel_does_not_try_to_create_SubmodelElements_if_they_are_null()
        {
            var submodelWithSubmodelElementsAsNull = new Submodel("testSubmodelId");
            await _objectUnderTest.CreateSubmodel(submodelWithSubmodelElementsAsNull);
            
            _writeSmeMock.Verify(_ => _.CreateSubmodelElement(It.IsAny<ISubmodelElement>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateSubmodel_does_not_try_to_create_SubmodelElements_if_they_are_empty()
        {

            var submodelWithEmptyListOfSubmodelElements =
                new Submodel("testSubmodelId", submodelElements: new List<ISubmodelElement>());
            await _objectUnderTest.CreateSubmodel(submodelWithEmptyListOfSubmodelElements);
            
            _writeSmeMock.Verify(_ => _.CreateSubmodelElement(It.IsAny<ISubmodelElement>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateSubmodel_tries_to_create_SubmodelElements_when_present()
        {
            var submodelWithTwoSubmodelElements =
                new Submodel("testSubmodelId", submodelElements: new List<ISubmodelElement>()
                    {new Property(DataTypeDefXsd.Boolean),new File("testContentType")});
            await _objectUnderTest.CreateSubmodel(submodelWithTwoSubmodelElements);

            _writeSmeMock.Verify(_ => _.CreateSubmodelElement(It.IsAny<ISubmodelElement>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task CreateSubmodel_tries_to_create_Relationship_to_SubmodelElements_when_created()
        {
            var submodelWithTwoSubmodelElements =
                new Submodel("testSubmodelId", submodelElements: new List<ISubmodelElement>()
                    { new Property(DataTypeDefXsd.Boolean), new File("testContentType") });

            _writeSmeMock.Setup(_ => _.CreateSubmodelElement(It.IsAny<ISubmodelElement>()))
                .ReturnsAsync("testSubmodelElementTwinId");

            await _objectUnderTest.CreateSubmodel(submodelWithTwoSubmodelElements);

            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(
                "testSubmodelTwinId", "submodelElement", "testSubmodelElementTwinId"), Times.Exactly(2));
        }

        [TestMethod]
        public async Task CreateSubmodelElementForSubmodel_tries_to_create_SubmodelElements_when_present()
        {
            var exemplarySubmodelElement = new Property(DataTypeDefXsd.Boolean);
            await _objectUnderTest.CreateSubmodelElementForSubmodel(exemplarySubmodelElement,"testSubmodelTwinId");

            _writeSmeMock.Verify(_ => _.CreateSubmodelElement(It.IsAny<ISubmodelElement>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateSubmodelElementForSubmodel_tries_to_create_Relationship_to_SubmodelElements_when_created()
        {
            var exemplarySubmodelElement = new Property(DataTypeDefXsd.Boolean);
            _writeSmeMock.Setup(_ => _.CreateSubmodelElement(It.IsAny<ISubmodelElement>()))
                .ReturnsAsync("testSubmodelElementTwinId");

            await _objectUnderTest.CreateSubmodelElementForSubmodel(exemplarySubmodelElement, "testSubmodelTwinId");

            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(
                "testSubmodelTwinId", "submodelElement", "testSubmodelElementTwinId"), Times.Once);
        }

    }
}
