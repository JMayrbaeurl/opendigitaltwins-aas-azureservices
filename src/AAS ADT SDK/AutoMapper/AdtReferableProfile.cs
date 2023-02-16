using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;

namespace AAS.ADT.AutoMapper
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
