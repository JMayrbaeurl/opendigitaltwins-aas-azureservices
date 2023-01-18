using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace AAS.API.Repository.Adt.Tests
{
    [TestClass]
    public class ADTAASModelFactoryTests
    {
        private ADTAASModelFactory objectUnderTest { get; set; }
        private AdtAas fullAdtAas { get; set; }
        private AssetAdministrationShell fullAas { get; set; }
        private AdtAas minimalAdtAas { get; set; }
        public AutoMocker _autoMocker { get; set; }
        private Mock<IMapper> _mapper;
        private AssetAdministrationShell minimalAas { get; set; }


        [TestInitialize]
        public void Setup()
        {
            _autoMocker = new AutoMocker();
            _mapper = new Mock<IMapper>();

            minimalAas = new AssetAdministrationShell("TestId", new AssetInformation(AssetKind.Type));
            minimalAdtAas = new AdtAas() { Id = "TestId" };
            _mapper.Setup(_ => _.Map<AssetAdministrationShell>(It.IsAny<AdtAas>()))
                .Returns(minimalAas);
            objectUnderTest = new ADTAASModelFactory(_mapper.Object);
        }

        [TestMethod]
        public void GetAas_returns_minimal_Aas_when_called_with_no_information()
        {
            var actual = objectUnderTest.GetAas(new AdtAssetAdministrationShellInformation());

            actual.Should().BeEquivalentTo(minimalAas);
        }

        [TestMethod]
        public void GetAas_returns_Aas_with_AssetInformation_if_provided()
        {
            var information = new AdtAssetAdministrationShellInformation();
            information.AssetInformation = new AdtAssetInformation
            {
                dtId = "testId",
                GlobalAssetId = "TestGlobalAssetId",
                SpecificAssetId = "TestSpecificAssetId",
                AssetKind = new AdtAssetKind() { AssetKind = "Type" }
            };
            information.RootElement = minimalAdtAas;
            var actual = objectUnderTest.GetAas(information);

            var expected = new AssetInformation(
                AssetKind.Type,
                new Reference(ReferenceTypes.GlobalReference,
                    new List<Key>() { new Key(KeyTypes.GlobalReference, "TestGlobalAssetId") }));

            actual.AssetInformation.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetAas_return_Aas_with_SubmodelInformation()
        {
            var information = new AdtAssetAdministrationShellInformation();
            information.Submodels = new List<AdtSubmodel>()
                { new AdtSubmodel() { Id = "TestId1" }, new AdtSubmodel() { Id = "TestId2" } };
            
            var expected = new List<Reference>()
            {
                new Reference(ReferenceTypes.ModelReference, new List<Key>() { new Key(KeyTypes.Submodel, "TestId1") }),
                new Reference(ReferenceTypes.ModelReference, new List<Key>() { new Key(KeyTypes.Submodel, "TestId2") })
            };

            var actualAas = objectUnderTest.GetAas(information);
            actualAas.Submodels.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetAas_return_Aas_with_DerivedFromAas()
        {
            var information = new AdtAssetAdministrationShellInformation();
            information.DerivedFrom = new AdtAas() { Id = "TestId1" };
            var expected = new Reference(ReferenceTypes.ModelReference,
                new List<Key>() { new Key(KeyTypes.AssetAdministrationShell, "TestId1") });
            var actualAas = objectUnderTest.GetAas(information);
            actualAas.DerivedFrom.Should().BeEquivalentTo(expected);
        }

    }
}
