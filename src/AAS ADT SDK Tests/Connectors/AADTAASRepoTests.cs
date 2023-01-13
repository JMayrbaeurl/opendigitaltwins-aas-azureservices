using AasCore.Aas3_0_RC02;
using Azure;
using Newtonsoft.Json.Linq;

namespace AAS.ADT.Tests
{
    using Azure.DigitalTwins.Core;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.AutoMock;
    using FluentAssertions;

    [TestClass]
    public class AADTAASRepoTests
    {
        public AutoMocker autoMocker { get; set; }

        private Mock<DigitalTwinsClient> digitalTwinsClient { get; set; }
        private ADTAASRepo objectUnderTest { get; set; }

        private Mock<ILogger<ADTAASRepo>> logger { get; set; }
        private AsyncPageable<BasicDigitalTwin> pageableWithExampleTwin { get; set; }
        private AsyncPageable<BasicDigitalTwin> pageableWithThreeExampleTwins { get; set; }


        private AsyncPageable<BasicDigitalTwin> emptyPageable { get; set; }
        private BasicDigitalTwin exampleTwin { get; set; }
        private BasicDigitalTwin exampleTwin2 { get; set; }


        [TestInitialize]
        public void Setup()
        {
            autoMocker = new AutoMocker();
            digitalTwinsClient = new Mock<DigitalTwinsClient>();
            logger = new Mock<ILogger<ADTAASRepo>>();
            objectUnderTest = new ADTAASRepo(digitalTwinsClient.Object, logger.Object);
            exampleTwin = new BasicDigitalTwin { Id = "1234" };
            exampleTwin2 = new BasicDigitalTwin { Id = "5678" };
            Page<BasicDigitalTwin> page = Page<BasicDigitalTwin>.FromValues(new[]
            {
                exampleTwin
            }, null, Mock.Of<Response>());
            Page<BasicDigitalTwin> page2 = Page<BasicDigitalTwin>.FromValues(new[]
            {
                exampleTwin2
            }, null, Mock.Of<Response>());

            pageableWithExampleTwin = AsyncPageable<BasicDigitalTwin>.FromPages(new[] { page });
            pageableWithThreeExampleTwins = AsyncPageable<BasicDigitalTwin>.FromPages(new[] { page, page, page2 });
            emptyPageable = AsyncPageable<BasicDigitalTwin>.FromPages(new Page<BasicDigitalTwin>[] { });
        }


        [TestMethod]
        public async Task FindLinkedReferences_excludes_GlobalReferences_and_FragmentedReferences()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(pageableWithExampleTwin);
            await objectUnderTest.FindLinkedReferences();

            var expectedAdtQuery =
                "SELECT * FROM digitaltwins where is_of_model('dtmi:digitaltwins:aas:Reference;1') " +
                "and key1.type != 'GlobalReference' and key1.type != 'FragmentReference'";
            digitalTwinsClient.Verify(_ =>
                _.QueryAsync<BasicDigitalTwin>(expectedAdtQuery, default(CancellationToken)));
        }

        [TestMethod]
        public async Task FindLinkedReferences_returns_list_of_ReferenceIds()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(pageableWithExampleTwin);
            var actualResult = await objectUnderTest.FindLinkedReferences();
            var expectedResult = new List<string>() { "1234" };
            actualResult.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public async Task FindReferenceElements_excludes_GlobalReferences_and_FragmentedReferences()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(pageableWithExampleTwin);
            await objectUnderTest.FindReferenceElements();

