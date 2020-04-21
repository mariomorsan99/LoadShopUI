using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Proxy.Visibility.Models;

namespace Loadshop.DomainServices.Loadshop.Mapper
{
    public class ClientServiceMappingProfile : Profile
    {
        public ClientServiceMappingProfile()
        {
            CreateMap<GetLoadStatusNotificationResponseModel, LoadStatusNotificationsData>()
                .ForMember(x => x.TextMessageEnabled, y => y.MapFrom(z => z.ResponseIsSMS))
                .ForMember(x => x.TextMessageNumber, y => y.MapFrom(z => z.Phone))
                .ForMember(x => x.EmailEnabled, y => y.MapFrom(z => z.ResponseIsEmail))
                .ForMember(x => x.Email, y => y.MapFrom(z => z.Email))
                .ForMember(x => x.DepartedEnabled, y => y.MapFrom(z => z.StatusIsDeparted))
                .ForMember(x => x.ArrivedEnabled, y => y.MapFrom(z => z.StatusIsArrived))
                .ForMember(x => x.DeliveredEnabled, y => y.MapFrom(z => z.StatusIsDelivered))
                .AfterMap((model, data) =>
                {
                    // remove the country code
                    if (data.TextMessageNumber.Length > 10 && data.TextMessageNumber.StartsWith("+"))
                    {
                        data.TextMessageNumber = data.TextMessageNumber.Substring(data.TextMessageNumber.Length - 10);
                    }
                });
            CreateMap<LoadStatusNotificationsData, VisibilityNotificationRegistrationModel>()
                .ForMember(x => x.IsArrived, y => y.MapFrom(z => z.ArrivedEnabled))
                .ForMember(x => x.IsDelivered, y => y.MapFrom(z => z.DeliveredEnabled))
                .ForMember(x => x.IsDeparted, y => y.MapFrom(z => z.DepartedEnabled))
                .ForMember(x => x.IsEmail, y => y.MapFrom(z => z.EmailEnabled))
                .ForMember(x => x.IsMobilePush, y => y.Ignore())
                .ForMember(x => x.IsSMS, y => y.MapFrom(z => z.TextMessageEnabled))
                .ForMember(x => x.IsWebPush, y => y.Ignore())
                .ForMember(x => x.PhoneNumber, y => y.MapFrom(z => z.TextMessageNumber))
                .ForMember(x => x.Email, y => y.MapFrom(z => z.Email));


            
        }
    }
}
