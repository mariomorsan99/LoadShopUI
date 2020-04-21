using AutoMapper;
using Loadshop.Customer.API.Models.LoadStop;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.Customer.API.Profiles
{
    public class LoadStopProfile : Profile
    {
        public LoadStopProfile()
        {
            CreateMap<AppointmentSchedulerConfirmationTypeData, AppointmentCodeViewModel>().ReverseMap();

            CreateMap<StopTypeData, StopTypeViewModel>().ReverseMap();
        }
    }
}