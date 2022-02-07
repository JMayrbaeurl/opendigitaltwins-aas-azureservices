using AAS.AASX.CmdLine.Import;
using AAS.AASX.CmdLine.Import.ADT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine.Test.Import
{
    [TestClass]
    public class SimpleAASXImportTests : AbstractTestSupport
    {
        private IAASXImporter importer;

        [TestInitialize]
        public void Setup()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) => { configBuilder.AddJsonFile("appsettings.tests.json"); })
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureBasicServices(services, hostContext.Configuration["ADT_SERVICE_URL"]);

                    services.AddSingleton<IAASRepo, ADTAASRepo>();
                    services.AddSingleton<IAASXImporter, ADTAASXPackageImporter>();
                    configuration = hostContext.Configuration;
                })
                .Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            importer = provider.GetRequiredService<IAASXImporter>();
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
}
