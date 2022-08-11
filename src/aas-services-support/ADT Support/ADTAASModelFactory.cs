using AAS.API.Models;
using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AAS.API.Services.ADT
{
    public class ADTAASModelFactory
    {
        public AssetAdministrationShell CreateAASFromBasicDigitalTwin(BasicDigitalTwin twin)
        {
            AssetAdministrationShell aShell = new AssetAdministrationShell();

            if(twin.Contents.ContainsKey("identification"))
            {
                string component1RawText = ((JsonElement)twin.Contents["identification"]).GetRawText();
                var component1 = JsonSerializer.Deserialize<BasicDigitalTwinComponent>(component1RawText);
                if (component1.Contents.ContainsKey("id"))
                    aShell.Identification = component1.Contents["id"].ToString();
            }

            if (twin.Contents.ContainsKey("idShort"))
            {
                aShell.IdShort = twin.Contents["idShort"].ToString();
            }
            else
            {
                aShell.IdShort = $"AAS with ADT id '{twin.Id}' has no entry for idShort";
            }

            if (twin.Contents.ContainsKey("category"))
                aShell.Category = twin.Contents["category"].ToString();

            if (twin.Contents.ContainsKey("displayName"))
                aShell.DisplayName = twin.Contents["displayName"].ToString();

            if (twin.Contents.ContainsKey("description"))
                aShell.Description = CreateLanguageStringArrayFromString(twin.Contents["description"].ToString());

            return aShell;
        }

        public AssetAdministrationShell CreateAASFromJsonElement(JsonElement element)
        {
            AssetAdministrationShell aShell = new AssetAdministrationShell();

            JsonElement propFromADT;
            if (element.TryGetProperty("idShort", out propFromADT)) {
                aShell.IdShort = propFromADT.GetString();
            } else
            {
                aShell.IdShort = $"AAS with ADT id '{element.GetProperty("$dtId")}' has no entry for idShort";
            }

            if (element.TryGetProperty("category", out propFromADT))
                aShell.Category = propFromADT.GetString();

            if (element.TryGetProperty("displayName", out propFromADT))
                aShell.DisplayName = propFromADT.GetString();

            if (element.TryGetProperty("description", out propFromADT))
                aShell.Description = CreateLanguageStringArrayFromString(propFromADT.GetString());

            return aShell;
        }

        public List<LangString> CreateLanguageStringArrayFromString(string compositeString)
        {
            List<LangString> result = new List<LangString>();

            if (compositeString != null && compositeString.Length > 0)
            {
                string[] langEntries = compositeString.Split(';');
                foreach (string langEntry in langEntries)
                {
                    string[] anEntry = langEntry.Split(',');
                    if (anEntry != null && anEntry.Length == 2)
                    {
                        result.Add(new LangString() { Language = anEntry[0], Text = anEntry[1] });
                    }
                }
            }

            return result;
        }
    }
}
