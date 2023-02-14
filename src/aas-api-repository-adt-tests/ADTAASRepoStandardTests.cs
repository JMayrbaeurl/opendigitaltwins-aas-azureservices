using AAS.ADT;
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
        public void TestConvertStringListToQueryArrayString()
        {
            List<string> values = new List<string>() { "First", "Second", "Third" };
            string queryString = AbstractADTAASService.ConvertStringListToQueryArrayString(values);
            Assert.AreEqual<string>(queryString, "['First','Second','Third']");
        }
    }
}
