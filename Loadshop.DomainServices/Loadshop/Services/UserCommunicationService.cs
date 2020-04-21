using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Loadshop.DomainServices.Common.Services.Crud;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Microsoft.Extensions.Logging;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class UserCommunicationService : CrudService<UserCommunicationEntity, UserCommunicationDetailData, LoadshopDataContext>, IUserCommunicationService
    {
        public UserCommunicationService(LoadshopDataContext context, IMapper mapper, ILogger<UserCommunicationService> logger, IUserContext userContext)
            : base(context, mapper, logger, userContext)
        {
        }

        protected override async Task<UserCommunicationEntity> GetByKeyQuery(params object[] keys)
        {
            var userCommunicationId = (Guid)keys.First();

            return await LoadUserCommunication(userCommunicationId);
        }

        protected override async Task<IQueryable<UserCommunicationEntity>> InterceptCollectionQuery(IQueryable<UserCommunicationEntity> query)
        {
            return await Task.FromResult(query.OrderBy(userCommunication => userCommunication.EffectiveDate));
        }

        protected override async Task GetCollectionLogic<TListData>(IEnumerable<TListData> data, IEnumerable<UserCommunicationEntity> entities)
        {
            var communicationIds = entities.Select(uc => uc.UserCommunicationId).ToArray();
            var acknowledgements = await Context.UserCommunicationAcknowledgements
                                                    .Where(uca => communicationIds.Contains(uca.UserCommunicationId))
                                                    .GroupBy(uca => uca.UserCommunicationId)
                                                    .Select(ucaGroup =>
                                                    new
                                                    {
                                                        UserCommunicationId = ucaGroup.Key,
                                                        AcknowledgementCount = ucaGroup.Count()
                                                    })
                                                    .ToDictionaryAsync(ucaCount => ucaCount.UserCommunicationId);



            foreach (var userCommunicationData in data)
            {
                if (userCommunicationData is UserCommunicationData)
                {
                    var castUserCommuncationData = userCommunicationData as UserCommunicationData;
                    if (acknowledgements.ContainsKey(castUserCommuncationData.UserCommunicationId.GetValueOrDefault()))
                    {
                        castUserCommuncationData.AcknowledgementCount = acknowledgements[castUserCommuncationData.UserCommunicationId.GetValueOrDefault()].AcknowledgementCount;
                    }
                }
            }
        }

        protected override async Task UpdateLogic(UserCommunicationDetailData data, UserCommunicationEntity entity, CrudResult<UserCommunicationDetailData> result)
        {
            MapLists(data, entity);

            ValidateCommunication(entity, result);

            await Task.CompletedTask;
        }

        protected override async Task<UserCommunicationEntity> UpdateQuery(UserCommunicationDetailData data, params object[] keys)
        {
            var userCommunicationId = (Guid)keys.First();

            return await LoadUserCommunication(userCommunicationId);
        }

        protected override async Task CreateLogic(UserCommunicationDetailData data, UserCommunicationEntity entity, CrudResult<UserCommunicationDetailData> result)
        {
            entity.UserCommunicationShippers = new List<UserCommunicationShipperEntity>();
            entity.UserCommunicationCarriers = new List<UserCommunicationCarrierEntity>();
            entity.UserCommunicationUsers = new List<UserCommunicationUserEntity>();
            entity.UserCommunicationSecurityAccessRoles = new List<UserCommunicationSecurityAccessRoleEntity>();

            MapLists(data, entity);

            ValidateCommunication(entity, result);

            await SetOwner(entity);

        }

        private async Task SetOwner(UserCommunicationEntity entity)
        {
            var currentUser = await LoadCurrentUser();

            entity.OwnerId = currentUser.UserId;
        }

        private void MapLists(UserCommunicationDetailData data, UserCommunicationEntity entity)
        {
            //Shippers
            entity.UserCommunicationShippers.MapList(
              data.UserCommunicationShippers,
              ucsEntity => ucsEntity.UserCommunicationShipperId,
              ucsData => ucsData.UserCommuncationShipperId,
              Mapper);

            //Carriers
            entity.UserCommunicationCarriers.MapList(
              data.UserCommunicationCarriers,
              uccEntity => uccEntity.UserCommunicationCarrierId,
              ucsData => ucsData.UserCommunicationCarrierId,
              Mapper);

            //Users
            entity.UserCommunicationUsers.MapList(
              data.UserCommunicationUsers,
              ucuEntity => ucuEntity.UserCommunicationUserId,
              ucuData => ucuData.UserCommunicationUserId,
              Mapper);

            //Security Access Roles
            entity.UserCommunicationSecurityAccessRoles.MapList(
              data.UserCommunicationSecurityAccessRoles,
              ucuEntity => ucuEntity.UserCommunicationSecurityAccessRoleId,
              ucuData => ucuData.UserCommunicationSecurityAccessRoleId,
              Mapper);
        }

        private async Task<UserCommunicationEntity> LoadUserCommunication(Guid userCommunicationId)
        {
            return await Context.UserCommunications
                 .Include(uc => uc.UserCommunicationShippers)
                 .Include(uc => uc.UserCommunicationCarriers)
                 .Include(uc => uc.UserCommunicationUsers)
                 .Include(uc => uc.UserCommunicationSecurityAccessRoles)
                 .SingleOrDefaultAsync(uc => uc.UserCommunicationId == userCommunicationId);
        }

        private async Task<UserEntity> LoadCurrentUser()
        {
            return await Context.Users.SingleOrDefaultAsync(user => user.IdentUserId == UserContext.UserId);
        }

        public async Task<CrudResult<List<UserCommunicationData>>> GetUserCommunicationsForDisplay(Guid identUserId)
        {
            try
            {
                var result = CrudResult<List<UserCommunicationData>>.Create();

                var user = await Context.Users
                    .Include(x => x.SecurityUserAccessRoles)
                    .Include(x => x.UserCarrierScacs)
                    .Include(x => x.UserShippers)
                    .SingleOrDefaultAsync(x => x.IdentUserId == identUserId);

                var userCarrierIds = user.UserCarrierScacs.Select(ucs => ucs.CarrierId).Distinct().ToArray();
                var userShipperIds = user.UserShippers.Select(us => us.CustomerId).ToArray();
                var userSecurityAccessRoleIds = user.SecurityUserAccessRoles.Select(suar => suar.AccessRoleId).ToArray();

                var communications = await Context
                    .UserCommunications
                    .Where(uc =>
                        !uc.UserCommunicationAcknowledgements.Any(uca => uca.Acknowledged && uca.UserId == user.UserId)
                        && uc.EffectiveDate <= DateTime.Now
                        && (uc.ExpirationDate == null || uc.ExpirationDate > DateTime.Now)
                        && (uc.AllUsers
                        || ((uc.UserCommunicationCarriers
                                .Any(ucc => userCarrierIds.Contains(ucc.CarrierId))
                            || uc.UserCommunicationShippers
                                .Any(ucs => userShipperIds.Contains(ucs.CustomerId))
                            || uc.UserCommunicationUsers
                                .Select(ucu => ucu.UserId)
                                .Contains(user.UserId))
                            && (uc.UserCommunicationSecurityAccessRoles.Count() == 0
                                || uc.UserCommunicationSecurityAccessRoles
                                        .Any(ucsar => userSecurityAccessRoleIds.Contains(ucsar.AccessRoleId)))
                            || (!uc.UserCommunicationCarriers.Any()
                                && !uc.UserCommunicationShippers.Any()
                                && !uc.UserCommunicationUsers.Any()
                                && uc.UserCommunicationSecurityAccessRoles
                                        .Any(ucsar => userSecurityAccessRoleIds.Contains(ucsar.AccessRoleId))))))
                    .OrderBy(uc => uc.EffectiveDate)
                    .ToListAsync();

                var communicationData = Mapper.Map<List<UserCommunicationData>>(communications);

                result.Data = communicationData;

                return result;
            }
            catch (Exception ex)
            {
                return CrudResult<List<UserCommunicationData>>.Create(ex);
            }
        }

        public async Task<CrudResult<List<UserCommunicationData>>> Acknowledge(Guid identUserId, AcknowledgeUserCommunication acknowledgeUserCommunication)
        {
            try
            {
                var user = await Context.Users.SingleOrDefaultAsync(u => u.IdentUserId == UserContext.UserId);

                var result = CrudResult<List<UserCommunicationData>>.Create();

                var newAcknowledgement = new UserCommunicationAcknowledgementEntity()
                {
                    Acknowledged = true,
                    AcknowledgedDate = DateTime.Now,
                    UserId = user.UserId,
                    UserCommunicationId = acknowledgeUserCommunication.UserCommunicationId
                };

                Context.UserCommunicationAcknowledgements.Add(newAcknowledgement);

                await Context.SaveChangesAsync(user.Username);

                var getUserCommunicationsResult = await GetUserCommunicationsForDisplay(identUserId);

                if (result.Successful)
                {
                    result.Data = getUserCommunicationsResult.Data;
                }
                else
                {
                    result.Data = new List<UserCommunicationData>();
                    result.AddWarning("Unable to retive updated User Communications");
                    result.AddExceptions(getUserCommunicationsResult.Exceptions.ToArray());
                }

                return result;
            }
            catch (Exception ex)
            {
                return CrudResult<List<UserCommunicationData>>.Create(ex);
            }
        }

        private bool ValidateCommunication(UserCommunicationEntity userCommunication, CrudResult result)
        {
            var isValid = true;

            if (!(userCommunication.UserCommunicationCarriers.Any()
                || userCommunication.UserCommunicationShippers.Any()
                || userCommunication.UserCommunicationUsers.Any()
                || userCommunication.UserCommunicationSecurityAccessRoles.Any()
                || userCommunication.AllUsers))
            {
                result.ModelState.AddModelError("urn:root", "User Communication does not target any users. Please select at least one target for the communication.");
                isValid = false;
            }

            return isValid;
        }
    }
}
