using AAS.API.Services.ADT;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AAS.API.Repository.Adt.Tests
{
    [TestClass]
    public class ADTAASRepoStandardTests
    {
        [TestMethod]
        public void TestGetAllAdministrationShells(
            IAdtAasConnector adtAasConnector, IMapper mapper, ILogger<AASRepositoryFactory> logger)
        {
            AASRepository repo = new AASRepositoryFactory(adtAasConnector,mapper,logger).CreateAASRepositoryForADT("https://hack2021aasadt.api.weu.digitaltwins.azure.net");
            List<AssetAdministrationShell> adminshells = repo.GetAllAssetAdministrationShells().GetAwaiter().GetResult();
            Assert.IsNotNull(adminshells);
            Assert.IsFalse(adminshells.Count == 0);
        }

        [TestMethod]
        public void TestConvertStringListToQueryArrayString()
        {
            List<string> values = new List<string>() { "First", "Second", "Third" };
            string queryString = AbstractADTAASService.ConvertStringListToQueryArrayString(values);
            Assert.AreEqual<string>(queryString, "['First','Second','Third']");
        }
    }
}
