using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace AAS.API.Repository.Adt.Tests
{
    

    [TestClass]
    public class AdtSubmodelModelFactoryTests
    {
        private Mock<IAdtDefinitionsAndSemanticsModelFactory> _adtDefinitionsAndSemantics;
        private Submodel _submodelFromAdtSubmodel;

        AdtSubmodelModelFactory objectUnderTest { get; set; }
        private AdtSubmodelAndSmcInformation<AdtSubmodel> information { get; set; }
        private AdtSubmodel _adtSubmodel { get; set; }

        
        [TestInitialize]
        public void Setup()
        {
            var _autoMocker = new AutoMocker();
            _adtDefinitionsAndSemantics = _autoMocker.GetMock<IAdtDefinitionsAndSemanticsModelFactory >();
            var submodelElementFactoryMock = _autoMocker.GetMock<AdtSubmodelElementFactory>();
            objectUnderTest = new AdtSubmodelModelFactory(_adtDefinitionsAndSemantics.Object,submodelElementFactoryMock.Object);
            _adtSubmodel = new AdtSubmodel
            {
                dtId = "TestDtID",
                Category = "TestCategory",
                Description = new AdtLanguageString { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDescription" } },
                DisplayName = new AdtLanguageString { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDisplayName" } },
                Checksum = "1234",
                IdShort = "TestIdShort",
                Id = "TestId",
                Administration = new AdtAdministration { Revision = "1", Version = "2", },
                Kind = new AdtHasKind { Kind = "Instance" }
            };
            _submodelFromAdtSubmodel = new Submodel(
                "TestId", null, "TestCategory", "TestIdShort", 
                new List<LangString>() { new LangString("en", "TestDisplayName") }, 
                new List<LangString>() { new LangString("en", "TestDescription") }, 
                "1234", new AdministrativeInformation(null, "2", "1"), ModelingKind.Instance, 
                null, null, null, null, new List<ISubmodelElement>());
        }


        [TestMethod]
        public async Task GetSubmodel_returns_Submodel_with_AdtSubmodel_Properties()
        {
            information = new()
            {
                GeneralAasInformation = new AdtGeneralAasInformation<AdtSubmodel>(){ RootElement = _adtSubmodel}
            };
            var actual = await objectUnderTest.GetSubmodel(information);
            var expected = _submodelFromAdtSubmodel;
            actual.Should().BeEquivalentTo(expected);
        }

    }
}

