using System.Collections.Generic;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT.AutoMapper
{
    public class AdtIec61360Profile : AdtBaseProfile
    {
        public AdtIec61360Profile()
        {
            CreateMap<AdtDataSpecificationIEC61360, DataSpecificationIec61360>()
                .ForMember(d => d.PreferredName,
                    o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.PreferredName)))
                .ForMember(d => d.Definition, o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.Definition)))
                .ForMember(d => d.ShortName, o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.ShortName)))
                .ForMember(s => s.ValueList, o => o.Ignore())
                .ForMember(d => d.UnitId, o => o.MapFrom(s => s.UnitIdValue == null ? null : new Reference(ReferenceTypes.GlobalReference, new List<Key>() { new Key(KeyTypes.GlobalReference, s.UnitIdValue) }, null))).ForSourceMember(s => s.UnitIdValue, opt => opt.DoNotValidate())
                .ConstructUsing(x => new DataSpecificationIec61360(ConvertAdtLangStringToGeneraLangString(x.PreferredName), null, null, null, null, null, null, null, null, null, null, null))
                .DisableCtorValidation();
        }

    }

}
