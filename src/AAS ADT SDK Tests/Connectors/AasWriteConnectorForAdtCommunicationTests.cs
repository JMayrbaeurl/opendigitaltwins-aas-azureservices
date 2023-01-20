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
        public async Task DoCreateOrReplaceDigitalTwinAsync_just_calls_adtApi()
        {
            var twinData = new BasicDigitalTwin();
            await objectUnderTest.DoCreateOrReplaceDigitalTwinAsync(twinData);
            digitalTwinsClient.Verify(_ =>
                _.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData, null, default(CancellationToken)));
        }

        [TestMethod]
        public async Task DoCreateOrReplaceRelationshipAsync_calls_AdtApi_to_create_Relationship()
        {
            var testRelationshipName = "testRelName";
            var testTargetId = "testTargetId";
            var expectedRelationshipId = "TestId-testRelName->testTargetId";
            await objectUnderTest.DoCreateOrReplaceRelationshipAsync("TestId", testRelationshipName, testTargetId);
            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync(
                "TestId", expectedRelationshipId, It.IsAny<BasicRelationship>(), null, default(CancellationToken)));
        }

        [TestMethod]
        public async Task
            DoCreateReferrableReferenceRelationships_throws_returns_empty_results_if_refList_is_null_or_empty()
        {
            var actual1 = await objectUnderTest.DoCreateReferrableReferenceRelationships(
                null, null, new List<string>());
            var actual2 = await objectUnderTest.DoCreateReferrableReferenceRelationships(
                new List<string>(), null, new List<string>());
            actual1.Should().BeEmpty();
            actual2.Should().BeEmpty();
        }

        [TestMethod]
        public async Task DoCreateReferrableReferenceRelationships_creates_Relationship_when_everything_ok()
        {
            digitalTwinsClient.Setup(_ => _.GetDigitalTwinAsync<AdtReference>("testId1", default(CancellationToken)))
                .ReturnsAsync(azureResponseMock.Object);

            repo.Setup(_ => _.FindTwinForReference(It.IsAny<Reference>())).Returns(Task.FromResult("testId2"));

            var targetId = "testId2";
            var relName = "testId1-referredElement->testId2";

            digitalTwinsClient.Setup(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null,
                default(CancellationToken))).ReturnsAsync(azureResponseMockRelationship.Object);

            await objectUnderTest.DoCreateReferrableReferenceRelationships(
                new List<string>() { "testId1" }, null, new List<string>());

            digitalTwinsClient.Verify(_ =>
                _.GetDigitalTwinAsync<AdtReference>("testId1", default(CancellationToken)));

            digitalTwinsClient.Verify(_ => _.CreateOrReplaceRelationshipAsync("testId1", relName,
                It.IsAny<BasicRelationship>(), null, default(CancellationToken)));

        }

        [TestMethod]
        public async Task
            DoCreateReferrableReferenceRelationships_creates_No_Relationship_when_filtered_twins_does_not_contain_entry_from_refList()
        {
            var actual = await objectUnderTest.DoCreateReferrableReferenceRelationships(
                new List<string>() { "testId1" }, new HashSet<string>(), new List<string>());
            actual.Should().BeEmpty();
        }

        [TestMethod]
        public async Task
            DoCreateReferrableReferenceRelationships_creates_Relationship_when_filtered_twins_contains_one_entry_from_refList()
        {
            digitalTwinsClient.Setup(_ => _.GetDigitalTwinAsync<AdtReference>("testId1", default(CancellationToken)))
                .ReturnsAsync(azureResponseMock.Object);

            repo.Setup(_ => _.FindTwinForReference(It.IsAny<Reference>())).Returns(Task.FromResult("testId2"));

            var targetId = "testId2";
            var relName = "testId1-referredElement->testId2";

            digitalTwinsClient.Setup(_ => _.CreateOrReplaceRelationshipAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BasicRelationship>(), null,
                default(CancellationToken))).ReturnsAsync(azureResponseMockRelationship.Object);

            var actual = await objectUnderTest.DoCreateReferrableReferenceRelationships(
                new List<string>() { "testId1" }, new HashSet<string>() { "testId1" }, new List<string>());
            actual.Should().BeEquivalentTo(new List<string>() { "testId1" });
        }
    }
}
