using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Moq;
using AAS.API.Services.ADT;
using AasCore.Aas3_0_RC02;
using Azure;

namespace AAS.ADT.Tests.Connectors
{
    [TestClass]
    public class AasUpdateAdtTests
    {
        private AasUpdateAdt _objectUnderTest;

        private Mock<DigitalTwinsClient> _digitalTwinsClient;
        private Mock<ILogger<AasUpdateAdt>> _logger;
        private Mock<IAasWriteSubmodel> _writeSubmodel;
        private Mock<IAasWriteAssetAdministrationShell> _writeShell;
        private Mock<IAasDeleteAdt> _deleteAdt;
        private Mock<IAasWriteConnector> _writeConnector;

        private Submodel _submodel;
        private AssetAdministrationShell _shell;


        [TestInitialize]
        public void Setup()
        {
            _digitalTwinsClient = new Mock<DigitalTwinsClient>();
            var clientFactory = new Mock<DigitalTwinsClientFactory>();
            clientFactory.Setup(_ => _.CreateClient()).Returns(_digitalTwinsClient.Object);
            _logger = new Mock<ILogger<AasUpdateAdt>>();
            _writeSubmodel = new Mock<IAasWriteSubmodel>();
            _writeShell = new Mock<IAasWriteAssetAdministrationShell>();
            _deleteAdt = new Mock<IAasDeleteAdt>();
            _writeConnector = new Mock<IAasWriteConnector>();

            var incomingPage = Page<BasicRelationship>.FromValues(
                new[]
                {
                    new BasicRelationship()
                    {
                        Id = "testIncomingRelationshipId", SourceId = "testSourceTwinId", Name = "testRelationshipName"
                    },
                    new BasicRelationship()
                    {
                        Id = "testIncomingRelationshipId2", SourceId = "testSourceTwinId2", Name = "testRelationshipName2"
                    }
                },
                "continuationToken", Mock.Of<Response>());
            var incomingRelationshipQueryResponse = Pageable<BasicRelationship>.FromPages(new[] { incomingPage });
            _digitalTwinsClient.Setup(_ =>
                    _.Query<BasicRelationship>(
                        "SELECT * FROM RELATIONSHIPS r WHERE r.$targetId = 'testSubmodelTwinId'", default(CancellationToken)))
                .Returns(incomingRelationshipQueryResponse);

            var shellSubmodelPages = Page<BasicRelationship>.FromValues(
                new[]
                {
                    new BasicRelationship()
                    {
                        Id = "testSubmodelRelationshipId", TargetId = "testSubmodelTwinId1", Name = "submodel"
                    },
                    new BasicRelationship()
                    {
                        Id = "testSubmodelRelationshipId2", TargetId = "testSubmodelTwinId2", Name = "submodel"
                    }
                },
                "continuationToken", Mock.Of<Response>());
            var shellRelationshipQueryResponse = Pageable<BasicRelationship>.FromPages(new[] { shellSubmodelPages });
            _digitalTwinsClient.Setup(_ =>
                    _.Query<BasicRelationship>(
                        "SELECT * FROM RELATIONSHIPS r WHERE r.$sourceId = 'testShellTwinId' and r.$relationshipName='submodel'",
                        default(CancellationToken)))
                .Returns(shellRelationshipQueryResponse);

            _writeSubmodel.Setup(_ => _.CreateSubmodel(It.IsAny<Submodel>())).ReturnsAsync("testNewSubmodelTwinId");
            _writeShell.Setup(_ => _.CreateShell(It.IsAny<AssetAdministrationShell>()))
                .ReturnsAsync("testNewShellTwinId");

            _objectUnderTest = new AasUpdateAdt(
                _logger.Object, _writeSubmodel.Object, _deleteAdt.Object, clientFactory.Object, 
                _writeConnector.Object,_writeShell.Object);

            _submodel = new Submodel("testSubmodelIdentifier");
            _shell = new AssetAdministrationShell("testShellId", new AssetInformation(AssetKind.Instance));
        }


        [TestMethod]
        public async Task UpdateFullSubmodel_deletes_old_submodel()
        {
            await _objectUnderTest.UpdateFullSubmodel("testSubmodelTwinId", _submodel);
            
            _deleteAdt.Verify(_ => _.DeleteTwin("testSubmodelTwinId"), Times.Once);
        }

        [TestMethod]
        public async Task UpdateFullSubmodel_creates_New_submodel()
        {
            await _objectUnderTest.UpdateFullSubmodel("testSubmodelTwinId", _submodel);
            
            _writeSubmodel.Verify(_ => _.CreateSubmodel(_submodel), Times.Once);
        }

        [TestMethod]
        public async Task UpdateFullSubmodel_creates_Shell_to_Submodel_relationships_again()
        {
            await _objectUnderTest.UpdateFullSubmodel("testSubmodelTwinId", _submodel);
            
            _writeConnector.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(
                "testSourceTwinId", "testRelationshipName", "testNewSubmodelTwinId"), Times.Once);
            _writeConnector.Verify(_ => _.DoCreateOrReplaceRelationshipAsync(
                "testSourceTwinId2", "testRelationshipName2", "testNewSubmodelTwinId"), Times.Once);
        }

        [TestMethod]
        public async Task UpdateFullShell_deletes_old_shell()
        {
            await _objectUnderTest.UpdateFullShell("testShellTwinId", _shell);
            
            _deleteAdt.Verify(_ => _.DeleteTwin("testShellTwinId"), Times.Once);

        }

        [TestMethod]
        public async Task UpdateFullShell_creates_New_Shell()
        {
            await _objectUnderTest.UpdateFullShell("testShellTwinId", _shell);
            
            _writeShell.Verify(_ => _.CreateShell(_shell), Times.Once);
        }

        [TestMethod]
        public async Task UpdateFullShell_recreates_submodel_References()
        {
            await _objectUnderTest.UpdateFullShell("testShellTwinId", _shell);

            _writeShell.Verify(_=>_.CreateSubmodelReference("testNewShellTwinId","testSubmodelTwinId1"),Times.Once);
            _writeShell.Verify(_ => _.CreateSubmodelReference("testNewShellTwinId", "testSubmodelTwinId2"), Times.Once);
        }
    }
}
