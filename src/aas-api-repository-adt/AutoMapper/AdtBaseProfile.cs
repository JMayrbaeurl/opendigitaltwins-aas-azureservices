﻿using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.API.Repository.Adt.AutoMapper
{
    public class AdtBaseProfile : Profile
    {
        public List<LangString> ConvertAdtLangStringToGeneraLangString(AdtLanguageString adtLangString)
        {
            var languageStrings = new List<LangString>();

            if (adtLangString == null || adtLangString.LangStrings == null)
            {
                return null;
            }
            else
            {
                foreach (var langString in adtLangString.LangStrings)
                {
                    languageStrings.Add(new LangString(langString.Key, langString.Value));
                }

                return languageStrings;
            }
        }
    }
}