using AutoMapper;
using Loadshop.Customer.API.Models.UnitOfMeasure;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.Customer.API.Profiles
{
    public class UnitOfMeasureProfile : Profile
    {
        public UnitOfMeasureProfile()
        {
            CreateMap<UnitOfMeasureData, UnitOfMeasureViewModel>().ReverseMap();
        }
    }
}