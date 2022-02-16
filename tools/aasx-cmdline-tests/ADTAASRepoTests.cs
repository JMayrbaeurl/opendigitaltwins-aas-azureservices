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
            NameplateRef.Keys.Add(new Key(Key.Submodel, true, Identification.IdShort, "Nameplate"));

            Assert.AreEqual<string>("Submodel_27f89173-0ef5-486b-98f9-fa388395523a", 
                this.aasRepo.FindTwinForReference(NameplateRef).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void TestFindTwinForPropertyOf01_FestoAAS()
        {
            var propRef = new Reference(
                new Key(Key.AAS, true, Identification.IRI, "smart.festo.com/demo/aas/1/1/454576463545648365874"));
            propRef.Keys.Add(new Key(Key.Submodel, true, Identification.IdShort, "Nameplate"));
            propRef.Keys.Add(new Key("SubmodelElementCollection", true, Identification.IdShort, "Marking_RCM"));
            propRef.Keys.Add(new Key("Property", true, Identification.IdShort, "RCMLabelingPresent"));

            Assert.AreEqual<string>("Property_c31b62cf-5e8c-4917-be7e-467ca9fc6218",
                this.aasRepo.FindTwinForReference(propRef).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void TestFindLinkedReferences()
        {
            var result = this.aasRepo.FindLinkedReferences().GetAwaiter().GetResult();
            Assert.IsNotNull(result);
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
