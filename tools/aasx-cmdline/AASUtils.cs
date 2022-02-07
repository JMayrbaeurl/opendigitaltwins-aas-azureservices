
using static AdminShellNS.AdminShellV20;

namespace AAS.AASX.CmdLine
{
    public class AASUtils
    {
        public static string URITOIRI(string idType)
        {
            if ("URI".Equals(idType))
                return Identification.IRI;
            else
                return idType;
        }

        public static string DescToString(Description desc)
        {
            if (desc == null)
                return default;
            else
            {
                string result = "";
                foreach (var entry in desc.langString)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string LangStringSetIEC61360ToString(LangStringSetIEC61360 langStrs)
        {
            if (langStrs == null)
                return default;
            else
            {
                string result = "";
                foreach (var entry in langStrs)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string LangStringSetToString(LangStringSet langStrs)
        {
            if (langStrs == null)
                return default;
            else
            {
                string result = "";
                foreach (var entry in langStrs.langString)
                {
                    if (result.Length > 0)
                        result += "\n";
                    result += $"{entry.lang},{entry.str}";
                }

                return result;
            }
        }

        public static string StripInvalidTwinIdCharacters(string dtIdProposal)
        {
            string result = dtIdProposal.Trim();

            result = result.Replace(" ", "");
            result = result.Replace("/", "");

            return result;
        }
    }
}
