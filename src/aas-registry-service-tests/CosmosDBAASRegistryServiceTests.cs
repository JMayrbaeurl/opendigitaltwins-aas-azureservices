using AAS.API.Models;
using AAS.API.Registry.CosmosDBImpl;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            CosmosClient client = new CosmosClient(account, key);

            return client;
        }

        [TestMethod]
        public void TestDISetup()
        {
            Assert.IsNotNull(registryService);
            Assert.IsNotNull(dbClient);
        }

        [TestMethod]
        public void TestSimpleGetAASDesc()
        {
            var aasDesc = registryService.GetAssetAdministrationShellDescriptorById("https://example.org/aas/motor").GetAwaiter().GetResult();
            Assert.IsNotNull(aasDesc);
        }
    }
}
