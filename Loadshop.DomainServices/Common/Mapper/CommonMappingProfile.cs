using AutoMapper;
using Loadshop.DomainServices.Common.DataProvider.Entities;
using Loadshop.DomainServices.Common.Services.Data;

namespace Loadshop.DomainServices.Common.Mapper
{
    public class CommonMappingProfile : Profile
    {
        public CommonMappingProfile()
        {
            CreateMap<StateEntity, StateData>()
                .ForMember(x => x.Abbreviation, y => y.MapFrom(z => z.StateCd))
                .ForMember(x => x.Name, y => y.MapFrom(z => z.StateName));
            
        }
    }
}
