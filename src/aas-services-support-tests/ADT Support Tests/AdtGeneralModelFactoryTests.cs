using AAS_Services_Support.ADT_Support;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;
using Azure.DigitalTwins.Core;
using FluentAssertions;

namespace AAS_Services_Support_Tests
{
    [TestClass]
    public class AdtGeneralModelFactoryTests
    {
        AdtGeneralModelFactory? objectUnderTest { get; set; }

        [TestInitialize]
        public void Setup()
        {
            objectUnderTest = new AdtGeneralModelFactory();
        }

        [TestMethod]
        public void ConvertAdtLangStringToGeneraLangString_returns_correct_List_of_LangStrings()
        {
            AdtLanguageString adtLanguageString = new AdtLanguageString(){LangStrings = new Dictionary<string, string>() { ["de"] = "StringZumTesten" , ["en"] = "StringToTest"},Metadata = new DigitalTwinMetadata()};
            var actual = objectUnderTest.ConvertAdtLangStringToGeneraLangString(adtLanguageString);
            var expected = new List<LangString>(){new LangString("de", "StringZumTesten"), new LangString("en", "StringToTest") };
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetSemanticId_returns_correct_Reference()
        {
            AdtReference adtSemanticId = new AdtReference()
                { Key1 = new AdtKey() { Type = "GlobalReference", Value = "TestSemanticId" } };
            var actual = objectUnderTest.GetSemanticId(adtSemanticId);
            var expected = new Reference(ReferenceTypes.GlobalReference,
                new List<Key>() { new Key(KeyTypes.GlobalReference, "TestSemanticId") });
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void GetSemanticId_returns_always_Reference_with_just_one_KeyValuePair()
        {
            AdtReference adtSemanticId = new AdtReference()
            {
                Key1 = new AdtKey() { Type = "GlobalReference", Value = "TestSemanticId" },
                Key2 = new AdtKey() { Type = "GlobalReference", Value = "A second Key"}
            };
            var actual = objectUnderTest.GetSemanticId(adtSemanticId);
            actual.Keys.Count.Should().Be(1);
        }
    }
}