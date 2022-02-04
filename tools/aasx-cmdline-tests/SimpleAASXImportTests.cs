using AAS.AASX.CmdLine.Import;
using AAS.AASX.CmdLine.Import.ADT;
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

namespace AAS.AASX.CmdLine.Test.Import
{
    [TestClass]
    public class SimpleAASXImportTests
    {
        private IAASXImporter importer;

        [TestInitialize]
        public void Setup()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    ConfigureBasicServices(services, "https://aasadtdevjm.api.weu.digitaltwins.azure.net");

                    services.AddSingleton<IAASXImporter, ADTAASXPackageImporter>();
                })
                .Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            importer = provider.GetRequiredService<IAASXImporter>();
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

        [TestMethod]
        public void TestImportRelationShipElement01()
        {
            RelationshipElement relElement = new RelationshipElement()
            {
                idShort = "test",
                first = new Reference(new Key(Key.SubmodelElement, true, Identification.IRI, "")),
                second = new Reference(new Key(new Key(Key.SubmodelElement, true, Identification.IRI, "")))
            };
            string dtId = importer.ImportRelationshipElement(relElement).GetAwaiter().GetResult();
            Assert.IsFalse(String.IsNullOrEmpty(dtId));
        }
    }

    public class DigitalTwinsClientOptions
    {
        public Uri ADTEndpoint { get; set; }
    }
}
