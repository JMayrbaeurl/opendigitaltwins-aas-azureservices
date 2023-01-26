using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.ADT.Tests.Connectors
{
    [TestClass]
    public class AdtTwinFactoryTests
    {
        private AdtTwinFactory _objectUnderTest { get; set; }
        private ISubmodelElement _exemplaryMinimalSubmodelElement { get; set; }
        private ISubmodelElement _exemplaryFullSubmodelElement { get; set; }
       
        private Submodel _exemplaryMinimalSubmodel { get; set; }
        private Submodel _exemplaryFullSubmodel { get; set; }
        
        private AssetAdministrationShell _exemplaryMinimalShell { get; set; }
        private AssetAdministrationShell _exemplaryFullShell { get; set; }

        private AssetInformation _fullAssetInformation { get; set; }
        private AssetInformation _minimalAssetInformation { get; set; }
        

        private BasicDigitalTwin _actualTwin { get; set; }


        [TestInitialize]
        public void Setup()
        {
            _actualTwin = new BasicDigitalTwin();

            var _logger = new Mock<ILogger<AdtTwinFactory>>();

            _objectUnderTest = new AdtTwinFactory(_logger.Object);
            _exemplaryMinimalSubmodelElement = new Property(DataTypeDefXsd.Boolean);
            _exemplaryFullSubmodelElement = new Property(DataTypeDefXsd.Boolean, new List<Extension>(),
                "testCategory", "testIdShort", new List<LangString>() { new LangString("en", "testDisplayName") },
                new List<LangString>() { new("en", "testDescription") }, "testChecksum", ModelingKind.Instance);

            _exemplaryFullSubmodel = new Submodel("testId", new List<Extension>(), "testCategory", "testIdShort",
                new List<LangString>() { new LangString("en", "testDisplayName") },
                new List<LangString>() { new("en", "testDescription") }, "testChecksum",
                new AdministrativeInformation(null,"1","2"), ModelingKind.Instance);
            _exemplaryMinimalSubmodel = new Submodel("testId");

            _fullAssetInformation = new AssetInformation(
                AssetKind.Instance, new Reference(ReferenceTypes.GlobalReference,
                    new List<Key>() { new Key(KeyTypes.GlobalReference, "testGlobalAssetId") }),
                new List<SpecificAssetId>()
                {
                    new ("Serial number","1234",new Reference(ReferenceTypes.GlobalReference,new List<Key>())),
                        new ("FID", "5678", new Reference(ReferenceTypes.GlobalReference, new List<Key>()))
                },new Resource("testDefaultThumbnailPath"));

            _minimalAssetInformation = new AssetInformation(AssetKind.Type);
            
            _exemplaryFullShell = new AssetAdministrationShell("testId", _fullAssetInformation,
                new List<Extension>(), "testCategory", "testIdShort",
                new List<LangString>() { new LangString("en", "testDisplayName") },
                new List<LangString>() { new("en", "testDescription") }, "testChecksum",
                new AdministrativeInformation(null, "1", "2"),new List<EmbeddedDataSpecification>());
            _exemplaryMinimalShell = new AssetAdministrationShell("testId", new AssetInformation(AssetKind.Type));
        }


        [TestMethod]
        public void GetTwin_adds_Referable_values_for_full_SubmodelElement()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullSubmodelElement);

            _actualTwin.Contents["category"].Should().Be("testCategory");
            _actualTwin.Contents["idShort"].Should().Be("testIdShort");
            _actualTwin.Contents["checksum"].Should().Be("testChecksum");
            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDescription" } });

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDisplayName" } });
            var tags = (BasicDigitalTwinComponent)_actualTwin.Contents["tags"];
            tags.Should().NotBeNull();
        }

        [TestMethod]
        public void GetTwin_adds_Referable_values_for_minimal_SubmodelElement()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalSubmodelElement);

            _actualTwin.Contents.Should().NotContainKey("category");
            _actualTwin.Contents.Should().NotContainKey("idShort");
            _actualTwin.Contents.Should().NotContainKey("checksum");

            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents.Should().NotContainKey("langString");

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents.Should().NotContainKey("displayName");
            var tags = (BasicDigitalTwinComponent)_actualTwin.Contents["tags"];
            tags.Should().NotBeNull();
        }

        [TestMethod]
        public void GetTwin_adds_Kind_for_full_SubmodelElement()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullSubmodelElement);
            var kind = (BasicDigitalTwinComponent)_actualTwin.Contents["kind"];
            kind.Contents["kind"].Should().Be("Instance");
        }

        [TestMethod]
        public void GetTwin_does_not_add_Kind_for_minimal_SubmodelElement()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalSubmodelElement);
            var kind = (BasicDigitalTwinComponent)_actualTwin.Contents["kind"];
            kind.Contents.Should().NotContainKey("kind");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_Property()
        {
            _actualTwin = _objectUnderTest.GetTwin(new Property(DataTypeDefXsd.Boolean));
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_PROPERTY);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_Property()
        {
            _actualTwin = _objectUnderTest.GetTwin(new Property(DataTypeDefXsd.Boolean));
            _actualTwin.Id.Should().StartWith("Property");
        }

        [TestMethod]
        public void GetTwin_returns_correct_twin_for_full_Property()
        {
            var property = new Property(DataTypeDefXsd.Boolean, value: "testValue");
            _actualTwin = _objectUnderTest.GetTwin(property);
            _actualTwin.Contents["valueType"].Should().Be("Boolean");
            _actualTwin.Contents["value"].Should().Be("testValue");
        }

        [TestMethod]
        public void GetTwin_returns_correct_twin_for_minimal_Property()
        {
            var property = new Property(DataTypeDefXsd.Boolean);
            _actualTwin = _objectUnderTest.GetTwin(property);
            _actualTwin.Contents["valueType"].Should().Be("Boolean");
            _actualTwin.Contents.Should().NotContainKey("value");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_SubmodelElementCollection()
        {
            _actualTwin = _objectUnderTest.GetTwin(new SubmodelElementCollection());
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_SUBMODELELEMENTCOLLECTION);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_SubmodelElementCollection()
        {
            _actualTwin = _objectUnderTest.GetTwin(new SubmodelElementCollection());
            _actualTwin.Id.Should().StartWith("SubmodelElements");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_File()
        {
            _actualTwin = _objectUnderTest.GetTwin(new File("testContentType"));
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_FILE);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_File()
        {
            _actualTwin = _objectUnderTest.GetTwin(new File("testContentType"));
            _actualTwin.Id.Should().StartWith("File");
        }

        [TestMethod]
        public void GetTwin_returns_correct_twin_for_full_File()
        {
            var file = new File("text/plain", value: "testValue");
            _actualTwin = _objectUnderTest.GetTwin(file);
            _actualTwin.Contents["contentType"].Should().Be("text/plain");
            _actualTwin.Contents["value"].Should().Be("testValue");
        }

        [TestMethod]
        public void GetTwin_returns_correct_twin_for_minimal_File()
        {
            var file = new File("text/plain");
            _actualTwin = _objectUnderTest.GetTwin(file);
            _actualTwin.Contents["contentType"].Should().Be("text/plain");
            _actualTwin.Contents.Should().NotContainKey("value");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_Reference()
        {
            _actualTwin = _objectUnderTest.GetTwin(new Reference(ReferenceTypes.GlobalReference,new List<Key>()));
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_REFERENCE);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_Reference()
        {
            _actualTwin = _objectUnderTest.GetTwin(new Reference(ReferenceTypes.GlobalReference, new List<Key>()));
            _actualTwin.Id.Should().StartWith("Reference");
        }

        [TestMethod]
        public void GetTwin_sets_ReferenceType_correct_for_given_Reference()
        {
            var reference = new Reference(ReferenceTypes.GlobalReference,
                new List<Key>());
            _actualTwin = _objectUnderTest.GetTwin(reference);
            _actualTwin.Contents["type"].Should().Be("GlobalReference");
        }

        [TestMethod]
        public void GetTwin_sets_Keys_correct_twin_for_full_Reference()
        {
            var reference = new Reference(ReferenceTypes.GlobalReference,
                new List<Key>()
                {
                    new Key(KeyTypes.GlobalReference, "testKeyValue1"),
                    new Key(KeyTypes.Blob, "testKeyValue2")

                }, null);
            _actualTwin = _objectUnderTest.GetTwin(reference);
            var key1 = (BasicDigitalTwinComponent)_actualTwin.Contents["key1"];
            key1.Contents["type"].Should().Be("GlobalReference");
            key1.Contents["value"].Should().Be("testKeyValue1");
            var key2 = (BasicDigitalTwinComponent)_actualTwin.Contents["key2"];
            key2.Contents["type"].Should().Be("Blob");
            key2.Contents["value"].Should().Be("testKeyValue2");
        }

        [TestMethod]
        public void GetTwin_sets_Keys_correct_twin_for_minimal_Reference()
        {
            var reference = new Reference(ReferenceTypes.ModelReference, new List<Key>());
            _actualTwin = _objectUnderTest.GetTwin(reference);
            var key1 = (BasicDigitalTwinComponent)_actualTwin.Contents["key1"];
            key1.Contents.Should().NotContainKey("value");
            var key2 = (BasicDigitalTwinComponent)_actualTwin.Contents["key2"];
            key2.Contents.Should().NotContainKey("value");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_Iec61360Content()
        {
            _actualTwin = _objectUnderTest.GetTwin(new DataSpecificationIec61360(new List<LangString>()));
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_DATASPECIEC61360);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_Iec61360Content()
        {
            _actualTwin = _objectUnderTest.GetTwin(new DataSpecificationIec61360(new List<LangString>()));
            _actualTwin.Id.Should().StartWith("DataSpecIEC61360");
        }

        [TestMethod]
        public void GetTwin_returns_DataSpecificationContentIec61360_twin_when_provided_with_full_Iec61360_content()
        {
            var content = new DataSpecificationIec61360(
                new List<LangString>() {new LangString("en", "testPreferredName")},
                new List<LangString>() { new LangString("en", "testShortName") },
                "testUnit", null, "testSourceOfDefinition","testSymbol",DataTypeIec61360.Date,
                new List<LangString>() { new LangString("en", "testDefinition") },
                "testValueFormat", null,"testValue", LevelType.Max);
            _actualTwin = _objectUnderTest.GetTwin(content);
            
            _actualTwin.Contents["unit"].Should().Be("testUnit");
            _actualTwin.Contents["sourceOfDefinition"].Should().Be("testSourceOfDefinition");
            _actualTwin.Contents["symbol"].Should().Be("testSymbol");
            _actualTwin.Contents["dataType"].Should().Be("DATE");
            _actualTwin.Contents["valueFormat"].Should().Be("testValueFormat");
            _actualTwin.Contents["value"].Should().Be("testValue");
            _actualTwin.Contents["levelType"].Should().Be("Max");

            var preferredName = (BasicDigitalTwinComponent)_actualTwin.Contents["preferredName"];
            preferredName.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testPreferredName" } });
            var shortName = (BasicDigitalTwinComponent)_actualTwin.Contents["shortName"];
            shortName.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testShortName" } });
            var definition = (BasicDigitalTwinComponent)_actualTwin.Contents["definition"];
            definition.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDefinition" } });
        }

        [TestMethod]
        public void GetTwin_returns_DataSpecificationContentIec61360_twin_when_provided_with_minimal_Iec61360_content()
        {
            var content =
                new DataSpecificationIec61360(new List<LangString>() { new LangString("en", "testPreferredName") });
            _actualTwin = _objectUnderTest.GetTwin(content);

            var preferredName = (BasicDigitalTwinComponent)_actualTwin.Contents["preferredName"];
            preferredName.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testPreferredName" } });

            _actualTwin.Contents.Should().NotContainKey("unit");
            _actualTwin.Contents.Should().NotContainKey("sourceOfDefinition");
            _actualTwin.Contents.Should().NotContainKey("symbol");
            _actualTwin.Contents.Should().NotContainKey("dataType");
            _actualTwin.Contents.Should().NotContainKey("valueFormat");
            _actualTwin.Contents.Should().NotContainKey("value");
            _actualTwin.Contents.Should().NotContainKey("levelType");
        }

        [TestMethod]
        public void GetTwin_returns_correct_formatted_complex_dataType_on_Iec61360_content()
        {
            var content =
                new DataSpecificationIec61360(new List<LangString>()
                {
                    new LangString("en", "testPreferredName")
                },dataType: DataTypeIec61360.IntegerCount);
            _actualTwin = _objectUnderTest.GetTwin(content);
            _actualTwin.Contents["dataType"].Should().Be("INTEGER_COUNT");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_Qualifier()
        {
            _actualTwin = _objectUnderTest.GetTwin(new Qualifier("testType",DataTypeDefXsd.Boolean));
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_QUALIFIER);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_Qualifier()
        {
            _actualTwin = _objectUnderTest.GetTwin(new Qualifier("testType", DataTypeDefXsd.Boolean));
            _actualTwin.Id.Should().StartWith("Qualifier");
        }

        [TestMethod]
        public void GetTwin_returns_QualifierTwin_when_provided_with_full_Qualifier()
        {
            var qualifier = new Qualifier("testType", DataTypeDefXsd.Boolean, null, null,
                QualifierKind.ConceptQualifier, "testValue", null);
            _actualTwin = _objectUnderTest.GetTwin(qualifier);

            _actualTwin.Contents["type"].Should().Be("testType");
            _actualTwin.Contents["valueType"].Should().Be("Boolean");
            _actualTwin.Contents["value"].Should().Be("testValue");
            _actualTwin.Contents["kind"].Should().Be("ConceptQualifier");
        }

        [TestMethod]
        public void GetTwin_returns_QualifierTwin_when_provided_with_minimal_Qualifier()
        {
            var qualifier = new Qualifier("testType", DataTypeDefXsd.Boolean, null, null, null,null, null);
            _actualTwin = _objectUnderTest.GetTwin(qualifier);

            _actualTwin.Contents["type"].Should().Be("testType");
            _actualTwin.Contents["valueType"].Should().Be("Boolean");
            _actualTwin.Contents.Should().NotContainKey("value");
            _actualTwin.Contents.Should().NotContainKey("kind");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_Submodel()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalSubmodel);
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_SUBMODEL);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_Submodel()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalSubmodel);
            _actualTwin.Id.Should().StartWith("Submodel");
        }

        [TestMethod]
        public void GetTwin_adds_Referable_values_for_full_Submodel()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullSubmodel);

            _actualTwin.Contents["category"].Should().Be("testCategory");
            _actualTwin.Contents["idShort"].Should().Be("testIdShort");
            _actualTwin.Contents["checksum"].Should().Be("testChecksum");
            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDescription" } });

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDisplayName" } });
            var tags = (BasicDigitalTwinComponent)_actualTwin.Contents["tags"];
            tags.Should().NotBeNull();
        }

        [TestMethod]
        public void GetTwin_adds_Referable_values_for_minimal_Submodel()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalSubmodel);

            _actualTwin.Contents.Should().NotContainKey("category");
            _actualTwin.Contents.Should().NotContainKey("idShort");
            _actualTwin.Contents.Should().NotContainKey("checksum");

            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents.Should().NotContainKey("langString");

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents.Should().NotContainKey("displayName");
            var tags = (BasicDigitalTwinComponent)_actualTwin.Contents["tags"];
            tags.Should().NotBeNull();
        }

        [TestMethod]
        public void GetTwin_adds_Identifiable_values_for_full_Submodel()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullSubmodel);
            _actualTwin.Contents["id"].Should().Be("testId");
            var administration = (BasicDigitalTwinComponent)_actualTwin.Contents["administration"];
            administration.Contents["version"].Should().Be("1");
            administration.Contents["revision"].Should().Be("2");

        }

        [TestMethod]
        public void GetTwin_adds_Kind_for_full_Submodel()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullSubmodel);
            var kind = (BasicDigitalTwinComponent)_actualTwin.Contents["kind"];
            kind.Contents["kind"].Should().Be("Instance");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_Shell()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalShell);
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_SHELL);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_Shell()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalShell);
            _actualTwin.Id.Should().StartWith("Shell");
        }

        [TestMethod]
        public void GetTwin_adds_Referable_values_for_full_Shell()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullShell);

            _actualTwin.Contents["category"].Should().Be("testCategory");
            _actualTwin.Contents["idShort"].Should().Be("testIdShort");
            _actualTwin.Contents["checksum"].Should().Be("testChecksum");
            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDescription" } });

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDisplayName" } });
            var tags = (BasicDigitalTwinComponent)_actualTwin.Contents["tags"];
            tags.Should().NotBeNull();
        }

        [TestMethod]
        public void GetTwin_adds_Referable_values_for_minimal_Shell()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalShell);

            _actualTwin.Contents.Should().NotContainKey("category");
            _actualTwin.Contents.Should().NotContainKey("idShort");
            _actualTwin.Contents.Should().NotContainKey("checksum");

            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents.Should().NotContainKey("langString");

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents.Should().NotContainKey("displayName");
            var tags = (BasicDigitalTwinComponent)_actualTwin.Contents["tags"];
            tags.Should().NotBeNull();
        }

        [TestMethod]
        public void GetTwin_adds_Identifiable_values_for_full_Shell()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullShell);
            _actualTwin.Contents["id"].Should().Be("testId");
            var administration = (BasicDigitalTwinComponent)_actualTwin.Contents["administration"];
            administration.Contents["version"].Should().Be("1");
            administration.Contents["revision"].Should().Be("2");
        }

        [TestMethod]
        public void GetTwin_adds_AssetInformationShort_for_full_Shell()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryFullShell);
            var assetInfoShort = (BasicDigitalTwinComponent)_actualTwin.Contents["assetInformationShort"];
            assetInfoShort.Contents["assetKind"].Should().Be("Instance");
            assetInfoShort.Contents["globalAssetId"].Should().Be("(GlobalReference)testGlobalAssetId");
            assetInfoShort.Contents["specificAssetId"].Should().Be("(Serial number)1234, (FID)5678");
            assetInfoShort.Contents["defaultThumbnailpath"].Should().Be("testDefaultThumbnailPath");
        }

        [TestMethod]
        public void GetTwin_adds_AssetInformationShort_for_minimal_Shell()
        {
            _actualTwin = _objectUnderTest.GetTwin(_exemplaryMinimalShell);
            var assetInfoShort = (BasicDigitalTwinComponent)_actualTwin.Contents["assetInformationShort"];
            assetInfoShort.Contents["assetKind"].Should().Be("Type");
            assetInfoShort.Contents.Should().NotContainKey("globalAssetId");
            assetInfoShort.Contents.Should().NotContainKey("specificAssetId");
            assetInfoShort.Contents.Should().NotContainKey("defaultThumbnailpath");
        }

        [TestMethod]
        public void GetTwin_adds_ModelId_for_AssetInformation()
        {
            _actualTwin = _objectUnderTest.GetTwin(_minimalAssetInformation);
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_ASSETINFORMATION);
        }

        [TestMethod]
        public void GetTwin_adds_id_for_AssetInformation()
        {
            _actualTwin = _objectUnderTest.GetTwin(_minimalAssetInformation);
            _actualTwin.Id.Should().StartWith("AssetInfo");
        }

        [TestMethod]
        public void GetTwin_returns_correct_twin_for_full_AssetInformation()
        {
            _actualTwin = _objectUnderTest.GetTwin(_fullAssetInformation);
            var assetKind = (BasicDigitalTwinComponent)_actualTwin.Contents["assetKind"];
            assetKind.Contents["assetKind"].Should().Be("Instance");
        }

        [TestMethod]
        public void GetTwin_returns_correct_twin_for_minimal_AssetInformation()
        {
            _actualTwin = _objectUnderTest.GetTwin(_minimalAssetInformation);
            var assetKind = (BasicDigitalTwinComponent)_actualTwin.Contents["assetKind"];
            assetKind.Contents["assetKind"].Should().Be("Type");
        }

    }
}
