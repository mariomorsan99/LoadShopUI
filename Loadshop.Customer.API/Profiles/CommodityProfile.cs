using AutoMapper;
using Loadshop.Customer.API.Models.Commodity;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.Customer.API.Profiles
{
    public class CommodityProfile : Profile
    {
        public CommodityProfile()
        {
            CreateMap<CommodityData, CommodityViewModel>().ReverseMap();
        }
    }
}