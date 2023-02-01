using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using FluentAssertions;

namespace AAS.API.Repository.Adt.Tests
{
    [TestClass]
    public class AdtGeneralModelFactoryTests
    {
        AdtGeneralModelFactory? _objectUnderTest { get; set; }

        [TestInitialize]
        public void Setup()
        {
            _objectUnderTest = new AdtGeneralModelFactory();
        }

        [TestMethod]
        public void ConvertAdtLangStringToGeneraLangString_returns_correct_List_of_LangStrings()
        {
            AdtLanguageString adtLanguageString = new AdtLanguageString(){LangStrings = new Dictionary<string, string>() { ["de"] = "StringZumTesten" , ["en"] = "StringToTest"},Metadata = new DigitalTwinMetadata()};
            var actual = _objectUnderTest.ConvertAdtLangStringToGeneraLangString(adtLanguageString);
            var expected = new List<LangString>(){new LangString("de", "StringZumTesten"), new LangString("en", "StringToTest") };
            actual.Should().BeEquivalentTo(expected);
        }

        
    }
}