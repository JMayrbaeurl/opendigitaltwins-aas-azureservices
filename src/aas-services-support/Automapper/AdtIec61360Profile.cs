using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;
using AutoMapper;
using System.Collections.Generic;

namespace Backend.Profiles
{
    public class AdtIec61360Profile : Profile
    {
        public AdtIec61360Profile()
        {
            CreateMap<AdtDataSpecificationIEC61360, DataSpecificationIec61360>()
                .ForMember(d => d.PreferredName,
                    o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.PreferredName)))
                .ForMember(d => d.Definition, o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.Definition)))
                .ForMember(d => d.ShortName, o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.ShortName)))
                .ForMember(s => s.ValueList, o => o.MapFrom(s => new ValueList(new List<ValueReferencePair>())))
                .ForMember(d => d.UnitId, o => o.MapFrom(s => new Reference(ReferenceTypes.GlobalReference,new List<Key>(){new Key(KeyTypes.GlobalReference,s.UnitIdValue)},null) ))
                //.ForMember(d => d.DataType, o => o.MapFrom(s => DataTypeDefXsd.String ))

                .ForSourceMember(s => s.UnitIdValue, opt => opt.DoNotValidate())
                .ConstructUsing(x => new DataSpecificationIec61360(ConvertAdtLangStringToGeneraLangString(x.PreferredName),null,null,null,null,null,null,null,null,null,null,null))
                .DisableCtorValidation();
        }

        public List<LangString> ConvertAdtLangStringToGeneraLangString(AdtLanguageString adtLangString)
        {
            var languageStrings = new List<LangString>();
            
            if (adtLangString== null || adtLangString.LangStrings == null)
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
