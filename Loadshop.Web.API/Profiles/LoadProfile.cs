using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.API.Models.Loads;

namespace Loadshop.Web.API.Profiles
{
    public class LoadProfile : Profile
    {
        public LoadProfile()
        {
            CreateMap<LoadData, LoadDetailViewModel>().ReverseMap();
        }
    }
}