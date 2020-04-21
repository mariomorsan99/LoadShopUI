using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Microsoft.EntityFrameworkCore;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Constants;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class UserLaneService : IUserLaneService
    {
        private const int DefaultDHMiles = 50;
        private readonly LoadshopDataContext _context;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly ISecurityService _securityService;

        public UserLaneService(LoadshopDataContext context, ICommonService commonService, ISecurityService securityService, IMapper mapper)
        {
            _context = context;
            _commonService = commonService;
            _securityService = securityService;
            _mapper = mapper;
        }

        public async Task<List<UserLaneData>> GetSavedLanesAsync(Guid userId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Profile_Favorites_View);

            var lanes = await _context.UserLanes
                .Include(x => x.UserLaneMessageTypes)
                .Include(x => x.UserLaneEquipments)
                .Where(x => x.User.IdentUserId == userId)
                .ToListAsync();
            var laneData = _mapper.Map<List<UserLaneData>>(lanes);

            var userLaneNotificationMessageTypes = await _context.MessageTypes
                .Where(x => x.MessageTypeId == MessageTypeConstants.Email || x.MessageTypeId == MessageTypeConstants.CellPhone)
                .ToListAsync();

            foreach (var messageType in userLaneNotificationMessageTypes)
            {
                foreach (var lane in laneData)
                {
                    var userMessageType = lane.UserLaneMessageTypes.SingleOrDefault(x => x.MessageTypeId == messageType.MessageTypeId);
                    if (userMessageType != null) continue;
                    lane.UserLaneMessageTypes.Add(_mapper.Map<UserLaneMessageTypeData>(messageType));
                }
            }
            return laneData;
        }

        public async Task<UserLaneData> CreateLaneAsync(UserLaneData lane, Guid identUserId, string username)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Profile_Favorites_Add_Edit);

            ConvertStatesToAbbreviations(lane);
            ValidateUserLane(lane);
            var dbLane = await _context.UserLanes.SingleOrDefaultAsync(x => x.UserLaneId.ToString() == lane.UserLaneId);
            if (dbLane != null)
            {
                throw new Exception($"User Lane already exists");
            }
            if (lane.EquipmentIds?.Count() == 0)
            {
                throw new Exception("Must have at least on equipment type selected");
            }
            var user = await _context.Users.SingleOrDefaultAsync(x => x.IdentUserId == identUserId);
            if (user == null)
            {
                throw new Exception("Invalid userId");
            }

            lane.UserLaneId = Guid.NewGuid().ToString();
            lane.UserId = user.UserId.ToString();
            dbLane = _mapper.Map<UserLaneEntity>(lane);
            foreach (var laneNotification in lane.UserLaneMessageTypes)
            {
                if (laneNotification.Selected)
                {
                    var dbLaneNotification = _mapper.Map<UserLaneMessageTypeEntity>(laneNotification);
                    dbLane.UserLaneMessageTypes.Add(dbLaneNotification);
                }
            }
            if (dbLane.UserLaneEquipments == null)
            {
                dbLane.UserLaneEquipments = new List<UserLaneEquipmentEntity>();
            }
            
            foreach (var equipment in lane.EquipmentIds)
            {
                var e = new UserLaneEquipmentEntity()
                {
                    EquipmentId = equipment
                };
                dbLane.UserLaneEquipments.Add(e);
            }
            
            _context.UserLanes.Add(dbLane);
            await _context.SaveChangesAsync(username);
            return await GetSavedLaneAsync(dbLane.UserLaneId);
        }

        public async Task<UserLaneData> UpdateLaneAsync(UserLaneData lane, Guid identUserId, string username)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Profile_Favorites_Add_Edit);

            ConvertStatesToAbbreviations(lane);
            ValidateUserLane(lane);
            var dbLane = await _context.UserLanes.SingleOrDefaultAsync(x => x.UserLaneId.ToString() == lane.UserLaneId);
            if (dbLane == null)
            {
                throw new Exception($"User Lane not found");
            }
            if (lane.EquipmentIds?.Count() == 0)
            {
                throw new Exception("Must have at least on equipment type selected");
            }
            var user = await _context.Users.SingleOrDefaultAsync(x => x.IdentUserId == identUserId);
            if (user == null)
            {
                throw new Exception("Invalid userId");
            }

            lane.UserId = user.UserId.ToString();
            dbLane = _mapper.Map(lane, dbLane);

            if (lane.OrigLat.HasValue && lane.OrigLng.HasValue && !dbLane.OrigDH.HasValue)
            {
                dbLane.OrigDH = DefaultDHMiles;
            }

            if (lane.DestLat.HasValue && lane.DestLng.HasValue && !dbLane.DestDH.HasValue)
            {
                dbLane.DestDH = DefaultDHMiles;
            }

            foreach (var laneNotification in lane.UserLaneMessageTypes)
            {
                var dbLaneNotification = await _context.UserLaneMessageTypes
                    .SingleOrDefaultAsync(x => x.UserLaneId == dbLane.UserLaneId && x.MessageTypeId == laneNotification.MessageTypeId);

                if (laneNotification.Selected && dbLaneNotification == null)
                {
                    dbLaneNotification = _mapper.Map<UserLaneMessageTypeEntity>(laneNotification);
                    dbLaneNotification.UserLaneId = dbLane.UserLaneId;
                    _context.UserLaneMessageTypes.Add(dbLaneNotification);
                }
                if (!laneNotification.Selected && dbLaneNotification != null)
                {
                    _context.UserLaneMessageTypes.Remove(dbLaneNotification);
                }
            }

            foreach (var equipmentId in lane.EquipmentIds)
            {
                var dbLaneEquipment = await _context.UserLaneEquipments
                    .SingleOrDefaultAsync(x => x.UserLaneId == dbLane.UserLaneId && x.EquipmentId == equipmentId);

                if (dbLaneEquipment == null)
                {
                    dbLaneEquipment = new UserLaneEquipmentEntity()
                    {
                        EquipmentId = equipmentId,
                        UserLaneId = dbLane.UserLaneId
                    };
                    _context.UserLaneEquipments.Add(dbLaneEquipment);
                }
            }

            var dbLaneEquipments = await _context.UserLaneEquipments.Where(x => x.UserLaneId == dbLane.UserLaneId).ToListAsync();
            foreach (var dbLaneEquipment in dbLaneEquipments)
            {
                if (!lane.EquipmentIds.Contains(dbLaneEquipment.EquipmentId))
                {
                    _context.UserLaneEquipments.Remove(dbLaneEquipment);
                }
            }
            await _context.SaveChangesAsync(username);
            return await GetSavedLaneAsync(dbLane.UserLaneId);
        }

        public async Task DeleteLaneAsync(Guid id)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Profile_Favorites_Add_Edit);

            var dbLane = await _context.UserLanes.SingleOrDefaultAsync(x => x.UserLaneId == id);
            if (dbLane == null)
            {
                throw new Exception($"UserLane not found");
            }
            _context.UserLanes.Remove(dbLane);
            await _context.SaveChangesAsync();
        }

        private async Task<UserLaneData> GetSavedLaneAsync(Guid id)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_Profile_Favorites_View);

            var lane = await _context.UserLanes
                .Include(x => x.UserLaneMessageTypes)
                .SingleOrDefaultAsync(x => x.UserLaneId == id);
            var laneData = _mapper.Map<UserLaneData>(lane);

            var types = await _context.MessageTypes.Where(x => x.MessageTypeId != "Email_SingleCarrierScac").ToListAsync();
            foreach (var messageType in types)
            {
                var userMessageType = laneData.UserLaneMessageTypes.SingleOrDefault(x => x.MessageTypeId == messageType.MessageTypeId);
                if (userMessageType != null) continue;
                laneData.UserLaneMessageTypes.Add(_mapper.Map<UserLaneMessageTypeData>(messageType));
            }
            return laneData;
        }

        private void ConvertStatesToAbbreviations(UserLaneData userLaneData)
        {
            if (userLaneData != null)
            {
                userLaneData.OrigState = ConvertStateToAbbreviation(userLaneData.OrigState);
                userLaneData.DestState = ConvertStateToAbbreviation(userLaneData.DestState);
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

        private void ValidateUserLane(UserLaneData userLaneData)
        {
            var errors = new StringBuilder();

            var validOrigin = ValidatePoint(userLaneData.OrigCity, userLaneData.OrigState, userLaneData.OrigCountry, userLaneData.OrigLat, userLaneData.OrigLng);
            var validDest = ValidatePoint(userLaneData.DestCity, userLaneData.DestState, userLaneData.DestCountry, userLaneData.DestLat, userLaneData.DestLng);

            if (!validOrigin && !validDest)
            {
                errors.AppendLine("Must have an Origin or Destination");
            }

            if (userLaneData.UserLaneMessageTypes == null)
            {
                errors.AppendLine("Missing notification types");
            }

            if (errors.Length > 0)
            {
                throw new ValidationException(errors.ToString());
            }
        }

        private bool ValidatePoint(string city, string state, string country, decimal? lat, decimal? lng)
        {
            // Statewide check
            if (!lat.HasValue && !lng.HasValue && !string.IsNullOrEmpty(state))
            {
                return true;
            }
            // Lat/Lng check
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(country))
            {
                return false;
            }

            if (!lat.HasValue || !lng.HasValue)
            {
                return false;
            }

            return true;
        }
    }
}
