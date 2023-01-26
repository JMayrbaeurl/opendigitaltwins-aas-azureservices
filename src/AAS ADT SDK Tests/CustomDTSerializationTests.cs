using AAS.ADT.Models;
using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AAS.ADT.Tests
{
    [TestClass]
    public class ADTAASRepoServiceTests
    {
        private IConfiguration configuration;

        private DigitalTwinsClient dtClient;

        private List<string> listOfAASDtIds = new List<string>();
        private List<string> listOfAssetInfoIds = new List<string>();

        public ADTAASRepoServiceTests()
        {
            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.tests.json").Build();
        }

        [TestInitialize]
        public void Setup()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    ConfigureBasicServices(services, "https://aasadtdevjm.api.weu.digitaltwins.azure.net");
                })
                .Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            this.dtClient = provider.GetService<DigitalTwinsClient>();

            this.readAllDtIds().GetAwaiter().GetResult();
        }

        private async Task readAllDtIds()
        {
            this.listOfAASDtIds.Clear();

            AsyncPageable<BasicDigitalTwin> twins = this.dtClient.QueryAsync<BasicDigitalTwin>("SELECT * FROM digitaltwins WHERE IS_OF_MODEL('dtmi:digitaltwins:aas:AssetAdministrationShell;1')");
            await foreach (BasicDigitalTwin twin in twins)
            {
                this.listOfAASDtIds.Add(twin.Id);
            }

            this.listOfAssetInfoIds.Clear();

            twins = this.dtClient.QueryAsync<BasicDigitalTwin>("SELECT * FROM digitaltwins WHERE IS_OF_MODEL('dtmi:digitaltwins:aas:AssetInformation;1')");
            await foreach (BasicDigitalTwin twin in twins)
            {
                this.listOfAssetInfoIds.Add(twin.Id);
            }
        }

        private static void ConfigureBasicServices(IServiceCollection services, string adtInstanceUrl)
        {
            services.Configure<DigitalTwinsClientOptions>(options => options.ADTEndpoint = new Uri(adtInstanceUrl));

            services.AddAzureClients(builder =>
            {
                builder.AddClient<DigitalTwinsClient, DigitalTwinsClientOptions>((options, provider) =>
                {
                    var appOptions = provider.GetService<IOptions<DigitalTwinsClientOptions>>();

                    var credentials = new DefaultAzureCredential();
                    DigitalTwinsClient client = new DigitalTwinsClient(appOptions.Value.ADTEndpoint,
                                credentials, new Azure.DigitalTwins.Core.DigitalTwinsClientOptions { Transport = new HttpClientTransport(new HttpClient()) });
                    return client;
                });

                // First use DefaultAzureCredentials and second EnvironmentCredential to enable local docker execution
                builder.UseCredential(new ChainedTokenCredential(new DefaultAzureCredential(), new EnvironmentCredential()));
            });
        }

        public class DigitalTwinsClientOptions
        {
            public Uri? ADTEndpoint { get; set; }
        }

        [TestMethod]
        public void TestSetup()
        {
            Assert.IsNotNull(this.dtClient);
        }

        [TestMethod]
        public void TestCustomAASSerializerForAAS()
        {
            Assert.IsTrue(this.listOfAASDtIds.Count > 0);
            
            var aShell = this.dtClient.GetDigitalTwin<AdtAas>(this.listOfAASDtIds[0]);
            Assert.IsNotNull(aShell);
        }

        [TestMethod]
        public void TestCustomAASSerializerForAssetInformation()
        {
            Assert.IsTrue(this.listOfAssetInfoIds.Count > 0);

            AdtAssetInformation anAsset = this.dtClient.GetDigitalTwin<AdtAssetInformation>(this.listOfAssetInfoIds[0]);
            Assert.IsNotNull(anAsset);
        }

        [TestMethod]
        public void TestExportAllShells()
        {
            string dirPath = "C:\\Dev\\ADT\\tests\\exports";

            DirectoryInfo di = new DirectoryInfo(dirPath);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }

            if (this.listOfAASDtIds.Count > 0)
            {
                foreach (var item in this.listOfAASDtIds)
                {
                    var result = this.dtClient.GetDigitalTwin<BasicDigitalTwin>(item);
                    File.WriteAllText($"{dirPath}\\Shell_{result.Value.Id}.json", JsonSerializer.Serialize(result.Value));
                }
            }
            if (this.listOfAssetInfoIds.Count > 0)
            {
                foreach (var item in this.listOfAssetInfoIds)
                {
                    var result = this.dtClient.GetDigitalTwin<BasicDigitalTwin>(item);
                    File.WriteAllText($"{dirPath}\\Asset_{result.Value.Id}.json", JsonSerializer.Serialize(result.Value));
                }
            }
        }
    }
}
