using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0_RC02;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace AAS.ADT.Tests.Connectors
{
    [TestClass]
    public class AasWriteAssetAdministrationShellTests
    {
        private AasWriteAssetAdministrationShell _objectUnderTest;
        private Mock<ILogger<AasWriteAssetAdministrationShell>> _loggerMock;
        private Mock<IAdtTwinFactory> _adtTwinFactoryMock;
        private Mock<IAasWriteConnector> _writeConnectorMock;
        private Mock<IAasWriteBase> _writeBaseMock;

        private AssetAdministrationShell _exemplaryShell;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<AasWriteAssetAdministrationShell>>();
            _writeConnectorMock = new Mock<IAasWriteConnector>();
            _adtTwinFactoryMock = new Mock<IAdtTwinFactory>();
            _writeBaseMock = new Mock<IAasWriteBase>();

            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<AssetAdministrationShell>())).Returns(
                new BasicDigitalTwin
                {
                    Id = "testAasTwinId"
                });
            _adtTwinFactoryMock.Setup(_ => _.GetTwin(It.IsAny<AssetInformation>())).Returns(
                new BasicDigitalTwin
                {
                    Id = "testAssetInformationTwinId"
                });

            _objectUnderTest = new AasWriteAssetAdministrationShell(_loggerMock.Object, _adtTwinFactoryMock.Object,
                _writeConnectorMock.Object, _writeBaseMock.Object);

            _exemplaryShell = new AssetAdministrationShell("testId", new AssetInformation(AssetKind.Instance));
        }

        [TestMethod]
        public async Task CreateShell_does_not_try_creating_anything_if_shell_is_null()
        {
            await _objectUnderTest.CreateShell(null);
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateShell_creates_shell_twin()
        {
            await _objectUnderTest.CreateShell(_exemplaryShell);
            _adtTwinFactoryMock.Verify(_=>_.GetTwin(It.IsAny<AssetAdministrationShell>()),Times.Once);
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Exactly(2));
        }

        [TestMethod]
        public async Task CreateShell_creates_DataSpecification()
        {
            await _objectUnderTest.CreateShell(_exemplaryShell);
            _writeBaseMock.Verify(_ => _.AddHasDataSpecification("testAasTwinId", null), Times.Once);
        }

        [TestMethod]
        public async Task CreateShell_creates_AssetInformation()
        {
            await _objectUnderTest.CreateShell(_exemplaryShell);
            _adtTwinFactoryMock.Verify(_ => _.GetTwin(It.IsAny<AssetInformation>()), Times.Once);
            _writeConnectorMock.Verify(_ => _.DoCreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>()),
                Times.Exactly(2));
            _writeConnectorMock.Verify(_ =>
                _.DoCreateOrReplaceRelationshipAsync("testAasTwinId", "assetInformation",
                    "testAssetInformationTwinId"));
        }   
    }
}
