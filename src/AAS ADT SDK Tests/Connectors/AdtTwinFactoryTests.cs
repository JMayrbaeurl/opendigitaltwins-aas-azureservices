using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Castle.Components.DictionaryAdapter;
using FluentAssertions;
using Newtonsoft.Json.Linq;

namespace AAS.ADT.Tests.Connectors
{
    [TestClass]
    public class AdtTwinFactoryTests
    {
        private AdtTwinFactory _objectUnderTest { get; set; }
        private ISubmodelElement _exemplaryMinimalSubmodelElement { get; set; }
        private ISubmodelElement _exemplaryFullSubmodelElement { get; set; }
        private BasicDigitalTwin _actualTwin { get; set; }


        [TestInitialize]
        public void Setup()
        {
            _actualTwin = new BasicDigitalTwin();

            _objectUnderTest = new AdtTwinFactory();
            _exemplaryMinimalSubmodelElement = new Property(DataTypeDefXsd.Boolean);
            _exemplaryFullSubmodelElement = new Property(DataTypeDefXsd.Boolean, new List<Extension>(),
                "testCategory", "testIdShort", new List<LangString>() { new LangString("en", "testDisplayName") },
                new List<LangString>() { new("en", "testDescription") }, "testChecksum", ModelingKind.Instance);
        }

        [TestMethod]
        public void CreateTwin_adds_ModelId()
        {
            _actualTwin = _objectUnderTest.CreateTwin(_exemplaryMinimalSubmodelElement, AdtAasOntology.MODEL_PROPERTY);
            _actualTwin.Metadata.ModelId.Should().Be(AdtAasOntology.MODEL_PROPERTY);
        }

        [TestMethod]
        public void CreateTwin_adds_id()
        {
            _actualTwin = _objectUnderTest.CreateTwin(_exemplaryMinimalSubmodelElement, AdtAasOntology.MODEL_PROPERTY);
            _actualTwin.Id.Should().StartWith("Property");
        }


        [TestMethod]
        public void AddSubmodelElementValues_adds_Referable_values_for_full_SubmodelElement()
        {

            _actualTwin = _objectUnderTest.CreateTwin(_exemplaryFullSubmodelElement, AdtAasOntology.MODEL_PROPERTY);

            _actualTwin.Contents["category"].Should().Be("testCategory");
            _actualTwin.Contents["idShort"].Should().Be("testIdShort");
            _actualTwin.Contents["checksum"].Should().Be("testChecksum");
            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>() { { "en", "testDescription" } });

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents["langString"].Should().BeEquivalentTo(new Dictionary<string, string>()
                { { "en", "testDisplayName" } });

        }

        [TestMethod]
        public void AddSubmodelElementValues_adds_Referable_values_for_minimal_SubmodelElement()
        {
            _actualTwin = _objectUnderTest.CreateTwin(_exemplaryMinimalSubmodelElement, AdtAasOntology.MODEL_PROPERTY);

            _actualTwin.Contents.Should().NotContainKey("category");
            _actualTwin.Contents.Should().NotContainKey("idShort");
            _actualTwin.Contents.Should().NotContainKey("checksum");

            var description = (BasicDigitalTwinComponent)_actualTwin.Contents["description"];
            description.Contents.Should().NotContainKey("langString");

            var displayName = (BasicDigitalTwinComponent)_actualTwin.Contents["displayName"];
            displayName.Contents.Should().NotContainKey("displayName");
        }

        [TestMethod]
        public void AddSubmodelElementValues_adds_Kind_for_full_SubmodelElement()
        {
            _actualTwin = _objectUnderTest.CreateTwin(_exemplaryFullSubmodelElement, AdtAasOntology.MODEL_PROPERTY);
            var kind = (BasicDigitalTwinComponent)_actualTwin.Contents["kind"];
            kind.Contents["kind"].Should().Be("Instance");
        }

        [TestMethod]
        public void AddSubmodelElementValues_does_not_add_Kind_for_minimal_SubmodelElement()
        {
            _actualTwin = _objectUnderTest.CreateTwin(_exemplaryMinimalSubmodelElement, AdtAasOntology.MODEL_PROPERTY);
            var kind = (BasicDigitalTwinComponent)_actualTwin.Contents["kind"];
            kind.Contents.Should().NotContainKey("kind");
        }

    }
}
