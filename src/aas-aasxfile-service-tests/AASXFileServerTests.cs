using AAS.API.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AAS.API.AASXFile.Tests
{
    [TestClass]
    public class AASXFileServerTests
    {
        private IConfiguration configuration;

        public AASXFileServerTests()
        {
            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.tests.json").Build();
        }

        private BlobServiceClient CreateBlobServiceClient()
        {
            return new BlobServiceClient(new Uri(configuration["AASX_FILESERVICE_BLOBSTORAGEURL"]), new DefaultAzureCredential());
        }

        [TestMethod]
        public void TestSimpleConstruction()
        {
            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logFactory.CreateLogger<AzureBlobAASXFileService>();
            Assert.IsNotNull(logger);

            var config = configuration;
            Assert.IsNotNull(config);

            AzureBlobAASXFileService service = new AzureBlobAASXFileService(CreateBlobServiceClient(),config, logger);
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void TestStoreAASXPackageSimple()
        {
            AASAASXFile service = CreateAASAASXFileService();
            BlobContainerClient container = CreateBlobServiceClient().GetBlobContainerClient("aasxfiles");

            try
            {
                PackageDescription packageDesc = service.StoreAASXPackage(new List<string> { "first", "second" }, Encoding.UTF8.GetBytes("This is a simple test"), "Simple.aasx").GetAwaiter().GetResult();
                Assert.IsNotNull(packageDesc);
                Assert.IsNotNull(packageDesc.PackageId);
                Assert.AreEqual(packageDesc.PackageId, "Simple");

                BlobClient blob = container.GetBlobClient($"{WebUtility.UrlEncode(packageDesc.PackageId)}/Package.aasx");
                Assert.IsTrue(blob.Exists());
                BlobProperties properties = blob.GetProperties();
                Assert.IsTrue(properties.Metadata["package"] != null);
                Assert.IsTrue(properties.Metadata["filename"] != null);
                Assert.AreEqual(properties.Metadata["filename"], "Simple.aasx");
                Assert.AreEqual(properties.Metadata["aasId_1"], "first");
                Assert.AreEqual(properties.Metadata["aasId_2"], "second");
            }
            finally
            {
                BlobClient blob = container.GetBlobClient("Simple/Package.aasx");
                blob.DeleteIfExists();
            }
        }

        [TestMethod]
        public void TestStoreAASXPackageForStdSampleFesto()
        {
            byte[] fileContents = System.IO.File.ReadAllBytes(".\\Resources\\01_Festo.aasx");
            Assert.IsNotNull(fileContents);

            AASAASXFile service = CreateAASAASXFileService();
            PackageDescription packageDesc = service.StoreAASXPackage(new List<string> { "smart.festo.com/demo/aas/1/1/454576463545648365874" }, 
                fileContents, "01_Festo.aasx").GetAwaiter().GetResult();
            Assert.IsNotNull(packageDesc);
            Assert.IsNotNull(packageDesc.PackageId);
        }

        [TestMethod]
        public void TestDeleteAASXByPackageIdSimple()
        {
            BlobContainerClient container = CreateBlobServiceClient().GetBlobContainerClient("aasxfiles");
            BlobClient blob = container.GetBlobClient("Simple/Package.aasx");
            blob.Upload(new BinaryData(Encoding.UTF8.GetBytes("This is a simple test")));

            try { 
                AASAASXFile service = CreateAASAASXFileService();
                service.DeleteAASXByPackageId("Simple").GetAwaiter().GetResult();
                Assert.IsFalse(blob.Exists());
            }
            finally
            {
                blob.DeleteIfExists();
            }
        }

        [TestMethod]
        public void TestGetAASXPackageForSimple()
        {
            BlobContainerClient container = CreateBlobServiceClient().GetBlobContainerClient("aasxfiles");
            BlobClient blob = container.GetBlobClient("Simple/Package.aasx");
            string teststring = "This is a simple test";
            blob.Upload(new BinaryData(Encoding.UTF8.GetBytes(teststring)));

            try
            {
                AASAASXFile service = CreateAASAASXFileService();
                byte[] packageContents = service.GetAASXByPackageId("Simple").GetAwaiter().GetResult().Contents;
                Assert.IsNotNull(packageContents);
                Assert.IsTrue(packageContents.Length > 0);
                Assert.AreEqual<string>(teststring, Encoding.UTF8.GetString(packageContents));
            }
            finally
            {
                blob.DeleteIfExists();
            }
        }

        [TestMethod]
        public void TestGetAASXPackageForStdSampleFesto()
        {
            AASAASXFile service = CreateAASAASXFileService();
            byte[] packageContents = service.GetAASXByPackageId("01_Festo").GetAwaiter().GetResult().Contents;
            Assert.IsNotNull(packageContents);
            Assert.IsTrue(packageContents.Length > 0);
        }

        [TestMethod]
        public void TestUpdateAASXPackageForFilename()
        {
            BlobContainerClient container = CreateBlobServiceClient().GetBlobContainerClient("aasxfiles");
            BlobClient blob = container.GetBlobClient("Simple/Package.aasx");
            string teststring = "This is a simple test";
            blob.Upload(new BinaryData(Encoding.UTF8.GetBytes(teststring)));

            try
            {
                IDictionary<string, string> metaData = blob.GetProperties().Value.Metadata;
                metaData.Add("filename", "Simple.aasx");
                metaData.Add("package", "true");
                metaData.Add("aasId_1", "first");
                metaData.Add("aasId_2", "second");
                blob.SetMetadata(metaData);

                AASAASXFile service = CreateAASAASXFileService();
                PackageDescription packageDesc = service.UpdateAASXPackage("Simple", null, null, "Simple2.aasx").GetAwaiter().GetResult();
                Assert.AreEqual("Simple", packageDesc.PackageId);
                Assert.IsTrue(packageDesc.AasIds.Contains("first") && packageDesc.AasIds.Contains("second"));

                metaData = blob.GetProperties().Value.Metadata;
                Assert.IsTrue(metaData.ContainsKey("filename") && metaData["filename"] == "Simple2.aasx");
            }
            finally
            {
                blob.DeleteIfExists();
            }
        }

        [TestMethod]
        public void TestUpdateAASXPackageForFile()
        {
            BlobContainerClient container = CreateBlobServiceClient().GetBlobContainerClient("aasxfiles");
            BlobClient blob = container.GetBlobClient("Simple/Package.aasx");
            string teststring = "This is a simple test";
            blob.Upload(new BinaryData(Encoding.UTF8.GetBytes(teststring)));

            try
            {
                IDictionary<string, string> metaData = blob.GetProperties().Value.Metadata;
                metaData.Add("filename", "Simple.aasx");
                metaData.Add("package", "true");
                metaData.Add("aasId_1", "first");
                metaData.Add("aasId_2", "second");
                blob.SetMetadata(metaData);

                AASAASXFile service = CreateAASAASXFileService();
                PackageDescription packageDesc = service.UpdateAASXPackage("Simple", null, Encoding.UTF8.GetBytes("Changed contents"), null).GetAwaiter().GetResult();
                Assert.AreEqual("Simple", packageDesc.PackageId);
                Assert.IsTrue(packageDesc.AasIds.Contains("first") && packageDesc.AasIds.Contains("second"));
                BinaryData blobContents = blob.DownloadContent().Value.Content;
                Assert.AreEqual("Changed contents", Encoding.UTF8.GetString(blobContents.ToArray()));
            }
            finally
            {
                blob.DeleteIfExists();
            }
        }

        [TestMethod]
        public void TestUpdateAASXPackageForFileAndAasIds()
        {
            BlobContainerClient container = CreateBlobServiceClient().GetBlobContainerClient("aasxfiles");
            BlobClient blob = container.GetBlobClient("Simple/Package.aasx");
            string teststring = "This is a simple test";
            blob.Upload(new BinaryData(Encoding.UTF8.GetBytes(teststring)));

            try
            {
                IDictionary<string, string> metaData = blob.GetProperties().Value.Metadata;
                metaData.Add("filename", "Simple.aasx");
                metaData.Add("package", "true");
                metaData.Add("aasId_1", "first");
                metaData.Add("aasId_2", "second");
                blob.SetMetadata(metaData);

                AASAASXFile service = CreateAASAASXFileService();
                PackageDescription packageDesc = service.UpdateAASXPackage("Simple", new List<string>() { "third", "fourth"}, Encoding.UTF8.GetBytes("Changed contents"), null).GetAwaiter().GetResult();
                Assert.AreEqual("Simple", packageDesc.PackageId);
                Assert.IsTrue(packageDesc.AasIds.Contains("third") && packageDesc.AasIds.Contains("fourth"));
                BinaryData blobContents = blob.DownloadContent().Value.Content;
                Assert.AreEqual("Changed contents", Encoding.UTF8.GetString(blobContents.ToArray()));
                IDictionary<string, string> newMetaData = blob.GetProperties().Value.Metadata;
                Assert.IsTrue(newMetaData.ContainsKey("aasId_1") && (newMetaData["aasId_1"] == "third" || newMetaData["aasId_1"] == "fourth"));
                Assert.IsTrue(newMetaData.ContainsKey("aasId_2") && (newMetaData["aasId_2"] == "third" || newMetaData["aasId_2"] == "fourth"));
            }
            finally
            {
                blob.DeleteIfExists();
            }
        }

        [TestMethod]
        public void TestGetAllAASXPackageIdsSimple()
        {
            BlobContainerClient container = CreateBlobServiceClient().GetBlobContainerClient("aasxfiles");
            BlobClient blob = container.GetBlobClient("Simple/Package.aasx");
            string teststring = "This is a simple test";
            blob.Upload(new BinaryData(Encoding.UTF8.GetBytes(teststring)));

            try
            {
                IDictionary<string, string> metaData = blob.GetProperties().Value.Metadata;
                metaData.Add("filename", "Simple.aasx");
                metaData.Add("package", "true");
                metaData.Add("aasId_1", "first");
                metaData.Add("aasId_2", "second");
                blob.SetMetadata(metaData);

                AASAASXFile service = CreateAASAASXFileService();
                List< PackageDescription> packDescs = service.GetAllAASXPackageIds("first").GetAwaiter().GetResult();
                Assert.IsNotNull(packDescs);
                Assert.IsTrue(packDescs.Count == 1);
                Assert.IsTrue(packDescs[0].PackageId == "Simple");
                Assert.IsTrue(packDescs[0].AasIds[0] == "first");
            }
            finally
            {
                blob.DeleteIfExists();
            }
        }

        private AASAASXFile CreateAASAASXFileService(string uri = null)
        {
            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logFactory.CreateLogger<AzureBlobAASXFileService>();

            AzureBlobAASXFileService service = new AzureBlobAASXFileService(
                new BlobServiceClient(new Uri(uri != null ? uri : configuration["AASX_FILESERVICE_BLOBSTORAGEURL"]), new DefaultAzureCredential()),
                configuration, logger);

            return service;
        }
    }
}
