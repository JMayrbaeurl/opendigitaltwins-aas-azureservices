using AAS_Services_Support.ADT_Support;
using AdtModels.AdtModels;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS_Services_Support_Tests
{
    [TestClass]
    public class AdtSubmodelElementModelFactoryTests
    {
        private Mock<IAdtDefinitionsAndSemanticsModelFactory> _adtDefinitionsAndSemantics;
        private List<ISubmodelElement> properties;
        private Mock<IMapper> _mapperMock;

        AdtSubmodelElementFactory<AdtSubmodel> objectUnderTestSubmodel { get; set; }
        AdtSubmodelElementFactory<AdtSubmodelElementCollection> objectUnderTestSmeCollection { get; set; }
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


            _adtDefinitionsAndSemantics = new Mock<IAdtDefinitionsAndSemanticsModelFactory>();
            objectUnderTestSubmodel = new AdtSubmodelElementFactory<AdtSubmodel>(_adtDefinitionsAndSemantics.Object, _mapperMock.Object);
            objectUnderTestSmeCollection = new AdtSubmodelElementFactory<AdtSubmodelElementCollection>(_adtDefinitionsAndSemantics.Object, _mapperMock.Object);
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
                new Property(DataTypeDefXsd.Boolean,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "TestValue",
                    null),
                new Property(DataTypeDefXsd.Boolean,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "TestValue2",
                    null)
            };

        }

        [TestMethod]
        public void Configure_sets_adtInformation_and_configures_adtDefinitionsAndSemanticsFactory()
        {
            objectUnderTestSmeCollection.Configure(new AdtSubmodelAndSmcInformation<AdtSubmodelElementCollection>());
            objectUnderTestSubmodel.Configure(new AdtSubmodelAndSmcInformation<AdtSubmodel>());

            _adtDefinitionsAndSemantics.Verify(_ => _.Configure(It.IsAny<DefinitionsAndSemantics>()), Times.AtLeastOnce);
            
        }

        [TestMethod]
        public void GetSubmodelElementsFromAdtSubmodelAndSMCInformation_adds_property_specific_Information()
        {
            informationSubmodel.properties = adtProperties;
            objectUnderTestSubmodel.Configure(informationSubmodel);
            var actual = objectUnderTestSubmodel.GetSubmodelElementsFromAdtSubmodelAndSMCInformation();
            var expected = properties;
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetSubmodelElementsFromAdtSubmodelAndSMCInformation_returns_properties_for_AdtSmeCollection()
        {
            informationSmeCollection.properties = adtProperties;

            objectUnderTestSmeCollection.Configure(informationSmeCollection);
            var actual = objectUnderTestSmeCollection.GetSubmodelElementsFromAdtSubmodelAndSMCInformation();
            var expected = properties;
            actual.Should().BeEquivalentTo(expected);
        }

    }
}
