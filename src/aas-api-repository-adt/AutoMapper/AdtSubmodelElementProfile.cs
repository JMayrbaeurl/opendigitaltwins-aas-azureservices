using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;
using File = AasCore.Aas3_0_RC02.File;

namespace AAS.API.Repository.Adt.AutoMapper
{
    public class AdtSubmodelElementProfile : Profile
    {
        public AdtSubmodelElementProfile()
        {
            CreateMap<AdtSubmodelElement, ISubmodelElement>()
                .IncludeBase<AdtReferable,IReferable>()
                .ForMember(d=> d.Kind,o => o.MapFrom(s=>s.Kind.Kind))
                .ForMember(d => d.SemanticId,o=> o.Ignore())
                .ForMember(d => d.Qualifiers, o => o.Ignore())
                .ForMember(d => d.EmbeddedDataSpecifications, o => o.Ignore())
                .ForMember(d => d.SupplementalSemanticIds, o => o.Ignore());


            CreateMap<AdtProperty, Property>()
                .ForMember(d => d.ValueId,o=> o.Ignore())

                .IncludeBase<AdtSubmodelElement, ISubmodelElement>()
                .ConstructUsing(x => new Property(DataTypeDefXsd.Boolean,null,null,null,null, null, null, null, null, null, null, null, null,null));
            CreateMap<AdtFile, File>()
                .IncludeBase<AdtSubmodelElement, ISubmodelElement>()
                .ConstructUsing(x => new File("FileType", null, null, null, null, null, null, null, null, null, null, null, null));

        }

    }
}
