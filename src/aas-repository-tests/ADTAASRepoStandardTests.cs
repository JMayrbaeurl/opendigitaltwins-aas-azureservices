using AAS.API.Repository;
using AAS.API.Services.ADT;
using AAS_Services_Support.ADT_Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AasCore.Aas3_0_RC02;

namespace AAS_Repository_Tests
{
    [TestClass]
    public class ADTAASRepoStandardTests
    {
        [TestMethod]
        public void TestGetAllAdministrationShells(IAdtInteractions adtInteractions)
        {
            AASRepository repo = new AASRepositoryFactory(adtInteractions).CreateAASRepositoryForADT("https://hack2021aasadt.api.weu.digitaltwins.azure.net");
            List<AssetAdministrationShell> adminshells = repo.GetAllAdministrationShells().GetAwaiter().GetResult();
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
