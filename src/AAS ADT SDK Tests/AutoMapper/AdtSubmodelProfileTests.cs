using AAS.ADT.AutoMapper;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using FluentAssertions;

namespace AAS.ADT.Tests.AutoMapper
{
    [TestClass]
    public class AdtSubmodelProfileTests
    {
        private IMapper? _objectUnderTest;
        private AdtSubmodel _fullAdtSubmodel;
        private AdtSubmodel _minimalAdtSubmodel;
        private Submodel _minimalSubmodel;
        private Submodel _fullSubmodel;

        [TestInitialize]
        public void Setup()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AdtSubmodelProfile());
                cfg.AddProfile(new AdtIdentifiableProfile());
                cfg.AddProfile(new AdtReferableProfile());

                cfg.DisableConstructorMapping();
                cfg.AllowNullCollections = true;
            });
            configuration.AssertConfigurationIsValid();
            _objectUnderTest = configuration.CreateMapper();

            _minimalAdtSubmodel = new AdtSubmodel
            {
                Id = "testSubmodelId"
            };
            _minimalSubmodel = new Submodel("testSubmodelId");

            _fullAdtSubmodel = new AdtSubmodel
            {
                Category = "TestCategory",
                Description = new AdtLanguageString
                    { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDescription" } },
                DisplayName = new AdtLanguageString
                    { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDisplayName" } },
                Checksum = "1234",
                IdShort = "TestIdShort",
                Id = "TestSubmodelId",
                Administration = new AdtAdministration
                {
                    Revision = "1",
                    Version = "2",
                    Metadata = null
                },
                Kind = new AdtHasKind { Kind = "Instance" }
            };
            _fullSubmodel = new Submodel("TestSubmodelId",
                null,
                "TestCategory",
                "TestIdShort",
                new List<LangString>() { new LangString("en", "TestDisplayName") },
                new List<LangString>() { new LangString("en", "TestDescription") },
                "1234",
                new AdministrativeInformation(new List<EmbeddedDataSpecification>(), "2", "1"),
                ModelingKind.Instance);
        }

        [TestMethod]
        public void Map_returns_Submodel_for_minimal_AdtSubmodel()
        {
            var actualSubmodel = _objectUnderTest.Map<Submodel>(_minimalAdtSubmodel);
            actualSubmodel.Should().BeEquivalentTo(_minimalSubmodel);
        }

        [TestMethod]
        public void Map_returns_Submodel_for_full_AdtSubmodel()
        {
            var actualSubmodel = _objectUnderTest.Map<Submodel>(_fullAdtSubmodel);
            actualSubmodel.Should().BeEquivalentTo(_fullSubmodel);
        }

    }
}
