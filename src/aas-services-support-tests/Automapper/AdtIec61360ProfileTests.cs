using AAS_Services_Support.AutoMapper;
using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;
using AutoMapper;
using FluentAssertions;


namespace AAS_Services_Support_Tests.Automapper
{
    [TestClass]
    public class AdtIec61360ProfileTests
    {
        private IMapper? _objectUnderTest;
        private AdtDataSpecificationIEC61360 _fullAdtIec61360;
        private DataSpecificationIec61360 _fullDataSpecificationIec61360;
        private AdtDataSpecificationIEC61360 _minimalAdtIec61360;
        private DataSpecificationIec61360 _minimalDataSpecificationIec;

        [TestInitialize]
        public void Setup()
        {

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AdtIec61360Profile());
                cfg.DisableConstructorMapping();
                cfg.AllowNullCollections = true;
            });
            configuration.AssertConfigurationIsValid();
            _objectUnderTest = configuration.CreateMapper();
            _fullAdtIec61360 = new AdtDataSpecificationIEC61360()
            {
                Definition = new AdtLanguageString()
                {
                    LangStrings = new Dictionary<string, string>() { ["de"] = "TestDefinition" }
                },
                PreferredName = new AdtLanguageString()
                {
                    LangStrings = new Dictionary<string, string>() { ["de"] = "TestPreferredName" }
                },
                ShortName = new AdtLanguageString()
                {
                    LangStrings = new Dictionary<string, string>() { ["de"] = "TestShortName" }
                },
                DataType = "INTEGER_Count",
                LevelType = "min",
                SourceOfDefinition = "TestSource",
                Symbol = "TestSymbol",
                Unit = "TestUnit",
                UnitIdValue = "TestUnitIdValue",
                Value = "TestValue",
                ValueFormat = "TestValueFormat"
            };

            _fullDataSpecificationIec61360 = new DataSpecificationIec61360(
                new List<LangString>() { new LangString("de", "TestPreferredName") },
                new List<LangString>() { new LangString("de", "TestShortName") },
                "TestUnit",
                new Reference(ReferenceTypes.GlobalReference,
                    new List<Key>() { new Key(KeyTypes.GlobalReference, "TestUnitIdValue") }),
                "TestSource",
                "TestSymbol",
                DataTypeIec61360.IntegerCount,
                new List<LangString>() { new LangString("de", "TestDefinition") },
                "TestValueFormat",
                null,
                "TestValue",
                LevelType.Min
            );

            _minimalAdtIec61360 = new AdtDataSpecificationIEC61360()
            {
                PreferredName = new AdtLanguageString()
                {
                    LangStrings = new Dictionary<string, string>() { ["de"] = "TestPreferredName" }
                }
            };
            _minimalDataSpecificationIec = new DataSpecificationIec61360(
                new List<LangString>(new List<LangString>() { new LangString("de", "TestPreferredName") }));

        }

        [TestMethod]
        public void Map_AdtIec61360_to_DataSpecificationAdt61360_with_full_Specification()
        {
            var actual = _objectUnderTest.Map<DataSpecificationIec61360>(_fullAdtIec61360);
            var expected = _fullDataSpecificationIec61360;

            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Map_AdtIec61360_to_DataSpecificationAdt61360_with_minimal_Specification()
        {
            var actual = _objectUnderTest.Map<DataSpecificationIec61360>(_minimalAdtIec61360);
            var expected = _minimalDataSpecificationIec;
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
