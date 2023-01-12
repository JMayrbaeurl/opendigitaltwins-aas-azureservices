using AAS.API.Repository.Adt.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.API.Repository.Adt.AutoMapper
{
    public class AdtAssetAdministrationShellProfile : Profile
    {
        public AdtAssetAdministrationShellProfile()
        {
            CreateMap<AdtAas, AssetAdministrationShell>()
                .IncludeBase<AdtIdentifiable, IIdentifiable>()
                .ForMember(d => d.AssetInformation, o => o.MapFrom(
                    s => GetAssetInformation(s.AssetInformation.GlobalAssetId)))
                .ForMember(d => d.DerivedFrom,o=> o.Ignore())
                .ForMember(d => d.EmbeddedDataSpecifications, o => o.Ignore())
                .ForMember(d => d.Submodels, o => o.Ignore())
                .ConstructUsing(x => new AssetAdministrationShell(x.Id, null, null, null, null, null, null, null, null, null, null, null));

            CreateMap<AdtAssetInformationShort, AssetInformation>()
                .ForMember(d => d.SpecificAssetIds, o => o.Ignore())
                .ForMember(d => d.DefaultThumbnail, o => o.Ignore())
                .ForMember(d => d.GlobalAssetId, o => o.Ignore())
                .ConstructUsing(x => new AssetInformation(AssetKind.Instance, null, null, null));
        }

        public AssetInformation GetAssetInformation(string globalAssetId)
        {
            var specificAssetIds = new List<Reference>();
            

            var assetInformation = new AssetInformation(
            AssetKind.Instance,
                new Reference(
                ReferenceTypes.GlobalReference,
                new List<Key>() { new Key(KeyTypes.GlobalReference, globalAssetId) }),null,null
                );
            
            
            return assetInformation;

        }
    }

}
