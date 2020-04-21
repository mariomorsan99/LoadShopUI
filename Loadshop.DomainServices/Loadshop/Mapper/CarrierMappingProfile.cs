using AutoMapper;
using Loadshop.DomainServices.Carrier.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;

namespace Loadshop.DomainServices.Loadshop.Mapper
{
    public class CarrierMappingProfile : Profile
    {
        public CarrierMappingProfile()
        {
            CreateMap<CarrierScacEntity, ScacData>()
                .ForMember(x => x.Name, y => y.MapFrom(z => z.ScacName));

            CreateMap<CarrierEntity, CarrierData>();
        }
    }
}