            var expectedAdtQuery =
                "SELECT * FROM digitaltwins where is_of_model('dtmi:digitaltwins:aas:ReferenceElement;1') " +
                "and key1.type != 'GlobalReference' and key1.type != 'FragmentReference'";
            digitalTwinsClient.Verify(_ =>
                _.QueryAsync<BasicDigitalTwin>(expectedAdtQuery, default(CancellationToken)));
        }

        [TestMethod]
        public async Task FindReferenceElements_returns_list_of_ReferenceElementIds()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(pageableWithExampleTwin);
            var actualResult = await objectUnderTest.FindReferenceElements();
            var expectedResult = new List<string>() { "1234" };
            actualResult.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public async Task FindTwinForReference_throws_ArgumentNullException_if_Reference_is_null()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await objectUnderTest.FindTwinForReference(null));
        }

        [TestMethod]
        public async Task FindTwinForReference_throws_ArgumentNullException_if_Reference_contains_no_keys()
        {
            var refWithNoKeys = new Reference(ReferenceTypes.GlobalReference, null, null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await objectUnderTest.FindTwinForReference(refWithNoKeys));
        }

        [TestMethod]
        public async Task FindTwinForReference_throws_ArgumentException_if_Reference_is_not_for_Identifiable()
        {
            var nonIdentifiableKeyType = KeyTypes.GlobalReference;
            var referenceWithKeyToNonIdentifiable = new Reference(
                ReferenceTypes.GlobalReference, new List<Key>() { new Key(nonIdentifiableKeyType, "test") }, null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await objectUnderTest.FindTwinForReference(referenceWithKeyToNonIdentifiable));
        }

        [TestMethod]
        public async Task FindTwinForReference_returns_null_if_no_identifiable_twin_exists()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(emptyPageable);
            var identifiableKeyType = KeyTypes.AssetAdministrationShell;
            var referenceWithKeyToNonIdentifiable = new Reference(
                ReferenceTypes.GlobalReference, new List<Key>() { new Key(identifiableKeyType, "test") }, null);

            var actualResult = await objectUnderTest.FindTwinForReference(referenceWithKeyToNonIdentifiable);
            actualResult.Should().BeNull();
        }

        [TestMethod]
        public async Task FindTwinForReference_returns_correct_Identifiable_dtId_if_reference_just_has_one_key()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(pageableWithExampleTwin);
            var identifiableKeyType = KeyTypes.AssetAdministrationShell;
            var referenceWithKeyToNonIdentifiable = new Reference(
                ReferenceTypes.GlobalReference, new List<Key>() { new Key(identifiableKeyType, "test") }, null);
            var actual = await objectUnderTest.FindTwinForReference(referenceWithKeyToNonIdentifiable);
            actual.Should().Be(exampleTwin.Id);
        }

        [TestMethod]
        public async Task
            FindTwinForReference_returns_correct_Referable_dtId_if_reference_has_more_than_one_key_and_Referable_exists()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(pageableWithThreeExampleTwins);

            var queryStringToQueryAllReferables =
                "SELECT referable2 FROM DIGITALTWINS MATCH(identifiable)-[]->(referable1)-[]->(referable2) " +
                "WHERE identifiable.$dtId = '1234' AND " +
                "referable1.idShort = 'test' AND IS_OF_MODEL(referable1, 'dtmi:digitaltwins:aas:SubmodelElement;1') " +
                "AND referable2.idShort = 'test2' AND IS_OF_MODEL(referable2, 'dtmi:digitaltwins:aas:SubmodelElement;1')";

            Page<BasicDigitalTwin> page2 = Page<BasicDigitalTwin>.FromValues(new[]
            {
                new BasicDigitalTwin
                {
                    Id = "1234",
                    ETag = null,
                    Metadata = null,
                    Contents = new Dictionary<string, object>()
                    {
                        { "referable2", "{\"$dtId\": \"5678\"}" }
                    }
                }
            }, null, Mock.Of<Response>());

            var pageableReferables = AsyncPageable<BasicDigitalTwin>.FromPages(new[] { page2 });

            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(queryStringToQueryAllReferables, default(CancellationToken)))
                .Returns(pageableReferables);
            var identifiableKeyType = KeyTypes.AssetAdministrationShell;
            var referableKeyType = KeyTypes.SubmodelElement;

            var referenceWithKeyToNonIdentifiable = new Reference(
                ReferenceTypes.GlobalReference, new List<Key>()
                {
                    new Key(identifiableKeyType, "test"),
                    new Key(referableKeyType, "test"),
                    new Key(referableKeyType, "test2")
                }, null);
            var actual = await objectUnderTest.FindTwinForReference(referenceWithKeyToNonIdentifiable);
            actual.Should().Be(exampleTwin2.Id);
        }

        [TestMethod]
        public async Task FindTwinForReference_returns_null_if_reference_has_more_than_one_key_and_Referable_does_not_exists()
        {
            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(It.IsAny<string>(), default(CancellationToken)))
                .Returns(pageableWithThreeExampleTwins);

            var queryStringToQueryAllReferables =
                "SELECT referable2 FROM DIGITALTWINS MATCH(identifiable)-[]->(referable1)-[]->(referable2) " +
                "WHERE identifiable.$dtId = '1234' AND " +
                "referable1.idShort = 'test' AND IS_OF_MODEL(referable1, 'dtmi:digitaltwins:aas:SubmodelElement;1') " +
                "AND referable2.idShort = 'test2' AND IS_OF_MODEL(referable2, 'dtmi:digitaltwins:aas:SubmodelElement;1')";

            digitalTwinsClient.Setup(
                    _ => _.QueryAsync<BasicDigitalTwin>(queryStringToQueryAllReferables, default(CancellationToken)))
                .Returns(emptyPageable);
            var identifiableKeyType = KeyTypes.AssetAdministrationShell;
            var referableKeyType = KeyTypes.SubmodelElement;

            var referenceWithKeyToNonIdentifiable = new Reference(
                ReferenceTypes.GlobalReference, new List<Key>()
                {
                    new Key(identifiableKeyType, "test"),
                    new Key(referableKeyType, "test"),
                    new Key(referableKeyType, "test2")
                }, null);
            var actual = await objectUnderTest.FindTwinForReference(referenceWithKeyToNonIdentifiable);
            actual.Should().BeNull();
        }
    }
}
