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
        private List<ISubmodelElement> properties;
        private Mock<IMapper> _mapperMock;
        private Mock<ILogger<AdtSubmodelElementFactory>> _loggerMock;

        AdtSubmodelElementFactory objectUnderTestSubmodel { get; set; }
        AdtSubmodelElementFactory objectUnderTestSmeCollection { get; set; }
        private AdtSubmodelAndSmcInformation<AdtSubmodel> informationSubmodel { get; set; }
        private AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection> informationSmeCollection { get; set; }

        private List<AdtProperty> adtProperties { get; set; }


        [TestInitialize]
        public void Setup()
        {
            _mapperMock = new Mock<IMapper>();
            var boolProperty = new Property(DataTypeDefXsd.Boolean);
            _mapperMock.Setup(_ => _.Map<Property>(It.IsAny<AdtSubmodelElement>()))
                .Returns(boolProperty);
            var decimalProperty = new Property(DataTypeDefXsd.Decimal);
            _mapperMock.Setup(_ => _.Map(It.IsAny<AdtSubmodelElement>(), decimalProperty))
                .Returns(decimalProperty);
            _mapperMock.Setup(_ => _.Map(It.IsAny<AdtSubmodelElement>(), It.IsAny<File>()))
                .Returns(new File("TestContentType"));

            _loggerMock = new Mock<ILogger<AdtSubmodelElementFactory>>();

            _adtDefinitionsAndSemantics = new Mock<IAdtDefinitionsAndSemanticsModelFactory>();
            objectUnderTestSubmodel = new AdtSubmodelElementFactory(_adtDefinitionsAndSemantics.Object, _mapperMock.Object, _loggerMock.Object);
            objectUnderTestSmeCollection = new AdtSubmodelElementFactory(_adtDefinitionsAndSemantics.Object, _mapperMock.Object, _loggerMock.Object);
            informationSubmodel = new AdtSubmodelAndSmcInformation<AdtSubmodel>();
            informationSmeCollection = new AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>();
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
            properties = new List<ISubmodelElement>()
            {
                new Property(DataTypeDefXsd.Boolean,value:"TestValue2"),
                new Property(DataTypeDefXsd.Boolean,value:"TestValue2")

            };

        }

        [TestMethod]
        public void GetSubmodelElements_creates_property()
        {
            informationSubmodel.AdtSubmodelElements.properties = adtProperties;
            
            var actual = objectUnderTestSubmodel.GetSubmodelElements(
                informationSubmodel.AdtSubmodelElements, informationSubmodel.GeneralAasInformation.definitionsAndSemantics);
            
            _mapperMock.Verify(_ => _.Map<Property>(It.IsAny<AdtProperty>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetEmbeddedDataSpecificationsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSupplementalSemanticIdsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Exactly(2));
        }



    }
}
