using AAS.API.Models;
using AAS.API.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AAS_Repository_Tests
{
    [TestClass]
    public class ADTAASRepoStandardTests
    {
        [TestMethod]
        public void TestGetAllAdministrationShells()
        {
            AASRepository repo = new AASRepositoryFactory().CreateAASRepositoryForADT("https://hack2021aasadt.api.weu.digitaltwins.azure.net");
            List<AssetAdministrationShell> adminshells = repo.GetAllAdministrationShells().GetAwaiter().GetResult();
            Assert.IsNotNull(adminshells);
            Assert.IsFalse(adminshells.Count == 0);
        }
    }
}
