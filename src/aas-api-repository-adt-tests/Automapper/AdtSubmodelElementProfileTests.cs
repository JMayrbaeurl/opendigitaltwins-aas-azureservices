using AAS.API.Repository.Adt.AutoMapper;
using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using FluentAssertions;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.API.Repository.Adt.Tests
{
    [TestClass]
    public class AdtSubmodelElementProfileTests
    {
        private IMapper? _objectUnderTest;
        private ISubmodelElement _fullSubmodelElement;
        private AdtSubmodelElement _fullAdtSubmodelElement;
        private AdtFile _minimalAdtSubmodelElement;
        private File _minimalSubmodelElement;


        [TestInitialize]
        public void Setup()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AdtSubmodelElementProfile());
                cfg.AddProfile(new AdtReferableProfile());
                cfg.DisableConstructorMapping();
                cfg.AllowNullCollections = true;
            });
            configuration.AssertConfigurationIsValid();
            _objectUnderTest = configuration.CreateMapper();
            _fullAdtSubmodelElement = new AdtSubmodelElement
            {
                dtId = "TestDtId",
                Category = "TestCategory",
                Description = new AdtLanguageString
                    { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDescription" } },
                DisplayName = new AdtLanguageString
                    { LangStrings = new Dictionary<string, string>() { ["en"] = "TestDisplayName" } },
                Checksum = "1234",
                IdShort = "TestIdShort",
                Kind = new AdtHasKind { Kind = "Instance" },
                SemanticIdValue = "TestSemanticIdValue"
            };

            _fullSubmodelElement = new Property(
                DataTypeDefXsd.Boolean,
                null,
                "TestCategory",
                "TestIdShort",
                new List<LangString>() { new LangString("en", "TestDisplayName") },
                new List<LangString>() { new LangString("en", "TestDescription") },
                "1234",
                ModelingKind.Instance,
                null,
                null,
                null,
                null,
                null,
                null);

            _minimalAdtSubmodelElement = new AdtFile();
            _minimalSubmodelElement = new AasCore.Aas3_0_RC02.File("TestContentType");

        }

        [TestMethod]
        public void Map_FullAdtSubmodelElement_To_FullSubmodelElement()
        {
            var actual = new Property(DataTypeDefXsd.Boolean);
            actual = _objectUnderTest.Map(_fullAdtSubmodelElement,actual);
            var expected = _fullSubmodelElement;
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Map_MinimalAdtSubmodelElement_To_FullSubmodelElement()
        {
            var input = new AasCore.Aas3_0_RC02.File("TestContentType");
            var actual = _objectUnderTest.Map(_minimalAdtSubmodelElement, _minimalSubmodelElement);
            var expected = _minimalSubmodelElement;
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Map_creates_new_Properties()
        {
            var adtProperty1 = new AdtProperty() { ValueType = "String" };
            var adtProperty2 = new AdtProperty() { ValueType = "Boolean" };

            var actual1 = _objectUnderTest.Map<Property>(adtProperty1);
            var actual2 = _objectUnderTest.Map<Property>(adtProperty2);

            var expected1 = new Property(DataTypeDefXsd.String);
            var expected2 = new Property(DataTypeDefXsd.Boolean);
            
            actual1.Should().BeEquivalentTo(expected1);
            actual2.Should().BeEquivalentTo(expected2);
        }

        [TestMethod]
        public void Map_creates_new_File()
        {
            var adtFile1 = new File("ContentType1");
            var adtFile2 = new File("ContentType2");

            var actual1 = _objectUnderTest.Map<File>(adtFile1);
            var actual2 = _objectUnderTest.Map<File>(adtFile2);
            
            var expected1 = new File("ContentType1");
            var expected2 = new File("ContentType2");
            
            actual1.Should().BeEquivalentTo(expected1);
            actual2.Should().BeEquivalentTo(expected2);
        }
    }
}
