using AAS.ADT.Models;
using AasCore.Aas3_0_RC02;
using AutoMapper;

namespace AAS.API.Repository.Adt.AutoMapper
{
    public class AdtIdentifiableProfile : Profile
    {
        public AdtIdentifiableProfile()
        {
            CreateMap<AdtIdentifiable, IIdentifiable>()
                .IncludeBase<AdtReferable, IReferable>()
                .ForMember(d => d.Administration,
                    o => o.MapFrom(s => CreateAdministrationFromAdtAdministration(s.Administration)));
        }

        public AdministrativeInformation CreateAdministrationFromAdtAdministration(AdtAdministration adtAdministration)
        {
            return new AdministrativeInformation(
                new List<EmbeddedDataSpecification>(), 
                adtAdministration.Version,
                adtAdministration.Revision);
        }
    }
}
