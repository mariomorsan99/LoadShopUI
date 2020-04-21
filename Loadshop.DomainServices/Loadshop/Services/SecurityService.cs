using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Common.Cache;
using Loadshop.DomainServices.Common.Cache.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly LoadshopDataContext _context;
        private readonly IUserContext _userContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly LoadShopCacheManager _cache;

        private UserEntity _user;
        private List<SecurityAccessRoleData> _userSecurityAccessRoles;
        private List<string> _userSecurityAppActions;
        private IReadOnlyCollection<UserFocusEntityData> _authorizedEntities;
        private Task _initTask;

        public Guid? OverrideUserIdentId { get; set; }

        public SecurityService(LoadshopDataContext context, IUserContext userContext, IMapper mapper, ILogger<SecurityService> logger, IDateTimeProvider dateTime, LoadShopCacheManager cache)
        {
            _context = context;
            _userContext = userContext;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
            _cache = cache;
        }

        #region Init
        /// <summary>
        /// Init the SecurityService
        /// </summary>
        /// <returns></returns>
        private async Task Init()
        {
            UserCache userCache = null;
            if (_userContext.UserId.HasValue || OverrideUserIdentId.HasValue)
            {
                _user = await _context.Users.SingleOrDefaultAsync(user =>
                    user.IdentUserId == (OverrideUserIdentId ?? _userContext.UserId));
                if(_user != null)
                {
                    userCache = await _cache.UserCache.GetUserCacheAsync(_user.UserId);
                }
            }

            if (userCache != null)
            {
                _userSecurityAccessRoles = userCache.UserSecurityAccessRoles;
                _userSecurityAppActions = userCache.UserSecurityAppActions;
                _authorizedEntities = userCache.AuthorizedEntities;
            }
            else
            {
                _userSecurityAccessRoles = new List<SecurityAccessRoleData>();
                _userSecurityAppActions = new List<string>();
                _authorizedEntities = new List<UserFocusEntityData>();
            }
        }

        /// <summary>
        /// Used for Lazy Init so Security Data is only loaded if it is used
        /// </summary>
        private async Task LazyInit()
        {
            if (_initTask != null)
                await _initTask;
            else
            {
                _initTask = Init();
                await _initTask;
            }
        }
        #endregion

        #region AsyncApi

        /// <summary>
        /// Return a bool representing whether or not the current user has the supplied role(s)
        /// </summary>
        /// <param name="roleNames">roleName to validate against</param>
        /// <returns></returns>
        public async Task<bool> UserHasRoleAsync(params string[] roleNames)
        {
            await LazyInit();
            return _userSecurityAccessRoles.Any(role => roleNames.Contains(role.AccessRoleName));
        }
        /// <summary>
        /// Returns a bool representing whether or not the current user has the supplied role(s) by Id
        /// </summary>
        /// <param name="roleIds">role Id to validate against</param>
        /// <returns></returns>
        public async Task<bool> UserHasRoleAsync(params Guid[] roleIds)
        {
            await LazyInit();
            return _userSecurityAccessRoles.Any(role => roleIds.Contains(role.AccessRoleId));
        }
        /// <summary>
        /// Returns a bool representing whether or not the current user has the supplied action(s)
        /// </summary>
        /// <param name="actionNames"></param>
        /// <returns></returns>
        public async Task<bool> UserHasActionAsync(params string[] actionNames)
        {
            await LazyInit();
            return _userSecurityAppActions.Any(actionNames.Contains);
        }
        /// <summary>
        /// Get all User Roles
        /// </summary>
        /// <returns>Returns a List of SecurityAccessRoleData</returns>
        public async Task<IReadOnlyCollection<SecurityAccessRoleData>> GetUserRolesAsync()
        {
            await LazyInit();
            //Don't return reference to List in security service
            return _userSecurityAccessRoles.ToList().AsReadOnly();
        }
        #endregion

        #region SyncApi

        /// <summary>
        /// Return a bool representing whether or not the current user has the supplied role(s)
        /// </summary>
        /// <param name="roleNames">roleName to validate against</param>
        /// <returns></returns>
        public bool UserHasRole(params string[] roleNames)
        {
            var task = UserHasRoleAsync(roleNames);
            task.Wait();

            return task.Result;
        }
        /// <summary>
        /// Returns a bool representing whether or not the current user has the supplied role(s) by Id
        /// </summary>
        /// <param name="roleIds">role Id to validate against</param>
        /// <returns></returns>
        public bool UserHasRole(params Guid[] roleIds)
        {
            var task = UserHasRoleAsync(roleIds);
            task.Wait();

            return task.Result;
        }
        /// <summary>
        /// Returns a bool representing whether or not the current user has the supplied action(s)
        /// </summary>
        /// <param name="actionNames"></param>
        /// <returns></returns>
        public bool UserHasAction(params string[] actionNames)
        {
            var task = UserHasActionAsync(actionNames);
            task.Wait();

            return task.Result;
        }
        /// <summary>
        /// Get all User Roles
        /// </summary>
        /// <returns>Returns a List of SecurityAccessRoleData</returns>
        public IReadOnlyCollection<SecurityAccessRoleData> GetUserRoles()
        {
            var task = GetUserRolesAsync();
            task.Wait();

            return task.Result;
        }
        #endregion

        #region Authorized Carrier Scacs

        /// <summary>
        /// Async method to get Authorized Scacs for the current user and the given carrierId
        /// </summary>
        /// <param name="carrierId">Carrier Id you would like to know the users authorized scacs for</param>
        /// <returns>Collection of Authorized Scacs</returns>
        /// 
        public async Task<IReadOnlyCollection<CarrierScacData>> GetCurrentUserAuthorizedScacsForCarrierAsync(string carrierId)
        {
            await LazyInit();

            return await GetAuthorizedScacsForCarrierAsync(carrierId, _user?.UserId ?? Guid.Empty);
        }

        public async Task<IReadOnlyCollection<CarrierScacData>> GetAuthorizedScacsForCarrierAsync(string carrierId, Guid userId)
        {
            if (carrierId != null)
            {
                var userCarrierScacs = await _context.UserCarrierScacs
                     .Where(userCarrierScac =>
                        userCarrierScac.CarrierId == carrierId
                        && userCarrierScac.User.UserId == userId)
                     .ToListAsync();

                List<CarrierScacData> carrierScacs;

                if (userCarrierScacs.Any(userCarrierScac => userCarrierScac.Scac == null))
                {
                    var carrierScacEntities = await _context.CarrierScacs
                                                                .Where(carrierScac => carrierScac.CarrierId == carrierId)
                                                                .Where(QueryFilters.GetActiveCarrierScacFilter(_dateTime.Today))
                                                                .ToListAsync();

                    carrierScacs = _mapper.Map<List<CarrierScacData>>(carrierScacEntities);
                }
                else
                {
                    var authorizedScacArray = userCarrierScacs
                                                .Select(userCarrierScac => userCarrierScac.Scac).ToArray();

                    var carrierScacEntities = await _context.CarrierScacs
                                                                .Where(carrierScac => carrierScac.CarrierId == carrierId
                                                                    && authorizedScacArray.Contains(carrierScac.Scac))
                                                                .Where(QueryFilters.GetActiveCarrierScacFilter(_dateTime.Today))
                                                                .ToListAsync();

                    carrierScacs = _mapper.Map<List<CarrierScacData>>(carrierScacEntities);
                }

                return carrierScacs.AsReadOnly();
            }

            return new List<CarrierScacData>().AsReadOnly();

        }

        /// <summary>
        /// Sync wrapper method to get Authorized Scacs for the current user and the given carrierId
        /// </summary>
        /// <param name="carrierId">Carrier Id you would like to know the users authorized scacs for</param>
        /// <returns>Collection of Authorized Scacs</returns>
        public IReadOnlyCollection<CarrierScacData> GetAuthorizedScacsForCarrier(string carrierId)
        {
            var getAuthorizedScasForCarrierTask = GetCurrentUserAuthorizedScacsForCarrierAsync(carrierId);
            getAuthorizedScasForCarrierTask.Wait();

            return getAuthorizedScasForCarrierTask.Result;
        }

        /// <summary>
        /// Async method to get Authorized Scacs for the current user and the given the primary scac
        /// </summary>
        /// <returns>Collection of Authorized Scacs</returns>
        public async Task<IReadOnlyCollection<CarrierScacData>> GetAuthorizedScasForCarrierByPrimaryScacAsync()
        {
            await LazyInit();
            var primaryScac = _user?.PrimaryScac ?? string.Empty;
            var carrierScacResult = _context.CarrierScacs.SingleOrDefault(carrierScac => carrierScac.Scac == primaryScac);

            if (carrierScacResult != null)
                return await GetCurrentUserAuthorizedScacsForCarrierAsync(carrierScacResult.CarrierId);

            return new List<CarrierScacData>().AsReadOnly();
        }

        /// <summary>
        /// Sync wrapper method to get Authorized Scacs for the current user and the given the primary scac
        /// </summary>
        /// <returns>Collection of Authorized Scacs</returns>
        public IReadOnlyCollection<CarrierScacData> GetAuthorizedScasForCarrierByPrimaryScac()
        {
            var getAuthorizedScasForCarrierByScacTask = GetAuthorizedScasForCarrierByPrimaryScacAsync();
            getAuthorizedScasForCarrierByScacTask.Wait();

            return getAuthorizedScasForCarrierByScacTask.Result;
        }

        #endregion

        #region All Authorized Entities

        /// <summary>
        /// Async method to get all authorized entities for a user
        /// </summary>
        /// <returns>Collection of Authorized Scacs</returns>
        public async Task<IReadOnlyCollection<UserFocusEntityData>> GetAllMyAuthorizedEntitiesAsync()
        {
            await LazyInit();
            return _authorizedEntities;
        }

        /// <summary>
        /// Sync wrapper method to get all authorized entities for a user
        /// </summary>
        /// <returns>Collection of Authorized Scacs</returns>
        public IReadOnlyCollection<UserFocusEntityData> GetAllMyAuthorizedEntities()
        {
            var getAuthorizedScasForCarrierTask = GetAllMyAuthorizedEntitiesAsync();
            getAuthorizedScasForCarrierTask.Wait();

            return getAuthorizedScasForCarrierTask.Result;
        }

        #endregion

        #region All Authorized Carriers
        /// <summary>
        /// Async method to get all authorized entities for a user
        /// </summary>
        /// <returns>Collection of Authorized Scacs</returns>
        public async Task<IReadOnlyCollection<CarrierData>> GetAllMyAuthorizedCarriersAsync()
        {
            await LazyInit();
            List<CarrierEntity> carrierEntities;
            if (await this.IsAdminAsync())
                carrierEntities = await _context.Carriers.ToListAsync();
            else
                carrierEntities = await _context.UserCarrierScacs
                                                        .Where(userCarrierScac => userCarrierScac.UserId == _user.UserId)
                                                        .Select(userCarrierScac => userCarrierScac.Carrier)
                                                        .Distinct()
                                                        .ToListAsync();

            return _mapper.Map<List<CarrierData>>(carrierEntities).AsReadOnly();

        }

        /// <summary>
        /// Sync wrapper method to get all authorized entities for a user
        /// </summary>
        /// <returns>Collection of Authorized Scacs</returns>
        public IReadOnlyCollection<CarrierData> GetAllMyAuthorizedCarriers()
        {
            var getAuthorizedScasForCarrierTask = GetAllMyAuthorizedCarriersAsync();
            getAuthorizedScasForCarrierTask.Wait();

            return getAuthorizedScasForCarrierTask.Result;
        }
        #endregion

        #region Authorized Shippers for User

        /// <summary>
        /// Async method to get Authorized Shippers for the current user
        /// </summary>
        /// <returns>Collection of Authorized Shippers</returns>
        public async Task<IReadOnlyCollection<CustomerData>> GetAuthorizedCustomersforUserAsync()
        {
            var authorizedShippers = await _context.UserShippers
                 .Where(userShipper => userShipper.User.IdentUserId == _userContext.UserId)
                 .Select(userShipper => userShipper.Customer)
                 .OrderBy(shipper => shipper.Name)
                 .ToListAsync();

            return _mapper.Map<List<CustomerData>>(authorizedShippers).AsReadOnly();
        }

        /// <summary>
        /// Sync wapper method to get Authorized Shippers for the current user
        /// </summary>
        /// <returns>Collection of Authorized Shippers</returns>
        public IReadOnlyCollection<CustomerData> GetAuthorizedCustomersforUser()
        {
            var getAuthorizedShippersForUser = GetAuthorizedCustomersforUserAsync();
            getAuthorizedShippersForUser.Wait();

            return getAuthorizedShippersForUser.Result;
        }

        #endregion

        #region Authorized Customers for a Carrier Scac
        /// <summary>
        /// Get Authorized Customers by CarrierScacContract
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<CustomerData>> GetAuthorizedCustomersByScacAync()
        {
            await LazyInit();
            var customerResults = await _context.CustomerCarrierScacContracts
                                                     .Where(customerCarrierScacContracts =>
                                                                customerCarrierScacContracts.Scac == _user.PrimaryScac
                                                                && customerCarrierScacContracts.CarrierScac.Carrier.IsLoadshopActive
                                                                && customerCarrierScacContracts.CarrierScac.IsActive
                                                                && customerCarrierScacContracts.CarrierScac.IsActive
                                                                && customerCarrierScacContracts.CarrierScac.IsBookingEligible
                                                                && (customerCarrierScacContracts.CarrierScac.EffectiveDate == null || DateTime.Today >= customerCarrierScacContracts.CarrierScac.EffectiveDate)
                                                                && (customerCarrierScacContracts.CarrierScac.ExpirationDate == null || DateTime.Today <= customerCarrierScacContracts.CarrierScac.ExpirationDate))
                                                     .Select(customerCarrierScacContracts => customerCarrierScacContracts.Customer)
                                                     .ToListAsync();

            return _mapper.Map<List<CustomerData>>(customerResults).AsReadOnly();

        }

        public IReadOnlyCollection<CustomerData> GetAuthorizedCustomersByScac()
        {
            var getAuthorizedCustomerbyScacTask = GetAuthorizedCustomersByScacAync();

            getAuthorizedCustomerbyScacTask.Wait();

            return getAuthorizedCustomerbyScacTask.Result;

        }

        #endregion

        #region Customers Contracted Scacs

        /// <summary>
        /// Get Customer active Contracted Scacs based off the users PrimaryShipperId
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<CarrierScacData>> GetCustomerContractedScacsByPrimaryCustomerAsync()
        {
            await LazyInit();
            var kbxlCarrierScacs = _context.Carriers
                .Where(x => x.KBXLContracted)
                .SelectMany(x => x.CarrierScacs)
                .Where(QueryFilters.GetActiveCarrierScacFilter())
                .ToListAsync();

            var carrierScacs = _context.CustomerCarrierScacContracts
                .Where(customerCarrierScacContract => customerCarrierScacContract.CustomerId == _user.PrimaryCustomerId)
                .Select(customerCarrierScacContracts => customerCarrierScacContracts.CarrierScac)
                .Where(QueryFilters.GetActiveCarrierScacFilter())
                .ToListAsync();

            Task.WaitAll(kbxlCarrierScacs, carrierScacs);
            var allCarrierScacs = kbxlCarrierScacs.Result.Union(carrierScacs.Result).Distinct();

            return _mapper.Map<List<CarrierScacData>>(allCarrierScacs).AsReadOnly();
        }

        /// <summary>
        /// Sync wrapper to get active Contracted Scacs based of the user PrimaryShipperId
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<CarrierScacData> GetCustomerContractedScacsByPrimaryCustomer()
        {
            var getCustomerContractedScacsTask = GetCustomerContractedScacsByPrimaryCustomerAsync();

            getCustomerContractedScacsTask.Wait();

            return getCustomerContractedScacsTask.Result;
        }

        #endregion

        #region Contracted Carriers

        /// <summary>
        /// Get active Contracted Carriers based on the users PrimaryShipper and active Contracted Scacs
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<CarrierData>> GetContractedCarriersByPrimaryCustomerIdAsync()
        {
            await LazyInit();
            var kbxlCarriers = _context.Carriers
                .Include(carrier => carrier.CarrierScacs)
                .Where(x => x.KBXLContracted)
                .ToListAsync();
            var contractedCarriers = _context.CustomerCarrierScacContracts
                .Where(customerCarrierScacContract => customerCarrierScacContract.CustomerId == _user.PrimaryCustomerId)
                .Select(customerCarrierScacContract => customerCarrierScacContract.CarrierScac)
                .Where(QueryFilters.GetActiveCarrierScacFilter())
                .Select(carrierScac => carrierScac.Carrier)
                .Include(carrier => carrier.CarrierScacs)
                .ToListAsync();

            Task.WaitAll(kbxlCarriers, contractedCarriers);

            var carriers = _mapper.Map<List<CarrierData>>(kbxlCarriers.Result.Union(contractedCarriers.Result).Distinct());

            var carriersWithAtLeastOneActiveScac = carriers.Where(carrier => carrier.CarrierScacs.Any()).ToList();

            return carriersWithAtLeastOneActiveScac.AsReadOnly();
        }

        /// <summary>
        /// Sync wrapper to get active Contracted Carriers based on the users PrimaryShipper and active Contracted Scacs
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<CarrierData> GetContractedCarriersByPrimaryCustomerId()
        {
            var getCustomerContractedCarriersTask = GetContractedCarriersByPrimaryCustomerIdAsync();

            getCustomerContractedCarriersTask.Wait();

            return getCustomerContractedCarriersTask.Result;
        }

        #endregion

        public void ResetInit()
        {
            OverrideUserIdentId = null;
            _user = null;
            _initTask = null;
        }

        //This is not used currently as the user is not meant to have a tree of roles. The SecurityAccessRoleParent table is meant to populate a drop down for the Carrie and Shipper admin screens.
        //Recursive Query for Child Roles is not needed on a per user bases
        //private IEnumerable<SecurityAccessRoleEntity> GetChildRoles(SecurityAccessRoleEntity parentRole)
        //{
        //    var childRoles = _context.SecurityAccessRoles
        //                        .Where(childSar => childSar.ParentAccessRoles.Any(parentSarRel => parentSarRel.TopLevelAccessRoleId == parentRole.AccessRoleId
        //                                                                                                && parentSarRel.ChildAccessRoleId != parentRole.AccessRoleId))
        //                        .Include(sar => sar.SecurityAccessRoleAppActions)
        //                        .ThenInclude(saraa => saraa.SecurityAppAction)
        //                        .ToList();

        //    foreach (var childRole in childRoles.ToArray())
        //    {
        //        childRoles.AddRange(GetChildRoles(childRole));
        //    }

        //    return childRoles;
        //}
    }
}
