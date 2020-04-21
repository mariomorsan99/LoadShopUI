using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Microsoft.EntityFrameworkCore;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Validation.Services;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class LoadCarrierGroupService : ILoadCarrierGroupService
    {
        private readonly LoadshopDataContext _context;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;

        public LoadCarrierGroupService(LoadshopDataContext context, ICommonService commonService, IMapper mapper, IUserContext userContext, ISecurityService securityService)
        {
            _context = context;
            _commonService = commonService;
            _mapper = mapper;
            _userContext = userContext;
            _securityService = securityService;
        }

        public LoadCarrierGroupDetailData GetLoadCarrierGroup(long loadCarrierGroupId)
        {
            var entity = _context.LoadCarrierGroups
                .Include(x => x.LoadCarrierGroupType)
                .Include(x => x.LoadCarrierGroupCarriers)
                .Include(x => x.LoadCarrierGroupEquipment)
                .ThenInclude(x => x.Equipment)
                .Where(x => x.LoadCarrierGroupId == loadCarrierGroupId)
                .Select(x => new
                {
                    Group = x,
                    CarrierCount = x.LoadCarrierGroupCarriers.Count()
                })
                .SingleOrDefault();
            if (entity == null)
                throw new Exception("Load Carrier Group not found.");

            GuardCustomer(entity.Group.CustomerId);

            var group = _mapper.Map<LoadCarrierGroupDetailData>(entity.Group);
            group.CarrierCount = entity.CarrierCount;
            return group;
        }

        public List<LoadCarrierGroupDetailData> GetLoadCarrierGroups()
        {
            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;

            var entities = _context.LoadCarrierGroups
                .Include(x => x.LoadCarrierGroupType)
                .Include(x => x.LoadCarrierGroupEquipment)
                .ThenInclude(x => x.Equipment)
                .Where(x => x.CustomerId == userPrimaryCustomerId)
                .Select(x => new
                {
                    Group = x,
                    CarrierCount = x.LoadCarrierGroupCarriers.Count()
                })
                .ToList();

            var groups = new List<LoadCarrierGroupDetailData>();
            foreach (var e in entities)
            {
                var group = _mapper.Map<LoadCarrierGroupDetailData>(e.Group);
                group.CarrierCount = e.CarrierCount;
                groups.Add(group);
            }
            return groups;
        }

        public SaveLoadCarrierGroupResponse CreateLoadCarrierGroup(LoadCarrierGroupDetailData group, string username)
        {
            var response = new SaveLoadCarrierGroupResponse();
            try
            {
                _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit);

                ConvertStatesToAbbreviations(group);
                if (group.LoadCarrierGroupId > 0)
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", "LoadCarrierGroup should not have an LoadCarrierGroupId assigned when creating.");
                    return response;
                }

                var validationErrorMessage = ValidateLoadCarrierGroup(group);
                if (!string.IsNullOrWhiteSpace(validationErrorMessage))
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", validationErrorMessage);
                    return response;
                }

                ValidateLoadCarrierGroupCarriers(group.Carriers, group.LoadCarrierGroupId);

                var dbGroup = _mapper.Map<LoadCarrierGroupEntity>(group);

                //Map Equipment Types
                dbGroup.LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentEntity>();
                dbGroup.LoadCarrierGroupEquipment.MapList(
                    group.LoadCarrierGroupEquipment,
                    lcgeEntity => lcgeEntity.LoadCarrierGroupEquipmentId,
                    lcgeData => lcgeData.LoadCarrierGroupEquipmentId, _mapper);

                dbGroup.LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>();
                dbGroup.LoadCarrierGroupCarriers.MapList(
                    group.Carriers,
                    lcgcEntity => lcgcEntity.LoadCarrierGroupCarrierId,
                    legcData => legcData.LoadCarrierGroupCarrierId,
                    _mapper);

                GuardCustomer(dbGroup.CustomerId);

                if (IsUnique(dbGroup))
                {

                    _context.LoadCarrierGroups.Add(dbGroup);
                    //_context.LoadCarrierGroupEquipment.AddRange(dbGroup.LoadCarrierGroupEquipment);
                    _context.SaveChanges(username);
                    response.LoadCarrierGroupData = GetLoadCarrierGroup(dbGroup.LoadCarrierGroupId);
                }
                else
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", GetCarrierGroupUniqueConstraintErrorMessage(group));
                }
            }
            catch (DbUpdateException ex)
            {
                //TODO: Improve to use sql database conditions
                if (ex.InnerException != null && ex.InnerException.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", GetCarrierGroupUniqueConstraintErrorMessage(group));
                    return response;
                }
                throw;
            }
            return response;
        }

        public SaveLoadCarrierGroupResponse UpdateLoadCarrierGroup(LoadCarrierGroupDetailData group, string username)
        {
            var response = new SaveLoadCarrierGroupResponse();
            try
            {

                _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit);

                ConvertStatesToAbbreviations(group);

                if (group.LoadCarrierGroupId <= 0)
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", "LoadCarrierGroup should have an LoadCarrierGroupId assigned when updating.");
                    return response;
                }

                var validationErrorMessage = ValidateLoadCarrierGroup(group);
                if (!string.IsNullOrWhiteSpace(validationErrorMessage))
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", validationErrorMessage);
                    return response;
                }

                ValidateLoadCarrierGroupCarriers(group.Carriers, group.LoadCarrierGroupId);

                var dbGroup = _context.LoadCarrierGroups
                                        .Include(x => x.LoadCarrierGroupCarriers)
                                        .Include(x => x.LoadCarrierGroupEquipment)
                                        .SingleOrDefault(x => x.LoadCarrierGroupId == group.LoadCarrierGroupId);
                if (dbGroup == null)
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", "LoadCarrierGroup not found");
                    return response;
                }

                GuardCustomer(dbGroup.CustomerId);

                _mapper.Map(group, dbGroup);

                //Update Load Carrier Group Equipment Entity
                dbGroup.LoadCarrierGroupEquipment.MapList(
                    group.LoadCarrierGroupEquipment,
                    lcgeEntity => lcgeEntity.LoadCarrierGroupEquipmentId,
                    legeData => legeData.LoadCarrierGroupEquipmentId,
                    _mapper);

                dbGroup.LoadCarrierGroupCarriers.MapList(
                    group.Carriers,
                    lcgcEntity => lcgcEntity.LoadCarrierGroupCarrierId,
                    legcData => legcData.LoadCarrierGroupCarrierId,
                    _mapper);

                if (IsUnique(dbGroup))
                {
                    _context.SaveChanges(username);

                    response.LoadCarrierGroupData = GetLoadCarrierGroup(dbGroup.LoadCarrierGroupId);
                }
                else
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", GetCarrierGroupUniqueConstraintErrorMessage(group));
                }
            }
            catch (DbUpdateException ex)
            {
                //TODO: Improve to use sql database conditions
                if (ex.InnerException != null && ex.InnerException.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    response.ModelState.AddModelError($"urn:LoadCarrierGroup", GetCarrierGroupUniqueConstraintErrorMessage(group));
                    return response;
                }
                throw;
            }
            return response;
        }

        private static string GetCarrierGroupUniqueConstraintErrorMessage(LoadCarrierGroupDetailData group)
        {
            var msg = new StringBuilder("A carrier group already exists for:" + Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(group.OriginAddress1)) msg.Append($"Origin Address1 - {group.OriginAddress1}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginCity)) msg.Append($"Origin City - {group.OriginCity}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginState)) msg.Append($"Origin State - {group.OriginState}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginPostalCode)) msg.Append($"Origin Postal Code - {group.OriginPostalCode}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginCountry)) msg.Append($"Origin Country - {group.OriginCountry}{Environment.NewLine}");

            if (!string.IsNullOrWhiteSpace(group.DestinationAddress1)) msg.Append($"Destination Address1 - {group.DestinationAddress1}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationCity)) msg.Append($"Destination City - {group.DestinationCity}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationState)) msg.Append($"Destination State - {group.DestinationState}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationPostalCode)) msg.Append($"Destination Postal Code - {group.DestinationPostalCode}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationCountry)) msg.Append($"Destination Country - {group.DestinationCountry}{Environment.NewLine}");

            if (group.LoadCarrierGroupEquipment.Any()) msg.Append($"Equipment Type(s) - {string.Join(", ", group.LoadCarrierGroupEquipment.Select(lcge => lcge.EquipmentId))}{Environment.NewLine}");
            return msg.ToString();
        }

        public void DeleteLoadCarrierGroup(long id)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit);

            var dbGroup = _context.LoadCarrierGroups.SingleOrDefault(x => x.LoadCarrierGroupId == id);
            if (dbGroup == null)
            {
                throw new Exception($"LoadCarrierGroup not found in the database with LoadCarrierGroupId: {id}");
            }

            GuardCustomer(dbGroup.CustomerId);

            // remove all equipment
            var equipment = _context.LoadCarrierGroupEquipment.Where(x => x.LoadCarrierGroupId == id).ToList();
            _context.LoadCarrierGroupEquipment.RemoveRange(equipment);

            // remove all carriers
            var carriers = _context.LoadCarrierGroupCarriers.Where(x => x.LoadCarrierGroupId == id).ToList();
            _context.LoadCarrierGroupCarriers.RemoveRange(carriers);

            _context.LoadCarrierGroups.Remove(dbGroup);
            _context.SaveChanges();
        }

        public List<LoadCarrierGroupCarrierData> GetLoadCarrierGroupCarriers(long loadCarrierGroupId)
        {
            var loadCarrierGroupEntity = _context.LoadCarrierGroups
                                                    .Include(lcg => lcg.LoadCarrierGroupCarriers)
                                                    .SingleOrDefault(lcg => lcg.LoadCarrierGroupId == loadCarrierGroupId);

            if (loadCarrierGroupEntity == null)
            {
                throw new Exception($"LoadCarrierGroup not found in the database with LoadCarrierGroupId: {loadCarrierGroupId}");
            }

            GuardCustomer(loadCarrierGroupEntity.CustomerId);

            return _mapper.Map<List<LoadCarrierGroupCarrierData>>(loadCarrierGroupEntity.LoadCarrierGroupCarriers);
        }

        public List<LoadCarrierGroupCarrierData> AddLoadCarrierGroupCarriers(List<LoadCarrierGroupCarrierData> carriers, string username)
        {
            long? loadCarrierGroupId = null;
            try
            {
                _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit);

                loadCarrierGroupId = carriers?.Select(_ => _.LoadCarrierGroupId).FirstOrDefault();
                if (loadCarrierGroupId <= 0)
                    throw new Exception("LoadCarrierGroupCarrier must have a LoadCarrierGroupId.");

                var loadCarrierGroupCustomerId = _context.LoadCarrierGroups
                                                            .Where(lcg => lcg.LoadCarrierGroupId == loadCarrierGroupId)
                                                            .Select(lcg => lcg.CustomerId)
                                                            .SingleOrDefault();

                GuardCustomer(loadCarrierGroupCustomerId);

                foreach (var carrier in carriers)
                {
                    if (carrier.LoadCarrierGroupCarrierId > 0)
                        throw new Exception($"LoadCarrierGroupCarrier should not have an LoadCarrierGroupCarrierId assigned when creating.");
                    if (carrier.LoadCarrierGroupId <= 0)
                        throw new Exception($"LoadCarrierGroupCarrier must have a LoadCarrierGroupId.");
                    if (carrier.LoadCarrierGroupId != loadCarrierGroupId)
                        throw new Exception($"All carriers being added must have the same LoadCarrierGroupId.");
                    if (string.IsNullOrWhiteSpace(carrier.CarrierId))
                        throw new Exception($"LoadCarrierGroupCarrier must have a CarrierId.");
                }

                var dbCarriers = _mapper.Map<List<LoadCarrierGroupCarrierEntity>>(carriers);

                _context.LoadCarrierGroupCarriers.AddRange(dbCarriers);
                _context.SaveChanges(username);

                return _mapper.Map<List<LoadCarrierGroupCarrierData>>(dbCarriers); ;
            }
            catch (DbUpdateException ex)
            {
                //TODO: Improve to use sql database conditions
                if (ex.InnerException != null && ex.InnerException.Message.Contains("Violation of UNIQUE KEY constraint"))
                {
                    StringBuilder msg = new StringBuilder("A carrier was attempted to be added that already exists for:" + Environment.NewLine);
                    msg.Append($"Load Carrier Group Id - {loadCarrierGroupId}{Environment.NewLine}");
                    throw new Exception(msg.ToString());
                }
                throw;
            }

        }

        public void DeleteLoadCarrierGroupCarrier(long loadCarrierGroupCarrierId)
        {

            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit);

            var dbCarrier = _context.LoadCarrierGroupCarriers.Include(lcgc => lcgc.LoadCarrierGroup).SingleOrDefault(x => x.LoadCarrierGroupCarrierId == loadCarrierGroupCarrierId);

            if (dbCarrier == null)
            {
                throw new Exception($"LoadCarrierGroupCarrier not found in the database with LoadCarrierGroupCarrierId: {loadCarrierGroupCarrierId}");
            }

            GuardCustomer(dbCarrier.LoadCarrierGroup.CustomerId);

            _context.LoadCarrierGroupCarriers.Remove(dbCarrier);
            _context.SaveChanges();
        }

        public void DeleteAllLoadCarrierGroupCarriers(long loadCarrierGroupId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit);

            var loadCarrierGroup = _context.LoadCarrierGroups
                                                .Include(lcg => lcg.LoadCarrierGroupCarriers)
                                                .SingleOrDefault(x => x.LoadCarrierGroupId == loadCarrierGroupId);

            if (loadCarrierGroup == null)
            {
                throw new Exception($"LoadCarrierGroup not found in the database with LoadCarrierGroupId: {loadCarrierGroupId}");
            }

            GuardCustomer(loadCarrierGroup.CustomerId);

            _context.LoadCarrierGroupCarriers.RemoveRange(loadCarrierGroup.LoadCarrierGroupCarriers);
            _context.SaveChanges();
        }

        public IList<ShippingLoadCarrierGroupData> GetLoadCarrierGroupsForLoad(Guid loadId)
        {
            //TODO: Add validation that user belongs to customer

            var load = _context.Loads
                .Include(_ => _.LoadStops)
                .SingleOrDefault(_ => _.LoadId == loadId);

            if (load == null)
            {
                throw new Exception($"Could not find load: {loadId}");
            }

            GuardCustomer(load.CustomerId);

            var orderedStops = load.LoadStops?.OrderBy(_ => _.StopNbr).ToList();
            var origin = orderedStops?.FirstOrDefault();
            var destination = orderedStops?.LastOrDefault();

            if (origin == null || destination == null)
                return null;

            var carriers = _securityService.GetContractedCarriersByPrimaryCustomerIdAsync();
            var matchingGroups = _context.LoadCarrierGroups
                .Include(_ => _.LoadCarrierGroupType)
                .Include(_ => _.LoadCarrierGroupCarriers)
                .ThenInclude(_ => _.Carrier)
                .Where(_ => _.CustomerId == load.CustomerId
                    && (_.OriginCity == null || _.OriginCity == origin.City)
                    && (_.OriginState == null || _.OriginState == origin.State)
                    && (_.OriginPostalCode == null || _.OriginPostalCode == origin.PostalCode)
                    && (_.OriginCountry == null || _.OriginCountry == origin.Country)
                    && (_.DestinationCity == null || _.DestinationCity == destination.City)
                    && (_.DestinationState == null || _.DestinationState == destination.State)
                    && (_.DestinationPostalCode == null || _.DestinationPostalCode == destination.PostalCode)
                    && (_.DestinationCountry == null || _.DestinationCountry == destination.Country)
                    && (!_.LoadCarrierGroupEquipment.Any() || _.LoadCarrierGroupEquipment.Any(loadCarrierGroupEquipment => loadCarrierGroupEquipment.EquipmentId == load.EquipmentId))
                    )
                .ToListAsync();

            Task.WaitAll(carriers, matchingGroups);

            // Address standardization has to happen after initial query
            var groups = matchingGroups.Result.Where(_ =>
                (_.OriginAddress1 == null || AddressValidationService.StandardizeAddress(origin.Address1, true)
                    .Equals(AddressValidationService.StandardizeAddress(_.OriginAddress1, true), StringComparison.OrdinalIgnoreCase))
                && (_.DestinationAddress1 == null || AddressValidationService.StandardizeAddress(destination.Address1, true)
                    .Equals(AddressValidationService.StandardizeAddress(_.DestinationAddress1, true), StringComparison.OrdinalIgnoreCase))
            ).ToList();

            var matchingGroupData = _mapper.Map<IList<ShippingLoadCarrierGroupData>>(groups);

            foreach (var group in matchingGroupData)
            {
                var carrierGroupType = (LoadCarrierGroupTypeEnum)group.LoadCarrierGroupTypeId;

                if (carrierGroupType == LoadCarrierGroupTypeEnum.Exclude)
                {
                    var excludeIds = group.Carriers.Select(carrier => carrier.CarrierId).ToArray();

                    var carrierList = carriers.Result
                                            .Where(x => !excludeIds.Contains(x.CarrierId))
                                            .ToList();

                    group.Carriers = carrierList;
                }
                //Make sure we are returning only carriers scacs that the customer has a contract with
                else
                {
                    var includeIds = group.Carriers.Select(carrier => carrier.CarrierId).ToArray();

                    var carrierList = carriers.Result
                                             .Where(x => includeIds.Contains(x.CarrierId))
                                             .ToList();

                    group.Carriers = carrierList;
                }
            }

            return matchingGroupData.Where(group => group.Carriers.Any()).ToList();
        }

        public List<LoadCarrierGroupTypeData> GetLoadCarrierGroupTypes()
        {
            return _mapper.Map<List<LoadCarrierGroupTypeData>>(_context.LoadCarrierGroupTypes);
        }

        private void ConvertStatesToAbbreviations(LoadCarrierGroupDetailData loadCarrierGroupData)
        {
            if (loadCarrierGroupData != null)
            {
                loadCarrierGroupData.OriginState = ConvertStateToAbbreviation(loadCarrierGroupData.OriginState);
                loadCarrierGroupData.DestinationState = ConvertStateToAbbreviation(loadCarrierGroupData.DestinationState);
            }
        }

        private string ConvertStateToAbbreviation(string stateName)
        {
            if (!string.IsNullOrWhiteSpace(stateName))
            {
                var state = _commonService.GetUSCANStateProvince(stateName);
                if (state != null)
                {
                    stateName = state.Abbreviation;
                }
            }

            return stateName;
        }

        private static string ValidateLoadCarrierGroup(LoadCarrierGroupDetailData group)
        {
            var errors = new StringBuilder();

            if (group.CustomerId == Guid.Empty)
                errors.AppendLine("Must have a Customer");

            if (string.IsNullOrEmpty(group.GroupName))
            {
                errors.AppendLine("Must have a name");
            }

            var validOrigin = ValidateLocation(group.OriginCity, group.OriginState, group.OriginCountry);
            var validDest = ValidateLocation(group.DestinationCity, group.DestinationState, group.DestinationCountry);
            var validEquipment = (group.LoadCarrierGroupEquipment?.Any()).GetValueOrDefault();

            if (!validOrigin && !validDest && !validEquipment)
            {
                errors.AppendLine("Must have an Origin, Destination, or Equipment");
            }

            if (errors.Length > 0)
            {
                return errors.ToString();
            }
            return string.Empty;
        }

        private static bool ValidateLocation(string city, string state, string country)
        {
            if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(state) && string.IsNullOrWhiteSpace(country))
            {
                return false;
            }

            return true;
        }

        private void GuardCustomer(Guid customerId)
        {
            if (!_securityService.IsAuthorizedForCustomer(customerId))
                throw new UnauthorizedAccessException($"User is not authorized for customer: {customerId}");
        }

        private bool IsUnique(LoadCarrierGroupEntity loadCarrierGroupData)
        {
            var equipmentIds = loadCarrierGroupData.LoadCarrierGroupEquipment.Select(lcge => lcge.EquipmentId).ToArray();

            return !_context.LoadCarrierGroups.Where(lcg =>
                lcg.CustomerId == loadCarrierGroupData.CustomerId
                && lcg.LoadCarrierGroupId != loadCarrierGroupData.LoadCarrierGroupId
                && lcg.OriginCity == loadCarrierGroupData.OriginCity
                && lcg.OriginState == loadCarrierGroupData.OriginState
                && lcg.OriginCountry == loadCarrierGroupData.OriginCountry
                && lcg.DestinationCity == loadCarrierGroupData.DestinationCity
                && lcg.DestinationState == loadCarrierGroupData.DestinationState
                && lcg.DestinationCountry == loadCarrierGroupData.DestinationCountry
                && lcg.LoadCarrierGroupEquipment.Select(lcge => lcge.EquipmentId).Intersect(equipmentIds).Count() == equipmentIds.Count())
                .Any();
        }

        private void ValidateLoadCarrierGroupCarriers(List<LoadCarrierGroupCarrierData> carriers, long? loadCarrierGroupId)
        {
            foreach (var carrier in carriers)
            {
                if (carrier.LoadCarrierGroupId != loadCarrierGroupId)
                    throw new Exception($"All carriers being added must have the same LoadCarrierGroupId.");
                if (string.IsNullOrWhiteSpace(carrier.CarrierId))
                    throw new Exception($"LoadCarrierGroupCarrier must have a CarrierId.");
            }
        }
    }
}
