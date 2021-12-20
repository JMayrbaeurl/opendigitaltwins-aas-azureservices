using AAS.API.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AAS.API.AASXFile.Tests
{
    [TestClass]
    public class AASXFileServerTests
    {
        [TestMethod]
        public void TestConnectToBlobService()
        {
            BlobServiceClient client = new BlobServiceClient(new Uri("https://aasxstoragejm.blob.core.windows.net/"), new DefaultAzureCredential());
            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void TestSimpleConstruction()
        {
            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logFactory.CreateLogger<AzureBlobAASXFileService>();
            Assert.IsNotNull(logger);

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Assert.IsNotNull(config);

            AzureBlobAASXFileService service = new AzureBlobAASXFileService(
                new BlobServiceClient(new Uri("https://aasxstoragejm.blob.core.windows.net/"), new DefaultAzureCredential()),
                config, logger);
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void TestStoreAASXPackageSimple()
        {
            AASAASXFile service = CreateAASAASXFileService();
            PackageDescription packageDesc = service.StoreAASXPackage(new List<string> {"first", "second" }, Encoding.UTF8.GetBytes("This is a simple test"), "Simple.aasx").GetAwaiter().GetResult();
            Assert.IsNotNull(packageDesc);
            Assert.IsNotNull(packageDesc.PackageId);
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
            AASAASXFile service = CreateAASAASXFileService();
            service.DeleteAASXByPackageId("Simple").GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestGetAASXPackageForStdSampleFesto()
        {
            AASAASXFile service = CreateAASAASXFileService();
            byte[] packageContents = service.GetAASXByPackageId("01_Festo").GetAwaiter().GetResult();
            Assert.IsNotNull(packageContents);
            Assert.IsTrue(packageContents.Length > 0);
        }

        private AASAASXFile CreateAASAASXFileService(string uri = "https://aasxstoragejm.blob.core.windows.net/")
        {
            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logFactory.CreateLogger<AzureBlobAASXFileService>();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            AzureBlobAASXFileService service = new AzureBlobAASXFileService(
                new BlobServiceClient(new Uri(uri), new DefaultAzureCredential()),
                config, logger);

            return service;
        }
    }
}
