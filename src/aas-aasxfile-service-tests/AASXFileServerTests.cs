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
        public void TestCreateAASXPackageSimple()
        {
            AASAASXFile service = CreateAASAASXFileService();
            PackageDescription packageDesc = service.CreateAASXPackage(new List<string> {"first", "second" }, Encoding.UTF8.GetBytes("This is a simple test"), "01_Festo.aasx").GetAwaiter().GetResult();
            Assert.IsNotNull(packageDesc);
            Assert.IsNotNull(packageDesc.PackageId);
        }

        [TestMethod]
        public void TestDeleteAASXByPackageIdSimple()
        {
            AASAASXFile service = CreateAASAASXFileService();
            service.DeleteAASXByPackageId("01_Festo");
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
