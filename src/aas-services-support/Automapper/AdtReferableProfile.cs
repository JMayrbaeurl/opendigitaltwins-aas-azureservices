﻿using AasCore.Aas3_0_RC02;
using AdtModels.AdtModels;

namespace AAS_Services_Support.AutoMapper
{
    public class AdtReferableProfile : AdtBaseProfile
    {
        public AdtReferableProfile()
        {
            CreateMap<AdtReferable, IReferable>()
                .ForMember(d => d.Description, o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.Description)))
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => ConvertAdtLangStringToGeneraLangString(s.DisplayName)))
                .ForMember(d => d.Extensions,o => o.Ignore());

        }
    }
}
