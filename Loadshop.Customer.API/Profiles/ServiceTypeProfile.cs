using AutoMapper;
using Loadshop.Customer.API.Models.ServiceType;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.Customer.API.Profiles
{
    public class ServiceTypeProfile : Profile
    {
        public ServiceTypeProfile()
        {
            CreateMap<ServiceTypeData, ServiceTypeViewModel>().ReverseMap();
        }
    }
}