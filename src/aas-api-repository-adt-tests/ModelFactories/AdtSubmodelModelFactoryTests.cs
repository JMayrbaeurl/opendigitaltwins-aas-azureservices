using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace AAS.API.Repository.Adt.Tests
{
    

    [TestClass]
    public class AdtSubmodelModelFactoryTests
    {
        private Mock<IAdtDefinitionsAndSemanticsModelFactory> _adtDefinitionsAndSemantics;
        private Mock<IAdtSubmodelElementFactory> _submodelElementFactoryMock;
        private Mock<IMapper> _mapperMock;
        private Submodel _submodelFromAdtSubmodel;

        AdtSubmodelModelFactory objectUnderTest { get; set; }
        private AdtSubmodelAndSmcInformation<AdtSubmodel> information { get; set; }
        private AdtSubmodel _adtSubmodel { get; set; }

        
        [TestInitialize]
        public void Setup()
        {
            var _autoMocker = new AutoMocker();
            _adtDefinitionsAndSemantics = _autoMocker.GetMock<IAdtDefinitionsAndSemanticsModelFactory >();
            _submodelElementFactoryMock = _autoMocker.GetMock<IAdtSubmodelElementFactory>();
            _mapperMock = new Mock<IMapper>();
            _mapperMock.Setup(_ => _.Map<Submodel>(It.IsAny<AdtSubmodel>())).Returns(new Submodel("TestSubmodelId"));

            objectUnderTest = new AdtSubmodelModelFactory(
                _adtDefinitionsAndSemantics.Object,_submodelElementFactoryMock.Object,_mapperMock.Object);

            _adtSubmodel = new AdtSubmodel() { dtId = "TestTwinId" };
        }


        [TestMethod]
        public void GetSubmodel_returns_Submodel_with_AdtSubmodel_Properties()
        {
            information = new()
            {
                RootElement = _adtSubmodel
        };
            objectUnderTest.GetSubmodel(information);

            _mapperMock.Verify(_ => _.Map<Submodel>(It.IsAny<AdtSubmodel>()),
                Times.Once);
            
            _adtDefinitionsAndSemantics.Verify(_ => _.GetEmbeddedDataSpecificationsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Once);
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSupplementalSemanticIdsForTwin(
                It.IsAny<string>(), It.IsAny<DefinitionsAndSemantics>()), Times.Once);
            _adtDefinitionsAndSemantics.Verify(_ => _.GetSemanticIdForTwin("TestTwinId", It.IsAny<DefinitionsAndSemantics>()), Times.Once);

            _submodelElementFactoryMock.Verify(_=>_.GetSubmodelElements(
                It.IsAny<AdtSubmodelElements>(),It.IsAny<DefinitionsAndSemantics>()),Times.Once);
        }

    }
}

