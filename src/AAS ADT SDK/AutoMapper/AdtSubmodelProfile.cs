using System.Collections.Generic;
using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.ADT.AutoMapper
{
    public class AdtSubmodelProfile : Profile
    {
        public AdtSubmodelProfile()
        {
            CreateMap<AdtSubmodel, Submodel>()
                .IncludeBase<AdtIdentifiable, IIdentifiable>()
                .ForMember(d => d.Kind, o => o.MapFrom(s => s.Kind.Kind))
                .ForMember(d => d.SemanticId, o => o.Ignore())
                .ForMember(d => d.Qualifiers, o => o.Ignore())
                .ForMember(d => d.SupplementalSemanticIds, o => o.Ignore())
                .ForMember(d => d.EmbeddedDataSpecifications, o => o.Ignore())
                .ForMember(d => d.SubmodelElements, o => o.Ignore())
                .ConstructUsing(x => new Submodel(x.Id, null, null, null, null, null, null, null, null, null, null, null,null,null));
        }
    }

}
