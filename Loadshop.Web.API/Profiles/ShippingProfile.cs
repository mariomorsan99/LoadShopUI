using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.API.Models.Shipping;

namespace Loadshop.Web.API.Profiles
{
    public class ShippingProfile : Profile
    {
        public ShippingProfile()
        {
            CreateMap<OrderEntryLoadDetailData, OrderEntryViewModel>().ReverseMap();
            CreateMap<LoadDetailData, OrderEntryViewModel>().ReverseMap();

            CreateMap<LoadContactData, OrderEntryLoadContactViewModel>().ReverseMap();
            CreateMap<LoadStopData, OrderEntryLoadStopViewModel>().ReverseMap();
            CreateMap<OrderEntryLoadStopData, OrderEntryLoadStopViewModel>().ReverseMap();
            CreateMap<LoadStopContactData, OrderEntryLoadStopContactViewModel>().ReverseMap();
            CreateMap<LoadLineItemData, OrderEntryLoadLineItemViewModel>().ReverseMap();

            CreateMap<LocationData, LocationViewModel>().ReverseMap();
            CreateMap<LocationData, LoadStopData>().ReverseMap();
        }
    }
}