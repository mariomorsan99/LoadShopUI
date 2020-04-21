using AutoMapper;
using Loadshop.API.Models.ViewModels;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loadshop.Web.Profiles
{
    public class LoadProfile : Profile
    {
        public LoadProfile()
        {
            CreateMap<LoadCarrierScacData, LoadCarrierScacViewModel>().ReverseMap();

            CreateMap<LoadContactData, LoadContactViewModel>().ReverseMap();

            CreateMap<LoadDetailData, LoadDetailViewModel>()
                .ForMember(x => x.ServiceTypes, y => y.MapFrom((src, dest) => src.ServiceTypes?.Select(_ => _.Name)))
                .ForMember(x => x.DoNotUseScacs, y => y.MapFrom((src, dest) =>
                    src.CarrierScacRestrictions?.Where(z => z.LoadCarrierScacRestrictionTypeId == Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.DoNotUse)).Select(_ =>
                    new LoadCarrierScacRestrictionViewModel()
                    {
                        Scac = _.Scac
                    })))
                .ForMember(x => x.UseOnlyScacs, y => y.MapFrom((src, dest) =>
                    src.CarrierScacRestrictions?.Where(z => z.LoadCarrierScacRestrictionTypeId == Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.UseOnly)).Select(_ =>
                    new LoadCarrierScacRestrictionViewModel()
                    {
                        Scac = _.Scac
                    })))
                .ReverseMap()
                .ForMember(x => x.ServiceTypes, y => y.MapFrom((src, dest) => src.ServiceTypes?.Select(_ => new ServiceTypeData { Name = _ })))
                .ForMember(x => x.CarrierScacRestrictions, y => y.MapFrom((src, dest) =>
                {
                    var scacRestrictions = new List<LoadCarrierScacRestrictionData>();

                    if (src.DoNotUseScacs != null)
                    {
                        scacRestrictions.AddRange(src.DoNotUseScacs.Select(z => new LoadCarrierScacRestrictionData()
                        {
                            Scac = z.Scac,
                            LoadCarrierScacRestrictionTypeId = Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.DoNotUse)
                        }));
                    }

                    if (src.UseOnlyScacs != null)
                    {
                        scacRestrictions.AddRange(src.UseOnlyScacs.Select(z => new LoadCarrierScacRestrictionData()
                        {
                            Scac = z.Scac,
                            LoadCarrierScacRestrictionTypeId = Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.UseOnly)
                        }));
                    }

                    return scacRestrictions;
                }));

            CreateMap<LoadLineItemData, LoadLineItemViewModel>().ReverseMap();

            CreateMap<LoadStopData, LoadStopViewModel>().ReverseMap();

            CreateMap<LoadStopContactData, LoadStopContactViewModel>().ReverseMap();

            CreateMap<LoadUpdateFuelData, LoadUpdateFuelViewModel>().ReverseMap();

            CreateMap<LoadUpdateScacData, LoadUpdateScacViewModel>().ReverseMap();
        }
    }
}