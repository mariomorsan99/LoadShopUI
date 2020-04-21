using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;

namespace Loadshop.DomainServices.Loadshop.Mapper
{
    public class LoadshopMappingProfile : Profile
    {
        public LoadshopMappingProfile()
        {
            CreateMap<CustomerData, CustomerEntity>().ReverseMap();
            CreateMap<CustomerProfileData, CustomerEntity>()
                .ForMember(x => x.FuelReratingNumberOfDays, y => y.MapFrom(z => Math.Abs(z.FuelReratingNumberOfDays.GetValueOrDefault(0))))
                .ReverseMap()
                .ForMember(x => x.IdentUserSetup, y => y.MapFrom(z => z.IdentUserId != null))
                .ForMember(x => x.CustomerCarrierScacs, y => y.MapFrom(z => z.CustomerCarrierScacContracts.Select(a => a.Scac)))
                .ForMember(x => x.FuelReratingNumberOfDays, y => y.MapFrom(z => Math.Abs(z.FuelReratingNumberOfDays)));

            CreateMap<CustomerContactData, CustomerContactEntity>().ReverseMap();

            CreateMap<EquipmentEntity, EquipmentData>().ReverseMap();

            CreateMap<LoadContactData, LoadContactEntity>().ReverseMap();

            CreateMap<LoadDetailData, LoadEntity>()
                .ForMember(x => x.EquipmentId, y => y.MapFrom(z => z.EquipmentType))
                .ForMember(x => x.TransportationMode, y => y.Ignore())
                .ForMember(x => x.Equipment, opt => opt.Ignore())
                .ForMember(x => x.LoadServiceTypes, y => y.MapFrom((src, dest) => src.ServiceTypes?.Select(_ => new LoadServiceTypeEntity { ServiceTypeId = _.ServiceTypeId, LoadId = src.LoadId ?? Guid.Empty })))
                .ReverseMap()
                .ForMember(x => x.LoadTransaction, y => y.MapFrom(z => z.LatestLoadTransaction))
                .ForMember(x => x.EquipmentType, y => y.MapFrom(z => z.EquipmentId))
                .ForMember(x => x.ServiceTypes, y => y.MapFrom((src, dest) => src.LoadServiceTypes?.Select(_ => new ServiceTypeData { ServiceTypeId = _.ServiceTypeId, Name = _.ServiceType?.Name })))
                .ForMember(x => x.EquipmentTypeDisplay, opt => opt.MapFrom(src => src.Equipment.CategoryEquipmentDesc ?? src.Equipment.EquipmentDesc))
                .ForMember(x => x.EquipmentCategoryDesc, opt => opt.MapFrom(src => src.Equipment.CategoryEquipmentDesc));

            CreateMap<LoadStopData, LoadStopEntity>()
                .ForMember(x => x.StopType, y => y.Ignore())
                .ReverseMap()
                .ForMember(x => x.StopType, y => y.MapFrom((src, dest) => src.StopType?.Name))
                .ForMember(x => x.AppointmentConfirmationCode, y => y.MapFrom((src, dest) => src.AppointmentSchedulerConfirmationType?.AppointmentConfirmationCode))
                .ForMember(x => x.AppointmentSchedulingCode, y => y.MapFrom((src, dest) => src.AppointmentSchedulerConfirmationType?.AppointmentSchedulingCode));

            CreateMap<LoadStopContactData, LoadStopContactEntity>().ReverseMap();

            CreateMap<LoadTransactionData, LoadTransactionEntity>()
                .ReverseMap()
                .ForMember(x => x.TransactionType, y => y.MapFrom(z => Enum.Parse(typeof(TransactionTypeData), z.TransactionTypeId)))
                .ForMember(x => x.LineHaulRate, y => y.MapFrom(z => z.Claim.LineHaulRate))
                .ForMember(x => x.FuelRate, y => y.MapFrom(z => z.Claim.FuelRate))
                .ForMember(x => x.UserId, y => y.MapFrom(z => z.Claim.UserId))
                .ForMember(x => x.Scac, y => y.MapFrom(z => z.Claim.Scac))
                .ForMember(x => x.LastUpdateTime, y => y.MapFrom(z => z.LastChgDtTm));

            CreateMap<MessageTypeData, MessageTypeEntity>().ReverseMap();

            CreateMap<UserLaneData, UserLaneEntity>()
                .ForMember(x => x.UserLaneMessageTypes, y => y.MapFrom(x => new List<UserLaneMessageTypeEntity>()))
                .ForMember(x => x.SearchTypeId, y => y.MapFrom(z => z.SearchType.ToString()))
                .ReverseMap()
                .ForMember(x => x.EquipmentIds, y => y.MapFrom(z => z.UserLaneEquipments.Select(a => a.EquipmentId)))
                .ForMember(x => x.SearchType, y => y.MapFrom(z => Enum.Parse(typeof(SearchTypeData), z.SearchTypeId)));

            CreateMap<UserLaneLoadData, UserLaneLoadEntity>().ReverseMap();

            CreateMap<UserLaneMessageTypeData, UserLaneMessageTypeEntity>().ReverseMap()
                .ForMember(x => x.Selected, y => y.MapFrom(x => true));

            CreateMap<UserNotificationData, UserNotificationEntity>().ReverseMap()
                .ForMember(x => x.Description, y => y.MapFrom(z => z.MessageType.MessageTypeDesc))
                .ForMember(x => x.MessageType, y => y.MapFrom(x => new MessageTypeData()));

            CreateMap<UserProfileData, UserEntity>()
                .ReverseMap()
                //Comment out this line once the Primary scac is populated
                .ForMember(userProfileData => userProfileData.CarrierName, opt => opt.MapFrom(user => user.PrimaryScacEntity.Carrier.CarrierName))
                //Map across the Many to Many
                .ForMember(UserProfileData => UserProfileData.AuthorizedShippers,
                                                        config => config.MapFrom(
                                                            user => user.UserShippers
                                                                .Select(userShipper => userShipper.Customer)))
                .ForMember(dest => dest.AllowManualLoadCreation,
                            opt => opt.MapFrom((src, dest) => src.PrimaryCustomer?.AllowManualLoadCreation ?? false))
                .AfterMap<SetUserNameAction>();

            CreateMap<MessageTypeEntity, UserNotificationData>()
                .ForMember(x => x.Description, y => y.MapFrom(z => z.MessageTypeDesc));

            CreateMap<MessageTypeEntity, UserLaneMessageTypeData>()
                .ForMember(x => x.Description, y => y.MapFrom(z => z.MessageTypeDesc));

            CreateMap<LoadDetailViewEntity, LoadDetailData>()
                .ForMember(x => x.IsAccepted, y => y.MapFrom(z => z.TransactionTypeId == "Accepted" || z.TransactionTypeId == "PreTender" || z.TransactionTypeId == "Pending" || z.TransactionTypeId == "Removed"))
                .ForMember(x => x.EquipmentType, y => y.MapFrom(z => z.EquipmentId))
                .ForMember(x => x.BookedUser, y => y.MapFrom(z =>
                   (!z.BookedUserId.HasValue || string.IsNullOrEmpty(z.BookedUser)) ? null : new UserContactData()
                   {
                       UserId = z.BookedUserId.HasValue ? z.BookedUserId.Value : Guid.Empty,
                       UserName = z.BookedUser
                   }))
                .ForMember(x => x.LoadTransaction, y => y.MapFrom(z => new LoadTransactionData()
                {
                    LineHaulRate = z.TransactionLineHaulRate,
                    FuelRate = z.TransactionFuelRate,
                    Scac = z.Scac,
                    TransactionType = (TransactionTypeData)Enum.Parse(typeof(TransactionTypeData), z.TransactionTypeId),
                    UserId = z.UserId,
                    LastUpdateTime = z.TransactionLastUpdateTime
                }));

            CreateMap<LoadDetailViewEntity, LoadData>()
                .ForMember(x => x.IsAccepted, y => y.MapFrom(z => z.TransactionTypeId == "Accepted" || z.TransactionTypeId == "PreTender" || z.TransactionTypeId == "Pending" || z.TransactionTypeId == "Removed" || z.TransactionTypeId == "Delivered"))
                .ForMember(x => x.EquipmentType, y => y.MapFrom(z => z.EquipmentId))
                .ForMember(x => x.EquipmentDesc, y => y.MapFrom(z => z.Equipment.EquipmentDesc))
                .ForMember(x => x.BookedUser, y => y.MapFrom(z =>
                   (!z.BookedUserId.HasValue || string.IsNullOrEmpty(z.BookedUser)) ? null : new UserContactData()
                   {
                       UserId = z.BookedUserId.HasValue ? z.BookedUserId.Value : Guid.Empty,
                       UserName = z.BookedUser
                   }))
                .ForMember(x => x.LoadTransaction, y => y.MapFrom(z => new LoadTransactionData()
                {
                    LineHaulRate = z.TransactionLineHaulRate,
                    FuelRate = z.TransactionFuelRate,
                    Scac = z.Scac,
                    TransactionType = (TransactionTypeData)Enum.Parse(typeof(TransactionTypeData), z.TransactionTypeId),
                    UserId = z.UserId,
                    LastUpdateTime = z.TransactionLastUpdateTime
                }))
                .ForMember(x => x.IsEstimatedFSC, y => y.MapFrom<LoadDetailViewEntity_LoadData_IsEstimatedFscResolver>())
                .ForMember(x => x.EquipmentTypeDisplay, opt => opt.MapFrom(src => src.Equipment.CategoryEquipmentDesc ?? src.Equipment.EquipmentDesc))
                .ForMember(x => x.EquipmentCategoryDesc, opt => opt.MapFrom(src => src.Equipment.CategoryEquipmentDesc))
                .ForMember(x => x.ServiceTypes, opt => opt.MapFrom(src => src.LoadServiceTypes))
                .ForMember(x => x.FeeData, opt => opt.MapFrom(src =>
                    new LoadshopFeeData
                    {
                        LoadshopFlatFee = src.FlatFee,
                        LoadshopPercentFee = src.PercentFee,
                        LoadshopFeeAdd = src.FeeAdd,
                        LoadshopFee = src.LoadshopFee
                    }
                ));

            CreateMap<LoadDetailViewEntity, NotificationDataEntity>()
                .ForMember(x => x.Origin, y => y.MapFrom(z => $"{ z.LoadStops[0].City},{z.LoadStops[0].State}"))
                .ForMember(x => x.Dest, y => y.MapFrom(z => $"{ z.LoadStops[z.LoadStops.Count - 1].City},{z.LoadStops[z.LoadStops.Count - 1].State}"))
                .ForMember(x => x.OriginDtTm, y => y.MapFrom(z => z.LoadStops[0].LateDtTm))
                .ForMember(x => x.DestDtTm, y => y.MapFrom(z => z.LoadStops[z.LoadStops.Count - 1].LateDtTm))
                .ForMember(x => x.EquipmentDesc, y => y.MapFrom(z => z.EquipmentId))
                .ForMember(x => x.FuelRate, y => y.MapFrom(z => z.FuelRate))
                .ForMember(x => x.Miles, y => y.MapFrom(z => z.Miles));

            CreateMap<LoadDetailData, NotificationDataEntity>()
                .ForMember(x => x.Origin, y => y.MapFrom(z => $"{ z.LoadStops[0].City},{z.LoadStops[0].State}"))
                .ForMember(x => x.Dest, y => y.MapFrom(z => $"{ z.LoadStops[z.LoadStops.Count - 1].City},{z.LoadStops[z.LoadStops.Count - 1].State}"))
                .ForMember(x => x.OriginDtTm, y => y.MapFrom(z => z.LoadStops[0].LateDtTm))
                .ForMember(x => x.DestDtTm, y => y.MapFrom(z => z.LoadStops[z.LoadStops.Count - 1].LateDtTm))
                .ForMember(x => x.EquipmentDesc, y => y.MapFrom(z => z.EquipmentType));

            CreateMap<LoadCarrierScacData, LoadCarrierScacEntity>().ReverseMap();
            CreateMap<LoadCarrierScacRestrictionData, LoadCarrierScacRestrictionEntity>().ReverseMap();
            CreateMap<LoadCarrierScacRestrictionTypeData, LoadCarrierScacRestrictionTypeEntity>().ReverseMap();

            CreateMap<LoadClaimData, LoadClaimEntity>().ReverseMap();

            CreateMap<EquipmentData, EquipmentEntity>().ReverseMap();
            CreateMap<LoadCarrierGroupEntity, LoadCarrierGroupDetailData>()
                .ForMember(x => x.Carriers, y => y.MapFrom(z => z.LoadCarrierGroupCarriers))
                .ReverseMap()
                .ForMember(lcgEntity => lcgEntity.LoadCarrierGroupEquipment, opt => opt.Ignore())
                .ForMember(lcgEntity => lcgEntity.LoadCarrierGroupCarriers, opt => opt.Ignore())
                .ForMember(x => x.LoadCarrierGroupType, opt => opt.Ignore());

            CreateMap<LoadCarrierGroupCarrierEntity, LoadCarrierGroupCarrierData>().ReverseMap();
            CreateMap<LoadCarrierGroupEquipmentEntity, LoadCarrierGroupEquipmentData>().ReverseMap();
            CreateMap<LoadCarrierGroupEntity, ShippingLoadCarrierGroupData>()
                .ForMember(loadCarrierGroupData => loadCarrierGroupData.Carriers,
                    opt => opt.MapFrom(loadCarrierGroupEntity => loadCarrierGroupEntity
                                                                    .LoadCarrierGroupCarriers
                                                                    .Select(loadCarrierGroupCarrier => loadCarrierGroupCarrier.Carrier)))
                .ForMember(loadCarrierGroupData => loadCarrierGroupData.ShippingLoadCarrierGroupDisplay,
                    opt => opt.MapFrom(loadCarrierGroupEntity => $"{loadCarrierGroupEntity.GroupName} - {loadCarrierGroupEntity.LoadCarrierGroupType.Name}"));

            CreateMap<LoadCarrierGroupTypeEntity, LoadCarrierGroupTypeData>();

            CreateMap<CommodityEntity, CommodityData>().ReverseMap();

            CreateMap<LoadEntity, ShippingLoadData>()
                .ForMember(x => x.Mileage, y => y.MapFrom(z => z.Miles))
                .ForMember(x => x.ShippersFSC, y => y.MapFrom(z => z.FuelRate))
                .ForMember(x => x.OnLoadshop, y => y.MapFrom(z => new List<string> { "New", "Updated" }.Contains(z.LatestTransactionTypeId)))
                .ForMember(x => x.IsEstimatedFSC, y => y.MapFrom<LoadEntity_ShippingLoadData_IsEstimatedFscResolver>())
                .ForMember(x => x.EquipmentCategoryDesc, opt => opt.MapFrom(src => src.Equipment.CategoryEquipmentDesc))
                .ForMember(x => x.EquipmentTypeDisplay, opt => opt.MapFrom(src => src.Equipment.CategoryEquipmentDesc ?? src.Equipment.EquipmentDesc))
                .ForMember(x => x.CarrierGroupIds, opt => opt.MapFrom(src => (src.PostedLoadCarrierGroups ?? new List<PostedLoadCarrierGroupEntity>()).Select(carrierGroup => carrierGroup.LoadCarrierGroupId)))
                .ForMember(x => x.ServiceTypes, opt => opt.MapFrom(src => src.LoadServiceTypes));

            CreateMap<LoadAuditLogEntity, LoadAuditLogData>().ReverseMap();

            CreateMap<LoadEntity, LoadHistoryEntity>()
                .ForMember(x => x.LoadHistoryId, opt => opt.Ignore())
                .ForMember(x => x.CreateBy, opt => opt.Ignore())
                .ForMember(x => x.CreateDtTm, opt => opt.Ignore())
                .ForMember(x => x.LastChgBy, opt => opt.Ignore())
                .ForMember(x => x.LastChgDtTm, opt => opt.Ignore())
                .ForMember(x => x.Customer, opt => opt.Ignore())
                .ForMember(x => x.Load, opt => opt.Ignore())
                ;

            CreateMap<SecurityAppActionEntity, SecurityAppActionData>();

            CreateMap<SecurityAccessRoleEntity, SecurityAccessRoleData>()
                //Map Across the Many to Many
                .ForMember(securityAccessRoleData => securityAccessRoleData.AppActions,
                                                        opt => opt.MapFrom(
                                                            securityAccessRoleEntity => securityAccessRoleEntity.SecurityAccessRoleAppActions
                                                                .Select(securityAccessRoleAppActions => securityAccessRoleAppActions.SecurityAppAction)));

            CreateMap<SecurityAccessRoleEntity, SecurityAccessRoleListData>();

            CreateMap<UserCarrierScacEntity, UserCarrierScacData>();

            CreateMap<CarrierEntity, CarrierData>()
                .ForMember(carrierData => carrierData.CarrierScacs,
                                            opt => opt.MapFrom(carrierEntity => carrierEntity.CarrierScacs
                                                                                    .Where(Utilities.QueryFilters.GetActiveCarrierScacFilterAsFunc())
                                                                                    .Select(cs => cs.Scac)));

            CreateMap<CarrierEntity, CarrierProfileData>()
                .ForMember(carrierData => carrierData.CarrierScacs,
                                            opt => opt.MapFrom(carrierEntity => carrierEntity.CarrierScacs
                                                                                    .Select(cs => cs.Scac)))
                .ForMember(carrierData => carrierData.Scacs, opt => opt.MapFrom(carrierEntity => carrierEntity.CarrierScacs))
                .ReverseMap()
                .ForMember(carrierEntity => carrierEntity.CarrierScacs, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.Address, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.City, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.State, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.Zip, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.Country, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.USDOTNbr, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.OperatingAuthNbr, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.RMISCertification, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.IsSourceActive, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.HazMatCertified, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.HasMexAuth, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.HasCanAuth, opt => opt.Ignore())
                .ForMember(carrierEntity => carrierEntity.DataSource, opt => opt.Ignore());

            CreateMap<CarrierScacEntity, CarrierScacData>()
                .ReverseMap()
                .ForMember(carrierScacEntity => carrierScacEntity.Scac, opt => opt.Ignore())
                .ForMember(carrierScacEntity => carrierScacEntity.ScacName, opt => opt.Ignore())
                .ForMember(carrierScacEntity => carrierScacEntity.EffectiveDate, opt => opt.Ignore())
                .ForMember(carrierScacEntity => carrierScacEntity.ExpirationDate, opt => opt.Ignore())
                .ForMember(carrierScacEntity => carrierScacEntity.CarrierId, opt => opt.Ignore())
                .ForMember(carrierScacEntity => carrierScacEntity.IsActive, opt => opt.Ignore())
                .ForMember(carrierScacEntity => carrierScacEntity.DataSource, opt => opt.Ignore());

            CreateMap<CarrierScacEntity, UserFocusEntityData>()
                                            .ForMember(userFocusEntityData => userFocusEntityData.Id, opt => opt.MapFrom(carrierScac => carrierScac.Scac))
                                            .ForMember(userFocusEntityData => userFocusEntityData.Name, opt => opt.MapFrom(carrierScac => carrierScac.Scac))
                                            .ForMember(userCarrierScacData => userCarrierScacData.Group, opt => opt.MapFrom(carrirScac => carrirScac.Carrier.CarrierName))
                                            .ForMember(userFocusEntityData => userFocusEntityData.Type, opt => opt.MapFrom(carrierScac => UserFocusEntityType.CarrierScac));

            CreateMap<CustomerEntity, UserFocusEntityData>()
                                           .ForMember(userFocusEntityData => userFocusEntityData.Id, opt => opt.MapFrom(customer => customer.CustomerId))
                                           .ForMember(userFocusEntityData => userFocusEntityData.Name, opt => opt.MapFrom(customer => customer.Name))
                                           .ForMember(userCarrierScacData => userCarrierScacData.Group, opt => opt.MapFrom(customer => "Shippers"))
                                           .ForMember(userFocusEntityData => userFocusEntityData.Type, opt => opt.MapFrom(customer => UserFocusEntityType.Shipper));

            CreateMap<CustomerCarrierScacContractEntity, CustomerCarrierScacContractData>().ReverseMap();

            CreateMap<UserShipperEntity, UserShipperData>().ReverseMap();

            CreateMap<UserEntity, UserData>()
                .ForMember(userData => userData.ShipperIds, opt => opt.MapFrom(userEntity => userEntity.UserShippers.Select(us => us.CustomerId)))
                .ForMember(userData => userData.SecurityRoleIds, opt => opt.MapFrom(userEntity => userEntity.SecurityUserAccessRoles.Select(us => us.AccessRoleId)))
                .ForMember(userData => userData.SecurityRoles,
                    src => src.MapFrom(userEntity =>
                        userEntity.SecurityUserAccessRoles == null ? new List<SecurityAccessRoleData>() : userEntity.SecurityUserAccessRoles
                        .Select(x => x.SecurityAccessRole == null ? new SecurityAccessRoleData() : new SecurityAccessRoleData
                        {   // Do not include all the app actions and other unnecessary role data, which makes for a large payload
                            AccessRoleId = x.SecurityAccessRole.AccessRoleId,
                            AccessRoleName = x.SecurityAccessRole.AccessRoleName
                        })
                        .Distinct()))
                .ReverseMap()
                .ForMember(userEntity => userEntity.UserNotifications, opt => opt.Ignore());

            CreateMap<AppointmentSchedulerConfirmationTypeEntity, AppointmentSchedulerConfirmationTypeData>().ReverseMap();
            CreateMap<StopTypeEntity, StopTypeData>().ReverseMap();
            CreateMap<TransportationModeEntity, TransportationModeData>().ReverseMap();
            CreateMap<UnitOfMeasureEntity, UnitOfMeasureData>().ReverseMap();
            CreateMap<ServiceTypeEntity, ServiceTypeData>().ReverseMap();
            CreateMap<LoadServiceTypeEntity, ServiceTypeData>()
                .ForMember(x => x.Name, y => y.MapFrom(z => z.ServiceType != null ? z.ServiceType.Name : string.Empty))
                .ReverseMap();

            CreateMap<LoadEntity, OrderEntryLoadDetailData>()
                .ForMember(x => x.EquipmentType, y => y.MapFrom(z => z.Equipment.EquipmentId))
                .ForMember(x => x.EquipmentDesc, y => y.MapFrom(z => z.Equipment.EquipmentDesc))
                .ForMember(x => x.CategoryEquipmentDesc, y => y.MapFrom(z => z.Equipment.CategoryEquipmentDesc))
                .ForMember(x => x.OnLoadshop, y => y.MapFrom(z => new List<string> { "New", "Updated" }.Contains(z.LatestTransactionTypeId)))
                .ForMember(x => x.ServiceTypes, y => y.MapFrom((src, dest) => src.LoadServiceTypes?.Select(_ => new ServiceTypeData { ServiceTypeId = _.ServiceTypeId, Name = _.ServiceType?.Name })));

            CreateMap<LoadStopEntity, OrderEntryLoadStopData>()
                .ForMember(x => x.AppointmentConfirmationCode, y => y.MapFrom((src, dest) => src.AppointmentSchedulerConfirmationType?.AppointmentConfirmationCode))
                .ForMember(x => x.AppointmentSchedulingCode, y => y.MapFrom((src, dest) => src.AppointmentSchedulerConfirmationType?.AppointmentSchedulingCode))
                .ReverseMap();

            CreateMap<LoadStopData, OrderEntryLoadStopData>().ReverseMap();
            CreateMap<LoadLineItemData, LoadLineItemEntity>().ReverseMap();

            CreateMap<LocationEntity, LocationData>()
                .ReverseMap()
                .ForMember(x => x.LocationId, opt => opt.Ignore());

            CreateMap<SpecialInstructionEntity, SpecialInstructionData>()
               .ReverseMap()
               .ForMember(x => x.SpecialInstructionEquipment, opt => opt.Ignore());

            CreateMap<SpecialInstructionEquipmentEntity, SpecialInstructionEquipmentData>()
               .ReverseMap();

            CreateMap<CustomerLoadTypeEntity, CustomerLoadTypeData>().ReverseMap();

            CreateMap<RatingQuestionEntity, RatingQuestionData>().ReverseMap();
            CreateMap<RatingQuestionAnswerEntity, RatingQuestionAnswerData>().ReverseMap();

            CreateMap<UserCommunicationEntity, UserCommunicationData>();

            CreateMap<UserCommunicationEntity, UserCommunicationDetailData>()
                .ReverseMap()
                .ForMember(userCommunicationEntity => userCommunicationEntity.UserCommunicationShippers, opt => opt.Ignore())
                .ForMember(userCommunicationEntity => userCommunicationEntity.UserCommunicationCarriers, opt => opt.Ignore())
                .ForMember(userCommunicationEntity => userCommunicationEntity.UserCommunicationUsers, opt => opt.Ignore())
                .ForMember(userCommunicationEntity => userCommunicationEntity.UserCommunicationSecurityAccessRoles, opt => opt.Ignore())
                .ForMember(userCommunicationEntity => userCommunicationEntity.OwnerId, opt => opt.Ignore());

            CreateMap<UserCommunicationShipperEntity, UserCommunicationShipperData>().ReverseMap();
            CreateMap<UserCommunicationCarrierEntity, UserCommunicationCarrierData>().ReverseMap();
            CreateMap<UserCommunicationUserEntity, UserCommunicationUserData>().ReverseMap();
            CreateMap<UserCommunicationSecurityAccessRoleEntity, UserCommunicationSecurityAccessRoleData>().ReverseMap();

            CreateMap<LoadStatusTransactionEntity, LoadStatusTransactionData>()
                .ReverseMap();

            CreateMap<LoadStatusTransactionEntity, LoadStatusTransactionData>()
                .ReverseMap();

            CreateMap<LoadDocumentEntity, LoadDocumentMetadata>()
                .ForMember(x => x.LoadDocumentType, opt => opt.MapFrom(x => new LoadDocumentTypeData()
                {
                    Id = x.DocumentServiceDocumentType
                }))
                .ReverseMap();

            CreateMap<AgreementDocumentEntity, AgreementDocumentData>()
                .ReverseMap();
        }
    }
}
