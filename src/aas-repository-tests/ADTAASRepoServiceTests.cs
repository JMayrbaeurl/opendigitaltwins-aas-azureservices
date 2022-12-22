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
using System.Net.Http;

namespace AAS.API.Repository.Tests
{
    [TestClass]
    public class ADTAASRepoServiceTests
    {
        private IConfiguration configuration;

        private AASRepository aasRepo;

        private DigitalTwinsClient dtClient;

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
                    ConfigureBasicServices(services, "https://aasadtjmstaging.api.weu.digitaltwins.azure.net");

                    services.AddSingleton<AASRepository, ADTAASRepository>();
                })
                .Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            this.aasRepo = provider.GetRequiredService<AASRepository>();
            this.dtClient = provider.GetService<DigitalTwinsClient>();
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
            public Uri ADTEndpoint { get; set; }
        }

        [TestMethod]
        public void TestSetup()
        {
            Assert.IsNotNull(this.aasRepo);
            Assert.IsNotNull(this.dtClient);
        }

        [TestMethod]
        public void TestCustomAASSerializer()
        {
            /*
            AAS.API.Services.ADT.Models.AssetAdministrationShell aShell = 
                this.dtClient.GetDigitalTwin<AAS.API.Services.ADT.Models.AssetAdministrationShell>("Shell_ff7c799b-cae5-4ab8-b38d-2618026f35c6");
            Assert.IsNotNull(aShell);
            */
        }
    }
}
