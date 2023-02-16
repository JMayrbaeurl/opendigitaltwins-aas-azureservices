using AAS.API.Services.ADT;
using AutoMapper;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using FluentAssertions;

namespace AAS.ADT.Tests.Connectors
{

    [TestClass]
    public class AasDeleteAdtTests
    {
        private AasDeleteAdt _objectUnderTest { get; set; }

        private Mock<DigitalTwinsClient> _digitalTwinsClient;
        private Mock<ILogger<AasDeleteAdt>> _logger { get; set; }

        [TestInitialize]
        public void Setup()
        {
            _logger = new Mock<ILogger<AasDeleteAdt>>();
            _digitalTwinsClient = new Mock<DigitalTwinsClient>();

            var incomingPage = Page<BasicRelationship>.FromValues(
                new[] { new BasicRelationship() { Id = "testIncomingRelationshipId", SourceId = "testSourceTwinId"} }, "continuationToken", Mock.Of<Response>());
            var incomingRelationshipQueryResponse = Pageable<BasicRelationship>.FromPages(new[] { incomingPage });
            _digitalTwinsClient.Setup(_ =>
                _.Query<BasicRelationship>(
                    "SELECT * FROM RELATIONSHIPS r WHERE r.$targetId = 'testTwinId'", default(CancellationToken)))
                .Returns(incomingRelationshipQueryResponse);

            var outgoingPage = Page<BasicRelationship>.FromValues(
                new[] { new BasicRelationship() { Id = "testOutgoingRelationshipId", SourceId = "testTwinId"} }, "continuationToken",
                Mock.Of<Response>());
            var outgoingRelationshipQueryResponse = Pageable<BasicRelationship>.FromPages(new[] { outgoingPage });
            _digitalTwinsClient.Setup(_ =>
                                _.Query<BasicRelationship>(
                                    "SELECT * FROM RELATIONSHIPS r WHERE r.$sourceId = 'testTwinId'",
                                    default(CancellationToken)))
                            .Returns(outgoingRelationshipQueryResponse);

            var relationshipPage = Page<BasicRelationship>.FromValues(
                new[] { new BasicRelationship() { Id = "testRelationshipId", SourceId = "testSourceTwinId"} }, "continuationToken",
                Mock.Of<Response>());
            var relationshipQueryResponse = Pageable<BasicRelationship>.FromPages(new[] { relationshipPage });
            _digitalTwinsClient.Setup(_ =>
                    _.Query<BasicRelationship>(
                        "SELECT * FROM RELATIONSHIPS r WHERE r.$sourceId = 'testSourceTwinId' AND " +
                        "r.$targetId = 'testTargetTwinId' AND " +
                        "r.$relationshipName = 'testRelationshipName'",
                        default(CancellationToken)))
                .Returns(relationshipQueryResponse);


            var digitalTwinsClientFactory = new Mock<DigitalTwinsClientFactory>();
            digitalTwinsClientFactory.Setup(_ => _.CreateClient()).Returns(_digitalTwinsClient.Object);

            _objectUnderTest = new AasDeleteAdt(digitalTwinsClientFactory.Object, _logger.Object);
        }

        [TestMethod]
        public async Task DeleteTwin_does_not_call_digitalTwinClient_if_provided_with_null()
        {
            await _objectUnderTest.DeleteTwin(null);
            _digitalTwinsClient.Verify(_ => _.DeleteDigitalTwinAsync(It.IsAny<string>(), null, default(CancellationToken)),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteTwin_calls_digitalTwinClient_to_delete_twin()
        {
            await _objectUnderTest.DeleteTwin("testTwinId");
            _digitalTwinsClient.Verify(_ => _.DeleteDigitalTwinAsync("testTwinId", null, default(CancellationToken)),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteTwin_calls_digitalTwinClient_to_delete_incoming_and_outgoing_relationships()
        {
            await _objectUnderTest.DeleteTwin("testTwinId");
            _digitalTwinsClient.Verify(
                _ => _.DeleteRelationshipAsync("testSourceTwinId", "testIncomingRelationshipId", null,
                    default(CancellationToken)),
                Times.Once);
            _digitalTwinsClient.Verify(
                _ => _.DeleteRelationshipAsync("testTwinId", "testOutgoingRelationshipId", null,
                    default(CancellationToken)),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteRelationship_calls_digitalTwinClient_to_delete_correct_relationship_when_everything_ok()
        {
            await _objectUnderTest.DeleteRelationship("testSourceTwinId","testTargetTwinId", "testRelationshipName");
            _digitalTwinsClient.Verify(_ => _.DeleteRelationshipAsync("testSourceTwinId", "testRelationshipId", null,
                    default(CancellationToken)),
                Times.Once);
        }


    }
}
