using AAS.AASX.CmdLine.Import;
using AAS.AASX.CmdLine.Import.ADT;
using AdminShellNS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine.Test.Import
{
    [TestClass]
    public class SimpleAASXImportTests : AbstractTestSupport
    {
        private IAASXImporter importer;

        private HttpClient httpClient;

        [TestInitialize]
        public void Setup()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) => { configBuilder.AddJsonFile("appsettings.tests.json"); })
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureBasicServices(services, hostContext.Configuration["ADT_SERVICE_URL"],
                        hostContext.Configuration["TENANT_ID"]);

                    services.AddSingleton<IAASRepo, ADTAASRepo>();
                    services.AddSingleton<IAASXImporter, ADTAASXPackageImporter>();
                    services.AddHttpClient();
                    configuration = hostContext.Configuration;
                })
                .Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            importer = provider.GetRequiredService<IAASXImporter>();
            this.httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
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

        [TestMethod]
        public void TestImportConceptDescription()
        {
            string outputPath = Path.GetTempPath() + "01_Festo.aasx";
            byte[] fileBytes = this.httpClient.GetByteArrayAsync(new Uri("https://admin-shell-io.com/samples/aasx/01_Festo.aasx")).GetAwaiter().GetResult();
            System.IO.File.WriteAllBytes(outputPath, fileBytes);

            try
            {
                using var package = new AdminShellPackageEnv(outputPath);

                this.importer.ImportConceptDescription(package.AasEnv.ConceptDescriptions[0]).GetAwaiter().GetResult();
            }
            finally
            {
                System.IO.File.Delete(outputPath);
            }
        }

        [TestMethod]
        public void TestCreateLinkedReferences()
        {
            this.importer.CreateLinkedReferences(null).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestCreateLinkedReferencesFiltered()
        {
            ISet<string> filter = new HashSet<string>(new string[] {""});
            this.importer.CreateLinkedReferences(filter).GetAwaiter().GetResult();
        }
    }
}
