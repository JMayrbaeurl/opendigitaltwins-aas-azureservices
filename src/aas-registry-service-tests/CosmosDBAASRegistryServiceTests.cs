using AAS.API.Models;
using AAS.API.Registry.CosmosDBImpl;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AAS.API.Registry.Tests
{
    [TestClass]
    public class CosmosDBAASRegistryServiceTests
    {
        private IConfiguration configuration;

        private AASRegistry registryService;

        private CosmosClient dbClient;

        public CosmosDBAASRegistryServiceTests()
        {
            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.tests.json").Build();
        }

        [TestInitialize]
        public void Setup()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                { configBuilder.AddJsonFile("appsettings.tests.json").AddUserSecrets<CosmosDBAASRegistryServiceTests>(); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<CosmosClient>(InitializeCosmosClientInstance(hostContext.Configuration.GetSection("CosmosDb")));
                    services.AddSingleton<AASRegistry, CosmosDBAASRegistry>();
                    configuration = hostContext.Configuration;
                }).UseConsoleLifetime();

            var host = builder.Build();

            registryService = host.Services.GetService<AASRegistry>();
            dbClient = host.Services.GetService<CosmosClient>();
        }

        /// <summary>
        /// Creates a Cosmos DB database and a container with the specified partition key. 
        /// </summary>
        /// <returns></returns>
        private static CosmosClient InitializeCosmosClientInstance(IConfigurationSection configurationSection)
        {
            string account = configurationSection.GetSection("Account").Value;
            string key = configurationSection.GetSection("Key").Value;
            CosmosClient client = new CosmosClient(account, key,
                new CosmosClientOptions() { SerializerOptions = new CosmosSerializationOptions() { IgnoreNullValues = true } });

            return client;
        }

        [TestMethod]
        public void TestDISetup()
        {
            Assert.IsNotNull(registryService);
            Assert.IsNotNull(dbClient);
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001AASDesc.json")]
        public void TestCreateShellDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001AASDesc.json"));

            AssetAdministrationShellDescriptor aasDesc = JsonConvert.DeserializeObject<AssetAdministrationShellDescriptor>(
                File.ReadAllText("test001AASDesc.json"));
            Assert.IsNotNull(aasDesc);

            try
            {
                Assert.IsNotNull(registryService.CreateAssetAdministrationShellDescriptor(aasDesc).GetAwaiter().GetResult());
                Assert.IsNull(registryService.CreateAssetAdministrationShellDescriptor(aasDesc).GetAwaiter().GetResult());
            }
            finally
            {
                try
                {
                    DeleteShellDescriptor(aasDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001AASDesc.json")]
        public void TestReadShellDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001AASDesc.json"));

            AssetAdministrationShellDescriptor aasDesc = JsonConvert.DeserializeObject<AssetAdministrationShellDescriptor>(
                File.ReadAllText("test001AASDesc.json"));
            Assert.IsNotNull(aasDesc);

            ShellsContainer().CreateItemAsync<DBAssetAdministrationShellDescriptor>(
                new DBAssetAdministrationShellDescriptor(aasDesc), new PartitionKey(aasDesc.Identification)).GetAwaiter().GetResult();
            try
            {
                Assert.IsNotNull(registryService.GetAssetAdministrationShellDescriptorById(aasDesc.Identification).GetAwaiter().GetResult());
            } finally
            {
                try
                {
                    DeleteShellDescriptor(aasDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        public void TestReadShellDescriptorForNonExisting()
        {
            Assert.IsNull(registryService.GetAssetAdministrationShellDescriptorById("Den gibt es wirklich nicht").GetAwaiter().GetResult());
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\testAASDescs.json")]
        public void TestReadAllShellDescriptors()
        {
            Assert.IsTrue(File.Exists("testAASDescs.json"));

            var aasDescs = JsonConvert.DeserializeObject<List<AssetAdministrationShellDescriptor>>(
                File.ReadAllText("testAASDescs.json"));
            Assert.IsNotNull(aasDescs);

            try
            {
                foreach (var aasDesc in aasDescs)
                {
                    ShellsContainer().CreateItemAsync<DBAssetAdministrationShellDescriptor>(
                        new DBAssetAdministrationShellDescriptor(aasDesc), new PartitionKey(aasDesc.Identification)).GetAwaiter().GetResult();
                }

                var readDescs = registryService.GetAllAssetAdministrationShellDescriptors().GetAwaiter().GetResult();
                Assert.IsNotNull(readDescs);
                Assert.IsTrue(readDescs.Count >= 3);
            } finally
            {
                try
                {
                    foreach (var aasDesc in aasDescs)
                    {
                        DeleteShellDescriptor(aasDesc.Identification).GetAwaiter().GetResult();
                    }
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001AASDesc.json")]
        public void TestUpdateShellDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001AASDesc.json"));

            AssetAdministrationShellDescriptor aasDesc = JsonConvert.DeserializeObject<AssetAdministrationShellDescriptor>(
                File.ReadAllText("test001AASDesc.json"));
            Assert.IsNotNull(aasDesc);

            ShellsContainer().CreateItemAsync<DBAssetAdministrationShellDescriptor>(
                new DBAssetAdministrationShellDescriptor(aasDesc), new PartitionKey(aasDesc.Identification)).GetAwaiter().GetResult();
            try
            {
                aasDesc.IdShort = "ChangedIdShortValue";
                Assert.IsNotNull(registryService.UpdateAssetAdministrationShellDescriptorById(aasDesc).GetAwaiter().GetResult());

                DBAssetAdministrationShellDescriptor readDesc = ShellsContainer().ReadItemAsync<DBAssetAdministrationShellDescriptor>(
                    DBAssetAdministrationShellDescriptor.CreateDocumentId(aasDesc.Identification),
                    new PartitionKey(aasDesc.Identification)).GetAwaiter().GetResult();

                Assert.AreEqual("ChangedIdShortValue", readDesc.Desc.IdShort);
            }
            finally
            {
                try
                {
                    DeleteShellDescriptor(aasDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001AASDesc.json")]
        public void TestUpdateShellDescriptorForNonExisting()
        {
            Assert.IsTrue(File.Exists("test001AASDesc.json"));

            AssetAdministrationShellDescriptor aasDesc = JsonConvert.DeserializeObject<AssetAdministrationShellDescriptor>(
                File.ReadAllText("test001AASDesc.json"));
            Assert.IsNotNull(aasDesc);

            aasDesc.Identification = "Den gibt es wirklich nicht";

            Assert.IsNull(registryService.UpdateAssetAdministrationShellDescriptorById(aasDesc).GetAwaiter().GetResult());
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001AASDesc.json")]
        public void TestDeleteShellDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001AASDesc.json"));

            AssetAdministrationShellDescriptor aasDesc = JsonConvert.DeserializeObject<AssetAdministrationShellDescriptor>(
                File.ReadAllText("test001AASDesc.json"));
            Assert.IsNotNull(aasDesc);

            ShellsContainer().CreateItemAsync<DBAssetAdministrationShellDescriptor>(
                new DBAssetAdministrationShellDescriptor(aasDesc), new PartitionKey(aasDesc.Identification)).GetAwaiter().GetResult();
            try
            {
                Assert.IsTrue(registryService.DeleteAssetAdministrationShellDescriptorById(
                    aasDesc.Identification).GetAwaiter().GetResult());
            }
            finally
            {
                try
                {
                    DeleteShellDescriptor(aasDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        public void TestDeleteShellDescriptorForNonExisting()
        {
            Assert.IsFalse(registryService.DeleteAssetAdministrationShellDescriptorById("Den gibt es wirklich nicht").GetAwaiter().GetResult());
        }

        [TestMethod]
        [ExpectedException(typeof(AASRegistryException))]
        public void TestDeleteShellDescriptorWithNullParam()
        {
            registryService.DeleteAssetAdministrationShellDescriptorById(null).GetAwaiter().GetResult();
        }

        private Container ShellsContainer()
        {
            return dbClient.GetContainer(CosmosDBAASRegistry.AASREGISTRYDBNAME, CosmosDBAASRegistry.SHELLSCONTAINERNAME);
        }

        private async Task DeleteShellDescriptor(string aasId)
        {
            await ShellsContainer().DeleteItemAsync<DBAssetAdministrationShellDescriptor>(
                        DBAssetAdministrationShellDescriptor.CreateDocumentId(aasId), new PartitionKey(aasId));
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001SubmodelDesc.json")]
        public void TestCreateSubmodelDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001SubmodelDesc.json"));

            SubmodelDescriptor submodelDesc = JsonConvert.DeserializeObject<SubmodelDescriptor>(
                File.ReadAllText("test001SubmodelDesc.json"));
            Assert.IsNotNull(submodelDesc);

            //string fullDBdoc = JsonConvert.SerializeObject(new DBSubmodelDescriptor(desc));
            //Assert.IsNotNull(fullDBdoc);

            try
            {
                Assert.IsNotNull(registryService.CreateSubmodelDescriptor(submodelDesc).GetAwaiter().GetResult());
                Assert.IsNull(registryService.CreateSubmodelDescriptor(submodelDesc).GetAwaiter().GetResult());
            }
            finally
            {
                try
                {
                    DeleteSubmodelDescriptor(submodelDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001SubmodelDesc.json")]
        public void TestReadSubmodelDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001SubmodelDesc.json"));

            SubmodelDescriptor submodelDesc = JsonConvert.DeserializeObject<SubmodelDescriptor>(
                File.ReadAllText("test001SubmodelDesc.json"));
            Assert.IsNotNull(submodelDesc);

            
            SubmodelsContainer().CreateItemAsync<DBSubmodelDescriptor>(
                new DBSubmodelDescriptor(submodelDesc), new PartitionKey(submodelDesc.Identification)).GetAwaiter().GetResult(); 
            try
            {
                Assert.IsNotNull(registryService.GetSubmodelDescriptorById(submodelDesc.Identification).GetAwaiter().GetResult());
            }
            finally
            {
                try
                {
                    DeleteSubmodelDescriptor(submodelDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001SubmodelDesc.json")]
        public void TestDeleteSubmodelDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001SubmodelDesc.json"));

            SubmodelDescriptor submodelDesc = JsonConvert.DeserializeObject<SubmodelDescriptor>(
                File.ReadAllText("test001SubmodelDesc.json"));
            Assert.IsNotNull(submodelDesc);

            SubmodelsContainer().CreateItemAsync<DBSubmodelDescriptor>(
                new DBSubmodelDescriptor(submodelDesc), new PartitionKey(submodelDesc.Identification)).GetAwaiter().GetResult();
            try
            {
                Assert.IsTrue(registryService.DeleteSubmodelDescriptorById(
                    submodelDesc.Identification).GetAwaiter().GetResult());
            }
            finally
            {
                try
                {
                    DeleteSubmodelDescriptor(submodelDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        public void TestDeleteSubmodelDescriptorForNonExisting()
        {
            Assert.IsFalse(registryService.DeleteSubmodelDescriptorById("Den gibt es wirklich nicht").GetAwaiter().GetResult());
        }

        [TestMethod]
        public void TestReadSubmodelDescriptorForNonExisting()
        {
            Assert.IsNull(registryService.GetSubmodelDescriptorById("Den gibt es wirklich nicht").GetAwaiter().GetResult());
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\testSubmodelDescs.json")]
        public void TestReadAllSubmodelDescriptors()
        {
            Assert.IsTrue(File.Exists("testSubmodelDescs.json"));

            var submodelDescs = JsonConvert.DeserializeObject<List<SubmodelDescriptor>>(
                File.ReadAllText("testSubmodelDescs.json"));
            Assert.IsNotNull(submodelDescs);

            try
            {
                foreach (var submodelDesc in submodelDescs)
                {
                    SubmodelsContainer().CreateItemAsync<DBSubmodelDescriptor>(
                        new DBSubmodelDescriptor(submodelDesc), new PartitionKey(submodelDesc.Identification)).GetAwaiter().GetResult();
                }

                var readDescs = registryService.GetAllSubmodelDescriptors().GetAwaiter().GetResult();
                Assert.IsNotNull(readDescs);
                Assert.IsTrue(readDescs.Count >= 3);
            }
            finally
            {
                try
                {
                    foreach (var submodelDesc in submodelDescs)
                    {
                        DeleteSubmodelDescriptor(submodelDesc.Identification).GetAwaiter().GetResult();
                    }
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001SubmodelDesc.json")]
        public void TestUpdateSubmodelDescriptorForTest001()
        {
            Assert.IsTrue(File.Exists("test001SubmodelDesc.json"));

            SubmodelDescriptor submodelDesc = JsonConvert.DeserializeObject<SubmodelDescriptor>(
                File.ReadAllText("test001SubmodelDesc.json"));
            Assert.IsNotNull(submodelDesc);

            SubmodelsContainer().CreateItemAsync<DBSubmodelDescriptor>(
                new DBSubmodelDescriptor(submodelDesc), new PartitionKey(submodelDesc.Identification)).GetAwaiter().GetResult();
            try
            {
                submodelDesc.IdShort = "ChangedIdShortValue";
                Assert.IsNotNull(registryService.UpdateSubmodelDescriptorById(submodelDesc).GetAwaiter().GetResult());

                DBSubmodelDescriptor readDesc = SubmodelsContainer().ReadItemAsync<DBSubmodelDescriptor>(
                    DBSubmodelDescriptor.CreateDocumentId(submodelDesc.Identification),
                    new PartitionKey(submodelDesc.Identification)).GetAwaiter().GetResult();

                Assert.AreEqual("ChangedIdShortValue", readDesc.Desc.IdShort);
            }
            finally
            {
                try
                {
                    DeleteSubmodelDescriptor(submodelDesc.Identification).GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        [DeploymentItem("Descriptor samples\\test001SubmodelDesc.json")]
        public void TestUpdateSubmodelDescriptorForNonExisting()
        {
            Assert.IsTrue(File.Exists("test001SubmodelDesc.json"));

            SubmodelDescriptor submodelDesc = JsonConvert.DeserializeObject<SubmodelDescriptor>(
                File.ReadAllText("test001SubmodelDesc.json"));
            Assert.IsNotNull(submodelDesc);

            submodelDesc.Identification = "Den gibt es wirklich nicht";

            Assert.IsNull(registryService.UpdateSubmodelDescriptorById(submodelDesc).GetAwaiter().GetResult());
        }

        private Container SubmodelsContainer()
        {
            return dbClient.GetContainer(CosmosDBAASRegistry.AASREGISTRYDBNAME, CosmosDBAASRegistry.SUBMODELSCONTAINERNAME);
        }

        private async Task DeleteSubmodelDescriptor(string submodelId)
        {
            await SubmodelsContainer().DeleteItemAsync<DBSubmodelDescriptor>(
                        DBSubmodelDescriptor.CreateDocumentId(submodelId), new PartitionKey(submodelId));
        }
    }
}
