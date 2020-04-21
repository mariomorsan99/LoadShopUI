using AutoMapper;
using Loadshop.Customer.API.Models.Transportation;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.Customer.API.Profiles
{
    public class TransportationProfile : Profile
    {
        public TransportationProfile()
        {
            CreateMap<TransportationModeData, TransportationModeViewModel>().ReverseMap();
        }
    }
}