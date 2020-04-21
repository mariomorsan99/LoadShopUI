using AutoMapper;
using Loadshop.DomainServices.Common.Services.Crud;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.API.Models.DataModels;
using Loadshop.DomainServices.Common.Cache;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Proxy.Tops.Loadshop;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class UserAdminService : CrudService<UserEntity, UserData, LoadshopDataContext>, IUserAdminService
    {
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;
        private readonly ITopsLoadshopApiService _topsLoadshopApiService;
        private readonly LoadShopCacheManager _cache;

        public UserAdminService(LoadshopDataContext context,
                                IMapper mapper,
                                ILogger<UserAdminService> logger,
                                IUserContext userContext,
                                ISecurityService securityService,
                                ITopsLoadshopApiService topsLoadshopApiService,
                                LoadShopCacheManager cache)
            : base(context, mapper, logger, userContext)
        {
            _userContext = userContext;
            _securityService = securityService;
            _topsLoadshopApiService = topsLoadshopApiService;
            _cache = cache;
        }

        public async Task<CrudResult<IEnumerable<TListData>>> GetCollection<TListData>(string query, int? take = null, int? skip = null)
        {
            query = query.ToLowerInvariant();

            return await base.GetCollection<TListData>(user =>
                                      ((user.FirstName + " " + user.LastName).ToLower().Contains(query))
                                      || (user.Username.ToLower().Contains(query)),
                                      take,
                                      skip);
        }

        protected override async Task<IQueryable<UserEntity>> InterceptCollectionQuery(IQueryable<UserEntity> query)
        {
            if (await _securityService.IsAdminAsync())
                return query;

            var authorizedEntities = await _securityService.GetAllMyAuthorizedEntitiesAsync();

            var getAuthorizedCustomersTask = _securityService.GetAuthorizedCustomersforUserAsync();
            var getAuthorizedCarriersTask = _securityService.GetAllMyAuthorizedCarriersAsync();
            var getUpdatingUserSecurityRoles = _securityService.GetUserRolesAsync();

            await Task.WhenAll(getAuthorizedCustomersTask, getAuthorizedCarriersTask, getUpdatingUserSecurityRoles);

            var authorizedCustomerIds = getAuthorizedCustomersTask.Result.Select(customer => customer.CustomerId).ToList();
            var authorizedCarrierIds = getAuthorizedCarriersTask.Result.Select(carrier => carrier.CarrierId).ToList();
            var topLevelRole = getUpdatingUserSecurityRoles.Result.OrderBy(role => role.AccessRoleLevel).FirstOrDefault()?.AccessRoleLevel ?? int.MaxValue;


            return query.Where(user => (user.UserShippers.Select(userShipper => userShipper.CustomerId).Any(customerId => authorizedCustomerIds.Contains(customerId))
                                || user.UserCarrierScacs.Select(userCarrierScac => userCarrierScac.CarrierId).Distinct().Any(carrierId => authorizedCarrierIds.Contains(carrierId)))
                                && topLevelRole <= user.SecurityUserAccessRoles.OrderBy(suar => suar.SecurityAccessRole.AccessRoleLevel).Select(suar => suar.SecurityAccessRole.AccessRoleLevel).FirstOrDefault());
        }

        protected override async Task<UserEntity> GetByKeyQuery(params object[] keys)
        {
            var userId = (Guid)keys.First();

            return await LoadUserById(userId);
        }

        protected override async Task<UserEntity> UpdateQuery(UserData data, params object[] keys)
        {
            var userId = keys.First() as Guid?;

            userId.NullArgumentCheck(nameof(userId));

            return await LoadUserById(userId.Value);
        }

        protected override async Task<UserEntity> DeleteQuery(params object[] keys)
        {
            var userId = keys.First() as Guid?;

            userId.NullArgumentCheck(nameof(userId));

            return await LoadUserById(userId.Value);
        }

        protected override async Task GetByKeyLogic(UserData data, UserEntity entity)
        {
            data.UserId.NullArgumentCheck(nameof(data.UserId));

            var loadCarrierScacsTask = GetUsersCarrierScacs(data.UserId.Value);
            var loadIdentityUserTask = GetIdentityUser(data.Username);
            var userNotificationTask = GetUsersNotifications(data.UserId.Value);

            await Task.WhenAll(loadCarrierScacsTask, loadIdentityUserTask, userNotificationTask);

            data.UserNotifications = userNotificationTask.Result;

            data.CarrierScacs = loadCarrierScacsTask.Result.Select(carrierScacs => carrierScacs.Scac).ToList();

            var identityUser = loadIdentityUserTask.Result;

            data.CompanyName = identityUser?.Company;
            data.Email = identityUser?.Email;

            data.SecurityRoles = null; // Only used by User maintenance GetCarrierUsers, not the Crud Service
        }

        protected override async Task UpdateLogic(UserData data, UserEntity userToBeUpdated, CrudResult<UserData> result)
        {
            await UpdateAndCreateLogic(data, userToBeUpdated);
            _cache.UserCache.RemoveUserCache(userToBeUpdated.UserId);
        }

        protected override async Task CreateLogic(UserData data, UserEntity userToBeUpdated, CrudResult<UserData> result)
        {
            userToBeUpdated.UserId = Guid.NewGuid();
            userToBeUpdated.SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>();
            userToBeUpdated.UserCarrierScacs = new List<UserCarrierScacEntity>();
            userToBeUpdated.UserShippers = new List<UserShipperEntity>();

            var updateAndCreateTask = UpdateAndCreateLogic(data, userToBeUpdated);
            var identityUserTask = GetIdentityUser(data.Username);

            await Task.WhenAll(updateAndCreateTask, identityUserTask);

            var identityUser = identityUserTask.Result;

            if (identityUser != null && !string.IsNullOrWhiteSpace(identityUser.Email))
            {
                var newUserNotification = new UserNotificationEntity();
                newUserNotification.MessageTypeId = MessageTypeConstants.Email;
                newUserNotification.NotificationValue = identityUser.Email;
                newUserNotification.User = userToBeUpdated;

                Context.UserNotifications.Add(newUserNotification);
            }

            if (identityUser != null && !string.IsNullOrWhiteSpace(identityUser.PhoneNumber))
            {
                var newUserNotification = new UserNotificationEntity();
                newUserNotification.MessageTypeId = MessageTypeConstants.CellPhone;
                newUserNotification.NotificationValue = identityUser.PhoneNumber;
                newUserNotification.User = userToBeUpdated;

                Context.UserNotifications.Add(newUserNotification);
            }

        }

        protected override async Task DeleteLogic(CrudResult result, UserEntity userToDelete)
        {
            _userContext.UserId.NullArgumentCheck(nameof(_userContext.UserId));

            var updatingUserEntity = await LoadUserByIdentId(_userContext.UserId.Value);
            if (await CanUpdateUser(updatingUserEntity, userToDelete))
            {
                //Delete Relationships
                Context.UserShippers.RemoveRange(userToDelete.UserShippers);
                Context.UserNotifications.RemoveRange(userToDelete.UserNotifications);
                Context.SecurityUserAccessRoles.RemoveRange(userToDelete.SecurityUserAccessRoles);
                Context.UserCarrierScacs.RemoveRange(userToDelete.UserCarrierScacs);
            };
        }

        private async Task UpdateAndCreateLogic(UserData data, UserEntity userToBeUpdated)
        {
            _userContext.UserId.NullArgumentCheck(nameof(_userContext.UserId));

            var updatingUserEntity = await LoadUserByIdentId(_userContext.UserId.Value);

            if (!(await CanUpdateUser(updatingUserEntity, userToBeUpdated)))
            {
                throw new UnauthorizedAccessException($"{updatingUserEntity.Username} cannot update user {userToBeUpdated.Username}");
            }
            //Map Allowed Security Roles
            var allowedRoleIds = await FilterToAuthorizedRoleIds(updatingUserEntity, data.SecurityRoleIds);
            Context.SecurityUserAccessRoles.RemoveRange(userToBeUpdated.SecurityUserAccessRoles);
            userToBeUpdated.SecurityUserAccessRoles.Clear();
            userToBeUpdated.SecurityUserAccessRoles.AddRange(allowedRoleIds.Select(roleId => new SecurityUserAccessRoleEntity() { AccessRoleId = roleId, UserId = userToBeUpdated.UserId }));

            if (await _securityService.UserHasActionAsync(SecurityActions.Loadshop_Ui_System_Carrier_User_Add_Edit))
            {
                //Map Carriers/Carrier Scacs
                var carrierScacs = await BuildUserCarrierScacs(userToBeUpdated.UserId, data.CarrierScacs);
                await GuardCarrier(updatingUserEntity, carrierScacs);
                Context.UserCarrierScacs.RemoveRange(userToBeUpdated.UserCarrierScacs);
                userToBeUpdated.UserCarrierScacs.Clear();
                userToBeUpdated.UserCarrierScacs.AddRange(carrierScacs);
            }

            if (await _securityService.UserHasActionAsync(SecurityActions.Loadshop_Ui_System_Shipper_User_Add_Edit))
            {
                //Map Shippers
                await GuardCustomer(data.ShipperIds);
                Context.UserShippers.RemoveRange(userToBeUpdated.UserShippers);
                userToBeUpdated.UserShippers.Clear();
                userToBeUpdated.UserShippers.AddRange(data.ShipperIds.Select(shipperId => new UserShipperEntity() { UserId = userToBeUpdated.UserId, CustomerId = shipperId }));
            }

            //Reset Primary Customer if not part of user's security anymore
            if (userToBeUpdated.PrimaryCustomerId != null
                && !userToBeUpdated.UserShippers
                        .Select(userShipper => userShipper.CustomerId)
                        .Contains(userToBeUpdated.PrimaryCustomerId.Value))
            {
                userToBeUpdated.PrimaryCustomerId = null;
            }

            //Reset Primary Carrier Scac if not part of user's security anymore
            if (userToBeUpdated.PrimaryScac != null && !data.CarrierScacs.Contains(userToBeUpdated.PrimaryScac))
                userToBeUpdated.PrimaryScac = null;

            //Set Focus Entity if not set yet
            if (userToBeUpdated.PrimaryScac == null && userToBeUpdated.PrimaryCustomerId == null)
            {
                if (data.CarrierScacs.Count() > 0)
                    userToBeUpdated.PrimaryScac = data.CarrierScacs.First();
                else if (userToBeUpdated.UserShippers.Count() > 0)
                    userToBeUpdated.PrimaryCustomerId = userToBeUpdated.UserShippers.First().CustomerId;
            }

            if (data.UserNotifications != null && data.UserNotifications.Any())
            {
                if(userToBeUpdated.UserNotifications == null)
                {
                    userToBeUpdated.UserNotifications = new List<UserNotificationEntity>();
                }
                // remove notification user deleted
                userToBeUpdated.UserNotifications = userToBeUpdated.UserNotifications.Where(x => data.UserNotifications.Any(y => y.UserNotificationId == x.UserNotificationId)).ToList();

                // add / update existing notifications
                foreach (var notification in data.UserNotifications)
                {
                    var dbNotification = userToBeUpdated.UserNotifications.SingleOrDefault(x => x.UserNotificationId == notification.UserNotificationId);
                    if (dbNotification == null)
                    {
                        dbNotification = new UserNotificationEntity()
                        {
                            MessageTypeId = notification.MessageTypeId,
                            UserId = userToBeUpdated.UserId
                        };
                        userToBeUpdated.UserNotifications.Add(dbNotification);
                    }
                    if (notification.MessageTypeId == MessageTypeConstants.CellPhone)
                    {
                        dbNotification.MessageTypeId = MessageTypeConstants.CellPhone;
                    }
                    else if (notification.MessageTypeId == MessageTypeConstants.Phone)
                    {
                        dbNotification.MessageTypeId = MessageTypeConstants.Phone;
                    }
                    else if (notification.MessageTypeId == MessageTypeConstants.Email)
                    {
                        dbNotification.MessageTypeId = MessageTypeConstants.Email;
                    }
                    dbNotification.NotificationEnabled = notification.NotificationEnabled;
                    dbNotification.IsDefault = notification.IsDefault;
                    dbNotification.NotificationValue = notification.NotificationValue ?? string.Empty;
                }
            }
        }

        public async Task<List<CustomerData>> GetAuthorizedCustomers()
        {
            if (await _securityService.IsAdminAsync())
            {
                return Mapper.Map<List<CustomerData>>(await Context.Customers.OrderBy(customer => customer.Name).ToListAsync());
            }

            return (await _securityService.GetAuthorizedCustomersforUserAsync()).ToList();
        }

        public async Task<List<CarrierScacData>> GetAuthorizedCarrierScacs()
        {
            if (await _securityService.IsAdminAsync())
            {
                var activeCarrierScacs = Context.CarrierScacs
                                                    .Where(QueryFilters.GetActiveCarrierScacFilter())
                                                    .OrderBy(carrierScac => carrierScac.CarrierId)
                                                    .ThenBy(carrierScac => carrierScac.Scac);
                return Mapper.Map<List<CarrierScacData>>(activeCarrierScacs);
            }
            else
            {
                var user = await Context.Users.SingleOrDefaultAsync(u => u.IdentUserId == _userContext.UserId);

                user.NullEntityCheck(_userContext.UserId);

                return await GetUsersCarrierScacs(user.UserId);
            }
        }

        public async Task<List<SecurityAccessRoleListData>> GetAvailableSecurityRoles(Guid identUserId)
        {
            //Return a list of the "shipper" roles that the User, identified by the identUserId, may add or remove
            var adminUser = Context.Users
                .Include(x => x.SecurityUserAccessRoles)
                .Where(x => x.IdentUserId == identUserId)
                .FirstOrDefault();

            if (adminUser == null)
            {
                throw new EmptyUserIdException("Invalid Admin UserId");
            }

            var adminUserTopRole = await GetTopLevelAccess(adminUser);

            if (adminUserTopRole == null)
                return new List<SecurityAccessRoleListData>();

            return Mapper.Map<List<SecurityAccessRoleListData>>(await GetSecurityRolesByTopLevelAccessId(adminUserTopRole.AccessRoleId));
        }

        public async Task<IReadOnlyCollection<UserData>> GetAllAdminUsers()
        {
            var roles = new List<string> { "LS ADMIN" };
            var users = await Context.SecurityUserAccessRoles
                .Where(x => roles.Contains(x.SecurityAccessRole.AccessRoleName.ToUpper())).Select(x => x.User)
                .Distinct().ToListAsync();

            return Mapper.Map<List<UserData>>(users).AsReadOnly();
        }

        public async Task<IdentityUserData> GetIdentityUserForCreate(string userName)
        {
            userName.NullArgumentCheck(nameof(userName));

            var alreadyExists = await Context.Users.Where(u => u.Username == userName).AnyAsync();

            if (alreadyExists)
                throw new Exception("User already exists in Loadshop");

            return await GetIdentityUser(userName);
        }

        private async Task<IdentityUserData> GetIdentityUser(string username)
        {
            var result = await _topsLoadshopApiService.GetIdentityUser(username);
            return result.Data;
        }

        private async Task<SecurityAccessRoleEntity> GetTopLevelAccess(UserEntity adminUser)
        {
            var adminUserRoles = adminUser.SecurityUserAccessRoles
                .Select(x => x.AccessRoleId).ToList();

            var adminUserTopRole = await Context.SecurityAccessRoles
                .Where(x => adminUserRoles.Contains(x.AccessRoleId))
                .OrderBy(x => x.AccessRoleLevel)
                .FirstOrDefaultAsync();

            return adminUserTopRole;
        }

        private async Task<List<SecurityAccessRoleEntity>> GetSecurityRolesByTopLevelAccessId(Guid toplevelAccessRoleId)
        {
            var securityAccessRoles = await Context.SecurityAccessRoleParents
                .Where(x => x.TopLevelAccessRoleId == toplevelAccessRoleId)
                .Select(x => x.ChildAccessRole)
                .Distinct()
                .ToListAsync();


            return securityAccessRoles;
        }

        private async Task<bool> CanUpdateUser(UserEntity updatingUser, UserEntity userToUpdate)
        {
            updatingUser.NullArgumentCheck(nameof(updatingUser));
            userToUpdate.NullArgumentCheck(nameof(userToUpdate));

            if (await _securityService.IsAdminAsync())
                return true;

            bool isCarrierOrShipperAdmin = await _securityService.UserHasRoleAsync(SecurityRoles.CarrierAdmin, SecurityRoles.ShipperAdmin);

            //Is Empty User
            if (isCarrierOrShipperAdmin && !userToUpdate.UserShippers.Any() && !userToUpdate.UserCarrierScacs.Any() && !userToUpdate.SecurityUserAccessRoles.Any())
                return true;

            //Validate not higher Level Security Role
            var updatingUserTopLevelRole = updatingUser.SecurityUserAccessRoles
                            .OrderBy(securityUserAccessRoles => securityUserAccessRoles.SecurityAccessRole.AccessRoleLevel)
                            .Select(securityUserAccessRoles => securityUserAccessRoles.SecurityAccessRole.AccessRoleLevel)
                            .FirstOrDefault();

            var userToBeUpdateTopLevelRole = userToUpdate.SecurityUserAccessRoles
                           .OrderBy(securityUserAccessRoles => securityUserAccessRoles.SecurityAccessRole.AccessRoleLevel)
                           .Select(securityUserAccessRoles => securityUserAccessRoles.SecurityAccessRole.AccessRoleLevel)
                           .FirstOrDefault();

            if (updatingUserTopLevelRole > userToBeUpdateTopLevelRole)
                return false;

            updatingUser.NullEntityCheck(_userContext.UserId);

            //Is User in Updating Users Shipper or Carriers
            return isCarrierOrShipperAdmin
                && (updatingUser.UserShippers.Select(userShipper => userShipper.CustomerId).Intersect(userToUpdate.UserShippers.Select(userShipper => userShipper.CustomerId)).Any()
                || updatingUser.UserCarrierScacs.Select(cs => cs.CarrierId).Intersect(userToUpdate.UserCarrierScacs.Select(cs => cs.CarrierId)).Any());
        }

        /// <summary>
        /// Returns the updating user and the user to be updated with Shippers and Carriers Loaded
        /// </summary>
        /// <param name="userIdentId"></param>
        /// <param name="userToUpdateId"></param>
        /// <returns></returns>
        private async Task<UserEntity> LoadUserByIdentId(Guid userIdentId)
        {
            var updatingUser = await Context.Users
                                             .Include(user => user.UserNotifications)
                                              .Include(user => user.UserShippers)
                                              .Include(user => user.UserCarrierScacs)
                                              .Include(user => user.SecurityUserAccessRoles)
                                              .ThenInclude(securityUserAccessRoles => securityUserAccessRoles.SecurityAccessRole)
                                              .SingleOrDefaultAsync(user => user.IdentUserId == userIdentId);

            return updatingUser;

        }

        private async Task<UserEntity> LoadUserById(Guid userId)
        {
            var updatingUser = await Context.Users
                                              .Include(user => user.UserNotifications)
                                              .Include(user => user.UserShippers)
                                              .Include(user => user.UserCarrierScacs)
                                              .Include(user => user.SecurityUserAccessRoles)
                                              .ThenInclude(securityUserAccessRoles => securityUserAccessRoles.SecurityAccessRole)
                                              .SingleOrDefaultAsync(user => user.UserId == userId);

            return updatingUser;

        }

        private async Task<IEnumerable<Guid>> FilterToAuthorizedRoleIds(UserEntity updatingUser, List<Guid> rolesToUpdate)
        {
            var updatingUserTopLevelRole = (await GetTopLevelAccess(updatingUser)).AccessRoleId;


            var allowedRoles = await GetSecurityRolesByTopLevelAccessId(updatingUserTopLevelRole);

            var allowedRoleIds = allowedRoles.Select(x => x.AccessRoleId).ToList();

            var filteredRoleIds = rolesToUpdate
                                            .Where(x => allowedRoleIds.Contains(x))
                                            .ToList();

            return filteredRoleIds;
        }

        private async Task<List<UserCarrierScacEntity>> BuildUserCarrierScacs(Guid updatingUserId, IEnumerable<string> scacs)
        {
            var updatingCarrierScacsEntities = await Context.CarrierScacs.Where(carrierScac => scacs.Contains(carrierScac.Scac)).ToListAsync();
            var carrierIds = updatingCarrierScacsEntities.Select(carrierScacs => carrierScacs.CarrierId).Distinct().ToArray();

            var allCarrierScasForUpdatingCarriersGroupedByCarrier = Context.CarrierScacs
                                                                                .Where(carrierScac => carrierIds.Contains(carrierScac.CarrierId))
                                                                                .Where(QueryFilters.GetActiveCarrierScacFilter())
                                                                                .AsEnumerable()
                                                                                .GroupBy(carrierScac => carrierScac.CarrierId)
                                                                                .ToDictionary(carrierScacGroup => carrierScacGroup.Key);


            List<UserCarrierScacEntity> userCarierScacs = new List<UserCarrierScacEntity>();

            foreach (var updatingCarrierScacGroup in updatingCarrierScacsEntities.GroupBy(carrierScac => carrierScac.CarrierId))
            {
                if (allCarrierScasForUpdatingCarriersGroupedByCarrier.ContainsKey(updatingCarrierScacGroup.Key))
                {
                    var allCarrierScacsGroup = allCarrierScasForUpdatingCarriersGroupedByCarrier[updatingCarrierScacGroup.Key];

                    if (updatingCarrierScacGroup.Count() == allCarrierScacsGroup.Count())
                    {
                        userCarierScacs.Add(new UserCarrierScacEntity() { UserCarrierScacId = Guid.NewGuid(), CarrierId = updatingCarrierScacGroup.Key, UserId = updatingUserId });
                    }
                    else
                    {
                        foreach (var carrierScac in updatingCarrierScacGroup)
                        {
                            userCarierScacs.Add(new UserCarrierScacEntity() { UserCarrierScacId = Guid.NewGuid(), CarrierId = updatingCarrierScacGroup.Key, UserId = updatingUserId, Scac = carrierScac.Scac });
                        }
                    }
                }
                else
                    //We should never experience this exception unless data is updated while we are processes the users Carrier Scacs
                    throw new Exception("Error Building User's Carrier Scacs");
            }

            return userCarierScacs;
        }

        private async Task GuardCarrier(UserEntity updatingUser, List<UserCarrierScacEntity> userCarrierScacs)
        {
            if (!await _securityService.IsAdminAsync())
            {
                foreach (var userCarrierScac in userCarrierScacs)
                {
                    var canUpdateScac = updatingUser.UserCarrierScacs.Any(ucs => ucs.CarrierId == userCarrierScac.CarrierId
                                                            && (ucs.Scac == null || ucs.Scac == userCarrierScac.Scac));

                    if (!canUpdateScac)
                        throw new UnauthorizedAccessException($"Unathorized Carrier Scac set for user");
                }
            }
        }

        private async Task GuardCustomer(List<Guid> customerIds)
        {
            if (!await _securityService.IsAdminAsync())
            {
                foreach (var customerId in customerIds)
                    if (!await _securityService.IsAuthorizedForCustomerAsync(customerId))
                        throw new UnauthorizedAccessException("Unauthorized Shipper set for user");
            }

        }

        private async Task<List<CarrierScacData>> GetUsersCarrierScacs(Guid userId)
        {
            var carrierIds = Context.UserCarrierScacs
                                            .Where(userCarrierScac => userCarrierScac.UserId == userId)
                                            .Select(userCarrierScac => userCarrierScac.CarrierId)
                                            .Distinct();

            List<CarrierScacData> carrierScacs = new List<CarrierScacData>();

            foreach (var carrierId in carrierIds)
            {
                carrierScacs.AddRange(await _securityService.GetAuthorizedScacsForCarrierAsync(carrierId, userId));
            }

            return carrierScacs
                    .OrderBy(carrierScac => carrierScac.CarrierId)
                    .ThenBy(carrierScac => carrierScac.Scac)
                    .ToList();
        }

        private async Task<List<UserNotificationData>> GetUsersNotifications(Guid userId)
        {
            var notificationEntities = await Context.UserNotifications.Where(x => x.UserId == userId).ToListAsync();

            var results = Mapper.Map<List<UserNotificationData>>(notificationEntities);

            return results.ToList();
        }

        public async Task<List<UserData>> GetCarrierUsersAsync(string carrierId, bool excludeAdminUsers = true)
        {
            if (string.IsNullOrWhiteSpace(carrierId))
            {
                throw new ArgumentNullException(nameof(carrierId));
            }

            var userIds = await (
                from u in Context.Users
                join usc in Context.UserCarrierScacs on u.UserId equals usc.UserId
                where usc.CarrierId == carrierId
                select u.UserId)
                .ToListAsync();

            var initQuery = Context.Users
                .Include(x => x.SecurityUserAccessRoles)
                .ThenInclude(x => x.SecurityAccessRole)
                .Include(x => x.UserNotifications)
                .ThenInclude(x => x.MessageType)
                .Include(x => x.UserCarrierScacs)
                .Where(x => userIds.Contains(x.UserId));

            IQueryable<UserEntity> userQuery = null;
            if (excludeAdminUsers)
            {
                userQuery = initQuery.
                    Where(x => !x.SecurityUserAccessRoles.Any(y => SecurityRoles.AdminRoles.Contains(y.SecurityAccessRole.AccessRoleName)));
            }
            else
            {
                userQuery = initQuery;
            }

           var users = await userQuery
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToListAsync();

            var userData = Mapper.Map<List<UserData>>(users);
            return userData;
        }

        public async Task<IReadOnlyCollection<CarrierData>> GetAllMyAuthorizedCarriersAsync()
        {
            List<CarrierEntity> carrierEntities;
            if (await _securityService.IsAdminAsync())
            {
                carrierEntities = await Context.Carriers
                    .Select(x => new CarrierEntity { CarrierId = x.CarrierId, CarrierName = x.CarrierName })
                    .ToListAsync();
            }
            else
            {
                carrierEntities = await (
                    from usc in Context.UserCarrierScacs
                    join u in Context.Users on usc.UserId equals u.UserId
                    join c in Context.Carriers on usc.CarrierId equals c.CarrierId
                    where u.IdentUserId == _userContext.UserId
                    select new CarrierEntity { CarrierId = c.CarrierId, CarrierName = c.CarrierName })
                    .Distinct()
                    .ToListAsync();
            }

            return Mapper.Map<List<CarrierData>>(carrierEntities).AsReadOnly();

        }
    }
}
