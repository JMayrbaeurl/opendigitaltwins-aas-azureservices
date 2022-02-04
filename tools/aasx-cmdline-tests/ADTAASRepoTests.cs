using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine.Test
{
    [TestClass]
    public class ADTAASRepoTests
    {
        private IAASRepo aasRepo;

        [TestMethod]
        public void TestFindTwinFor01_FestoAAS()
        {
            var AASRef = new Reference(new Key(Key.AAS, true, Identification.IRI, "smart.festo.com/demo/aas/1/1/454576463545648365874"));
            Assert.IsFalse(String.IsNullOrEmpty(this.aasRepo.FindTwinForReference(AASRef).GetAwaiter().GetResult()));
        }

        [TestMethod]
        public void TestFindTwinForNameplateOf01_FestoAAS()
        {
            var NameplateRef = new Reference(
                new Key(Key.AAS, true, Identification.IRI, "smart.festo.com/demo/aas/1/1/454576463545648365874"));
            NameplateRef.Keys.Add(
                new Key(Key.Submodel, true, Identification.IdShort, "Nameplate"));
            Assert.IsFalse(String.IsNullOrEmpty(this.aasRepo.FindTwinForReference(NameplateRef).GetAwaiter().GetResult()));
        }

        [TestInitialize]
        public void Setup()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    ConfigureBasicServices(services, "https://aasadtdevjm.api.weu.digitaltwins.azure.net");

                    services.AddSingleton<IAASRepo, ADTAASRepo>();
                })
                .Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            this.aasRepo = provider.GetRequiredService<IAASRepo>();
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
    }
}
