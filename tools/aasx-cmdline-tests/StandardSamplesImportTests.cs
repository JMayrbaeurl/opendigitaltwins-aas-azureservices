using AAS.AASX.CmdLine.Import;
using AAS.AASX.CmdLine.Import.ADT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace AAS.AASX.CmdLine.Test.Import
{
    [TestClass]
    public class StandardSamplesImportTests : AbstractTestSupport
    {
        private IAASXImporter importer;
        private HttpClient httpClient;

        private Dictionary<string, Uri> samplesUrls = new Dictionary<string, Uri>()
        {
            { "01 Festo", new Uri("https://admin-shell-io.com/samples/aasx/01_Festo.aasx") },
            { "02 Bosch", new Uri("https://admin-shell-io.com/samples/aasx/02_Bosch.aasx") },
            { "03 Bosch", new Uri("https://admin-shell-io.com/samples/aasx/03_Bosch.aasx") },
            { "04 Bosch", new Uri("https://admin-shell-io.com/samples/aasx/04_Bosch.aasx") },
            { "05 Bosch", new Uri("https://admin-shell-io.com/samples/aasx/05_Bosch.aasx") },
            { "06 Bosch", new Uri("https://admin-shell-io.com/samples/aasx/06_Bosch.aasx") },
            { "07 PhoenixContact", new Uri("https://admin-shell-io.com/samples/aasx/07_PhoenixContact.aasx") },
            { "08 SchneiderElectric", new Uri("https://admin-shell-io.com/samples/aasx/08_SchneiderElectric.aasx") },
            { "15 Siemens", new Uri("https://admin-shell-io.com/samples/aasx/15_Siemens.aasx") }
        };

        [TestInitialize]
        public void Setup()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) => { configBuilder.AddJsonFile("appsettings.tests.json"); })
                .ConfigureServices((hostContext, services) =>
                {
                    ConfigureBasicServices(services, hostContext.Configuration["ADT_SERVICE_URL"]);

                    services.AddHttpClient();
                    services.AddSingleton<IAASRepo, ADTAASRepo>();
                    services.AddSingleton<IAASXImporter, ADTAASXPackageImporter>();
                    configuration = hostContext.Configuration;
                })
                .Build();

            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            this.importer = provider.GetRequiredService<IAASXImporter>();
            this.httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        }

        [TestMethod]
        public void TestSetup()
        {
            Assert.IsNotNull(this.importer);
            Assert.IsNotNull(this.httpClient);
        }

        [TestMethod]
        public void TestImport_Sample_01_Festo()
        {
            string outputPath = Path.GetTempPath() + "01_Festo.aasx";
            byte[] fileBytes = this.httpClient.GetByteArrayAsync(this.samplesUrls["01 Festo"]).GetAwaiter().GetResult();
            File.WriteAllBytes(outputPath, fileBytes);

            ImportResult importResult;
            try
            {
                ImportContext processInfo = new ImportContext();
                importResult = this.importer.ImportFromPackageFile(outputPath, processInfo).GetAwaiter().GetResult();
            }
            finally
            {
                File.Delete(outputPath);
            }

            Assert.IsNotNull(importResult);
            Assert.IsTrue(importResult.DTInstances.Any());
        }

        [TestMethod]
        public void TestImport_Sample_02_Bosch()
        {
            string outputPath = Path.GetTempPath() + "02_Bosch.aasx";
            byte[] fileBytes = this.httpClient.GetByteArrayAsync(this.samplesUrls["02 Bosch"]).GetAwaiter().GetResult();
            File.WriteAllBytes(outputPath, fileBytes);

            ImportResult importResult;
            try
            {
                ImportContext processInfo = new ImportContext();
                importResult = this.importer.ImportFromPackageFile(outputPath, processInfo).GetAwaiter().GetResult();
            }
            finally
            {
                File.Delete(outputPath);
            }

            Assert.IsNotNull(importResult);
            Assert.IsTrue(importResult.DTInstances.Any());
        }

        [TestMethod]
        public void TestImport_Sample_07_PhoenixContact()
        {
            string outputPath = Path.GetTempPath() + "07_PhoenixContact.aasx";
            byte[] fileBytes = this.httpClient.GetByteArrayAsync(this.samplesUrls["07 PhoenixContact"]).GetAwaiter().GetResult();
            File.WriteAllBytes(outputPath, fileBytes);

            ImportResult importResult;
            try
            {
                ImportContext processInfo = new ImportContext();
                importResult = this.importer.ImportFromPackageFile(outputPath, processInfo).GetAwaiter().GetResult();
            }
            finally
            {
                File.Delete(outputPath);
            }

            Assert.IsNotNull(importResult);
            Assert.IsTrue(importResult.DTInstances.Any());
        }

        [TestMethod]
        public void TestImport_Sample_08_SchneiderElectric()
        {
            string outputPath = Path.GetTempPath() + "08_SchneiderElectric.aasx";
            byte[] fileBytes = this.httpClient.GetByteArrayAsync(this.samplesUrls["08 SchneiderElectric"]).GetAwaiter().GetResult();
            File.WriteAllBytes(outputPath, fileBytes);

            ImportResult importResult;
            try
            {
                ImportContext processInfo = new ImportContext();
                importResult = this.importer.ImportFromPackageFile(outputPath, processInfo).GetAwaiter().GetResult();
            }
            finally
            {
                File.Delete(outputPath);
            }

            Assert.IsNotNull(importResult);
            Assert.IsTrue(importResult.DTInstances.Any());
        }

        [TestMethod]
        public void TestImport_Sample_15_Siemens()
        {
            string outputPath = Path.GetTempPath() + "15_Siemens.aasx";
            byte[] fileBytes = this.httpClient.GetByteArrayAsync(this.samplesUrls["15 Siemens"]).GetAwaiter().GetResult();
            File.WriteAllBytes(outputPath, fileBytes);

            ImportResult importResult;
            try
            {
                ImportContext processInfo = new ImportContext();
                importResult = this.importer.ImportFromPackageFile(outputPath, processInfo).GetAwaiter().GetResult();
            }
            finally
            {
                File.Delete(outputPath);
            }

            Assert.IsNotNull(importResult);
            Assert.IsTrue(importResult.DTInstances.Any());
        }
    }
}
