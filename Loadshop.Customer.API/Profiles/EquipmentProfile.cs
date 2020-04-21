using AutoMapper;
using Loadshop.Customer.API.Models.Equipment;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.Customer.API.Profiles
{
    public class EquipmentProfile : Profile
    {
        public EquipmentProfile()
        {
            CreateMap<EquipmentData, EquipmentViewModel>().ReverseMap();
        }
    }
}