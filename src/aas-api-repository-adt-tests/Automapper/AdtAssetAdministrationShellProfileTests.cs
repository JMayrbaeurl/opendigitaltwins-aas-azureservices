using AAS.API.Repository.Adt.AutoMapper;
using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using FluentAssertions;

namespace AAS.API.Repository.Adt.Tests
{
    [TestClass]
    public class AdtAssetAdministrationShellProfileTests
    {
        private IMapper? _objectUnderTest;
        private AdtAas _fullAdtAas;
        private AdtAas _minimalAdtAas;
        private AssetAdministrationShell _minimalAas;
        private AssetAdministrationShell _fullAas;

        [TestInitialize]
        public void Setup()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AdtAssetAdministrationShellProfile());
                cfg.AddProfile(new AdtIdentifiableProfile());
                cfg.AddProfile(new AdtReferableProfile());

                cfg.DisableConstructorMapping();
                cfg.AllowNullCollections = true;
            });
            configuration.AssertConfigurationIsValid();
            _objectUnderTest = configuration.CreateMapper();
            _fullAdtAas = new AdtAas
            {
                dtId = "TestDtId",
                Category = "TestCategory",
                Description = new AdtLanguageString
                { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDescription" } },
                DisplayName = new AdtLanguageString
                { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDisplayName" } },
                Checksum = "1234",
                IdShort = "TestIdShort",
                Id = "TestId",
                Administration = new AdtAdministration
                {
                    Revision = "1",
                    Version = "2",
                    Metadata = null
                },
                AssetInformation = new AdtAssetInformationShort
                {
                    AssetKind = "Instance",
                    GlobalAssetId = "TestAssetId",
                    Metadata = null
                }
            };

            _fullAas = new AssetAdministrationShell(
                "TestId",
                new AssetInformation(
                    AssetKind.Instance,
                    new Reference(ReferenceTypes.GlobalReference,new List<Key>(){new Key(KeyTypes.GlobalReference,"TestAssetId")})),
                null,
                "TestCategory",
                "TestIdShort",
                new List<LangString>() { new LangString("en", "TestDisplayName") },
                new List<LangString>() { new LangString("en", "TestDescription") },
                "1234",
                new AdministrativeInformation(new List<EmbeddedDataSpecification>(), "2", "1"),
                null,
                null,
                null);

        }
        
        [TestMethod]
        public void Map_constructs_new_general_Aas_from_AdtAas()
        {
            var aasInformations = new List<AdtResponseForAllAasInformation>();

            var actual = _objectUnderTest.Map<AssetAdministrationShell>(_fullAdtAas);
            
            actual.Should().BeEquivalentTo(_fullAas);
        }


        [TestMethod]
        public void Map_constructs_new_AssetInformation_from_AdtAssetInformationShort_Type_Type()
        {
            var adtAssetInformation = new AdtAssetInformationShort
            {
                AssetKind =  "Type"
            };
            var expected = new AssetInformation(AssetKind.Type);
            var actual = _objectUnderTest.Map<AssetInformation>(adtAssetInformation);
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Map_constructs_new_AssetInformation_from_AdtAssetInformationShort_Type_Instance()
        {
            var adtAssetInformation = new AdtAssetInformationShort
            {
                AssetKind =  "Instance"
            };
            var expected = new AssetInformation(AssetKind.Instance);
            var actual = _objectUnderTest.Map<AssetInformation>(adtAssetInformation);
            actual.Should().BeEquivalentTo(expected);
        }


    }
}
