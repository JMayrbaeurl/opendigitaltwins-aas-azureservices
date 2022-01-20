using AAS.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace AAS.API.Registry.Tests
{
    [TestClass]
    public class AASRegistryServiceTests
    {
        private IConfiguration configuration;

        private IDistributedCache cache;

        private AASRegistry registryService;

        public AASRegistryServiceTests()
        {
            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.tests.json").Build();
        }

        [TestInitialize]
        public void Setup()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) => 
                    { configBuilder.AddJsonFile("appsettings.tests.json").AddUserSecrets<AASRegistryServiceTests>(); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddStackExchangeRedisCache(setupAction =>
                    {
                        setupAction.Configuration = hostContext.Configuration.GetConnectionString("RedisCache");
                    });
                    services.AddSingleton<AASRegistry, RedisAASRegistry>();
                    configuration = hostContext.Configuration;
                }).UseConsoleLifetime();

            var host = builder.Build();

            registryService = host.Services.GetService<AASRegistry>();
            cache = host.Services.GetService<IDistributedCache>();
        }

        [TestMethod]
        public void TestDISetup()
        {
            Assert.IsNotNull(registryService);
        }

        [TestMethod]
        public void TestCreateEntryForFesto01()
        {
            AssetAdministrationShellDescriptor aasDesc = CreateAASDescriptorForFesto01();

            try
            {
                AssetAdministrationShellDescriptor result = registryService.CreateAssetAdministrationShellDescriptor(aasDesc).GetAwaiter().GetResult();
                Assert.IsNotNull(result);
                Assert.AreEqual(aasDesc, result);

                Console.WriteLine(cache.GetString($"aas_{aasDesc.Identification}"));
            }
            finally
            {
                try
                {
                    cache.RemoveAsync($"aas_{aasDesc.Identification}").GetAwaiter().GetResult();
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        public void TestDeleteEntryForDemo()
        {
            string key = Guid.NewGuid().ToString();
            cache.SetString($"aas_{key}", "Test");

            try
            {
                registryService.DeleteAssetAdministrationShellDescriptorById(key).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                try
                {
                    cache.RemoveAsync($"aas_{key}").GetAwaiter().GetResult();
                }
                catch (Exception) { }

                throw ex;
            }
        }

        private AssetAdministrationShellDescriptor CreateAASDescriptorForFesto01()
        {
            AssetAdministrationShellDescriptor result = new AssetAdministrationShellDescriptor();

            result.Identification = "test_smart.festo.com/demo/aas/1/1/454576463545648365874";
            result.IdShort = "Festo_3S7PM0CP4BD";
            result.SubmodelDescriptors = new List<SubmodelDescriptor>();
            result.SubmodelDescriptors.Add(new SubmodelDescriptor()
            {
                IdShort = "Nameplate",
                Identification = "www.company.com/ids/sm/4343_5072_7091_3242",
                SemanticId = new GlobalReference() { Value = new List<string>() { "https://www.hsu-hh.de/aut/aas/nameplate" } }
            });
            result.Endpoints = new List<Endpoint>();
            result.Endpoints.Add(new Endpoint() 
            { _Interface = "AAS-1.0", 
              ProtocolInformation = new ProtocolInformation() { EndpointAddress = "https://hack2021aasapi.azurewebsites.net", EndpointProtocolVersion = "1.1"} });

            return result;
        }
    }
}
