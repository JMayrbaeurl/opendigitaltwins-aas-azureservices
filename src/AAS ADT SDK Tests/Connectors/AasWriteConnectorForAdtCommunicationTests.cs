using System.Runtime.InteropServices;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Azure.DigitalTwins.Core;
using Castle.Components.DictionaryAdapter.Xml;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace AAS.ADT.Tests
{
    [TestClass]
    public class AasWriteConnectorForAdtCommunicationTests
    {
        public AutoMocker autoMocker { get; set; }

        private Mock<DigitalTwinsClient> digitalTwinsClient { get; set; }
        private AasWriteConnectorForAdtCommunication objectUnderTest { get; set; }

        private Mock<ILogger<AasWriteConnectorForAdtCommunication>> logger { get; set; }
        private Mock<IAASRepo> repo { get; set; }
        private Mock<IMapper> mapper { get; set; }
        private Mock<Azure.Response<AdtReference>> azureResponseMock { get; set; }
        private Mock<Azure.Response<BasicRelationship>> azureResponseMockRelationship { get; set; }

        [TestInitialize]
        public void Setup()
        {
            autoMocker = new AutoMocker();
            digitalTwinsClient = new Mock<DigitalTwinsClient>();
            logger = new Mock<ILogger<AasWriteConnectorForAdtCommunication>>();
            repo = new Mock<IAASRepo>();
            mapper = new Mock<IMapper>();

            objectUnderTest =
                new AasWriteConnectorForAdtCommunication(digitalTwinsClient.Object, logger.Object, repo.Object,
                    mapper.Object);

            azureResponseMock = new Mock<Azure.Response<AdtReference>>();
            azureResponseMock.Setup(_ => _.Value).Returns(new AdtReference()
            {
                dtId = "testId1",
                Metadata = new DigitalTwinMetadata { ModelId = "testModelId" },
                Key1 = new AdtKey()
                {
                    Type = "ConceptDescription",
                    Value = "testValue1"
                }
            });

            azureResponseMockRelationship = new Mock<Azure.Response<BasicRelationship>>();
            azureResponseMockRelationship.Setup(_ => _.Value).Returns(new BasicRelationship()
                { Id = "testRelId" });

            mapper.Setup(_ => _.Map<Reference>(It.IsAny<AdtReference>()))
                .Returns(new Reference(ReferenceTypes.GlobalReference, new List<Key>()
                {
                    new Key(KeyTypes.ConceptDescription, "testValue1")
                }));
        }

        [TestMethod]
        public async Task DoCreateOrReplaceDigitalTwinAsync_does_not_try_to_create_twin_if_provided_with_null()
        {
            await objectUnderTest.DoCreateOrReplaceDigitalTwinAsync(null);
            digitalTwinsClient.Verify(_ =>
                _.CreateOrReplaceDigitalTwinAsync(It.IsAny<string>(), It.IsAny<BasicDigitalTwin>(), null, default(CancellationToken)),Times.Never);
        }

        [TestMethod]
        public async Task DoCreateOrReplaceDigitalTwinAsync_just_calls_adtApi()
        {
            var twinData = new BasicDigitalTwin();
            await objectUnderTest.DoCreateOrReplaceDigitalTwinAsync(twinData);
            digitalTwinsClient.Verify(_ =>
                _.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData, null, default(CancellationToken)));
        }

        [TestMethod]
        public async Task DoCreateOrReplaceRelationshipAsync_does_not_try_to_create_relationship_if_sourceId_is_null()
        {
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync(null, "testRelationshipName", "testTargetId");
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null, default(CancellationToken)),Times.Never);
        }

        [TestMethod]
        public async Task DoCreateOrReplaceRelationshipAsync_does_not_try_to_create_relationship_if_sourceId_is_empty()
        {
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync("", "testRelationshipName", "testTargetId");
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null,
                default(CancellationToken)), Times.Never);
        }

        [TestMethod]
        public async Task
            DoCreateOrReplaceRelationshipAsync_does_not_try_to_create_relationship_if_relationshipName_is_null()
        {
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync("testSourceId", null, "testTargetId");
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null,
                default(CancellationToken)), Times.Never);
        }

        [TestMethod]
        public async Task
            DoCreateOrReplaceRelationshipAsync_does_not_try_to_create_relationship_if_relationshipName_is_empty()
        {
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync("testSourceId", "", "testTargetId");
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null,
                default(CancellationToken)), Times.Never);
        }

        [TestMethod]
        public async Task DoCreateOrReplaceRelationshipAsync_does_not_try_to_create_relationship_if_targetId_is_null()
        {
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync("testSourceId", "testRelationshipName", null);
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null,
                default(CancellationToken)), Times.Never);
        }

        [TestMethod]
        public async Task DoCreateOrReplaceRelationshipAsync_does_not_try_to_create_relationship_if_targetId_is_empty()
        {
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync("testSourceId", "testRelationshipName", "");
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null,
                default(CancellationToken)), Times.Never);
        }


        [TestMethod]
        public async Task DoCreateOrReplaceRelationshipAsync_calls_AdtApi_to_create_Relationship_when_input_parameters_ok()
        {
            var testRelationshipName = "testRelName";
            var testTargetId = "testTargetId";
            var expectedRelationshipId = "TestId-testRelName->testTargetId";
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync("TestId", testRelationshipName, testTargetId);
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                "TestId", expectedRelationshipId, It.IsAny<BasicRelationship>(), null, default(CancellationToken)));
        }
    }
}
