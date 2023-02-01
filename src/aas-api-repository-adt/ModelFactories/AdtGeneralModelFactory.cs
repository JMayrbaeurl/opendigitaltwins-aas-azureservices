using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.API.Repository.Adt
{
    public class AdtGeneralModelFactory
    {
        public List<LangString>? ConvertAdtLangStringToGeneraLangString(AdtLanguageString? adtLangString)
        {
            var languageStrings = new List<LangString>();
            if (adtLangString == null || adtLangString.LangStrings == null)
            {
                return null;
            }
            foreach (var langString in adtLangString.LangStrings)
            {
                languageStrings.Add(new LangString(langString.Key, langString.Value));
            }

            return languageStrings;
        }
    }
}
