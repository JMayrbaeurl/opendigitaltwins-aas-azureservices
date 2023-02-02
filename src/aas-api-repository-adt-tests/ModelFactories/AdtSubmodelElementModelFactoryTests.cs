using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.API.Repository.Adt.Tests
{
    [TestClass]
    public class AdtSubmodelElementModelFactoryTests
    {
        private Mock<IAdtDefinitionsAndSemanticsModelFactory> _adtDefinitionsAndSemantics;
        private Mock<IMapper> _mapperMock;
        private Mock<ILogger<AdtSubmodelElementFactory>> _loggerMock;

        AdtSubmodelElementFactory _objectUnderTest { get; set; }
        private AdtSubmodelAndSmcInformation<AdtSubmodel> _informationSubmodel { get; set; }
        private List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> _informationSmeCollectionsWithoutSmes;
        private List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>> _informationSmeCollectionsWithSmes;


        private List<AdtProperty> adtProperties { get; set; }


        [TestInitialize]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();

            _mapperMock.Setup(_ => _.Map<Property>(It.IsAny<AdtSubmodelElement>()))
                .Returns(new Property(DataTypeDefXsd.Boolean));

            _mapperMock.Setup(_ => _.Map<File>(It.IsAny<AdtSubmodelElement>()))
                .Returns(new File("testContentType"));

            _mapperMock.Setup(_ => _.Map<SubmodelElementCollection>(It.IsAny<AdtSubmodelElement>()))
                .Returns(new SubmodelElementCollection());



            _loggerMock = new Mock<ILogger<AdtSubmodelElementFactory>>();

            _adtDefinitionsAndSemantics = new Mock<IAdtDefinitionsAndSemanticsModelFactory>();
            _objectUnderTest = new AdtSubmodelElementFactory(_adtDefinitionsAndSemantics.Object, _mapperMock.Object, _loggerMock.Object);
            _informationSubmodel = new AdtSubmodelAndSmcInformation<AdtSubmodel>();

            _informationSmeCollectionsWithoutSmes = new List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>>
                {
                    new AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>
                    {

                            RootElement = new AdtSubmodelElementCollection(),
                            DefinitionsAndSemantics = new DefinitionsAndSemantics(),

                        AdtSubmodelElements = new AdtSubmodelElements()
                    }
                };
            _informationSmeCollectionsWithSmes = new List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>>
            {
                new AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>
                {

                        RootElement = new AdtSubmodelElementCollection(),
                        DefinitionsAndSemantics = new DefinitionsAndSemantics(),

                    AdtSubmodelElements = new AdtSubmodelElements
                    {
                        smeCollections = new List<AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>>(),
                        properties = new List<AdtProperty>(){new AdtProperty()},
                        files = new List<AdtFile>(){new AdtFile()}
                    }
                }
            };
            adtProperties = new List<AdtProperty>()
            {
                new AdtProperty
                {
                    dtId = "TestDtId1",
                    Value = "TestValue",

                },
                new AdtProperty
                {
                    dtId = "TestDtId2",
                    Value = "TestValue2",
                }
            };
        }

        [TestMethod]
        public void GetSubmodelElements_creates_property()
        {
            _informationSubmodel.AdtSubmodelElements.properties = adtProperties;

            var actualList = _objectUnderTest.GetSubmodelElements(
                _informationSubmodel.AdtSubmodelElements, _informationSubmodel.DefinitionsAndSemantics);

            actualList.Should().HaveCount(2);
            _mapperMock.Verify(_ => _.Map<Property>(It.IsAny<AdtProperty>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetEmbeddedDataSpecificationsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSupplementalSemanticIdsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSemanticIdForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetSubmodelElements_creates_files()
        {
            _informationSubmodel.AdtSubmodelElements.files = new List<AdtFile>() { new AdtFile(), new AdtFile() };

            var actualList = _objectUnderTest.GetSubmodelElements(
                _informationSubmodel.AdtSubmodelElements,
                _informationSubmodel.DefinitionsAndSemantics);

            actualList.Should().HaveCount(2);
            _mapperMock.Verify(_ => _.Map<File>(It.IsAny<AdtFile>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetEmbeddedDataSpecificationsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSupplementalSemanticIdsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSemanticIdForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetSubmodelElements_creates_SubmodelElementCollection()
        {
            _informationSubmodel.AdtSubmodelElements.smeCollections = _informationSmeCollectionsWithoutSmes;

            var actualList = _objectUnderTest.GetSubmodelElements(
                _informationSubmodel.AdtSubmodelElements,
                _informationSubmodel.DefinitionsAndSemantics);

            actualList.Should().HaveCount(1);
            _mapperMock.Verify(_ => _.Map<SubmodelElementCollection>(It.IsAny<AdtSubmodelElementCollection>()), Times.Once);
            _adtDefinitionsAndSemantics.Verify(_ => _.GetEmbeddedDataSpecificationsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Once);
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSupplementalSemanticIdsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Once);
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSemanticIdForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Once);
        }

        [TestMethod]
        public void GetSubmodelElements_creates_SubmodelElementCollection_with_SubmodelElements()
        {
            _informationSubmodel.AdtSubmodelElements.smeCollections = _informationSmeCollectionsWithSmes;

            var actualList = _objectUnderTest.GetSubmodelElements(
                _informationSubmodel.AdtSubmodelElements,
                _informationSubmodel.DefinitionsAndSemantics);

            actualList.Should().HaveCount(1);
            ((SubmodelElementCollection)actualList[0]).Value.Should().HaveCount(2);
            _mapperMock.Verify(_ => _.Map<SubmodelElementCollection>(It.IsAny<AdtSubmodelElementCollection>()),
                Times.Once);
            _adtDefinitionsAndSemantics.Verify(_ => _.GetEmbeddedDataSpecificationsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(3));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSupplementalSemanticIdsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(3));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSemanticIdForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(3));
        }
    }
}
