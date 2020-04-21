using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LazyCache;
using Loadshop.DomainServices.Common.Cache.Models;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Loadshop.DomainServices.Common.Cache
{
    public class UserCacheManager
    {
        //cache strings
        private static readonly string USER_CACHE = "USER_CACHE";

        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserCacheManager> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IAppCache _cache;

        public UserCacheManager(
            LoadshopDataContext context,
            IMapper mapper,
            ILogger<UserCacheManager> logger,
            IDateTimeProvider dateTimeProvider,
            IAppCache cache)
        {
            _context = context;
            _mapper = mapper;
            _dateTime = dateTimeProvider;
            _cache = cache;
            _logger = logger;
        }

        public async Task<UserCache> GetUserCacheAsync(Guid userId)
        {
            return await _cache.GetOrAddAsync<UserCache>(
                CacheKey(userId),
                () => GenerateUserCacheAsync(userId),
                // Sliding expiration window of 20 minutes, because we explicitly remove the user
                // from the cache when their profile is updated via the UserAdminService
                TimeSpan.FromMinutes(20)
            );
        }

        public void RemoveUserCache(Guid userId)
        {
            _cache.Remove(CacheKey(userId));
        }

        private string CacheKey(Guid userId)
        {
            return $"{USER_CACHE}:{userId}";
        }

        private async Task<UserCache> GenerateUserCacheAsync(Guid userId)
        {
            try
            {
                var suars = await _context.SecurityUserAccessRoles
                    .Include(suar => suar.SecurityAccessRole)
                    .ThenInclude(sar => sar.SecurityAccessRoleAppActions)
                    .ThenInclude(saraa => saraa.SecurityAppAction)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                try
                {
                    var sars = suars.Select(x => x.SecurityAccessRole).ToList();
                    var entitiesTask = Task.Run(() => GetAllMyAuthorizedEntitiesAsync(userId, sars));
                    
                    var userSecurityAppActions = sars
                        .SelectMany(x => x.SecurityAccessRoleAppActions)
                        .Select(x => x.AppActionId)
                        .Distinct()
                        .ToList();

                    var userSecurityAccessRoles = _mapper.Map<List<SecurityAccessRoleData>>(sars);

                    return  new UserCache
                    {
                        UserId = userId,
                        UserSecurityAccessRoles = userSecurityAccessRoles,
                        UserSecurityAppActions = userSecurityAppActions,
                        AuthorizedEntities = await entitiesTask
                    };
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error fetching Security", userId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error building user cache", userId);
            }

            return null;
        }

        private async Task<IReadOnlyCollection<UserFocusEntityData>> GetAllMyAuthorizedEntitiesAsync(Guid userId, List<SecurityAccessRoleEntity> userSecurityAccessRoles)
        {
            var isAdmin = UserHasRole(userSecurityAccessRoles, SecurityRoles.AdminRoles);
            var entityResults = new List<UserFocusEntityData>();

            if (isAdmin)
            {
                //Get Carriers
                var carrierScacEntities = await _context.CarrierScacs
                    .Where(carrierScac =>
                        carrierScac.Carrier.IsLoadshopActive
                        && carrierScac.IsActive
                        && carrierScac.IsBookingEligible
                        && (carrierScac.EffectiveDate == null || _dateTime.Today >= carrierScac.EffectiveDate)
                        && (carrierScac.ExpirationDate == null || _dateTime.Today <= carrierScac.ExpirationDate))
                    .Include(carrierScac => carrierScac.Carrier)
                    .ToListAsync();

                //Get Shippers
                var shipperEntities = await _context.Customers
                    .ToListAsync();

                entityResults.AddRange(_mapper.Map<IEnumerable<UserFocusEntityData>>(carrierScacEntities));
                entityResults.AddRange(_mapper.Map<IEnumerable<UserFocusEntityData>>(shipperEntities));
            }
            else
            {
                // Dedicated Planners
                if (UserHasRole(userSecurityAccessRoles, SecurityRoles.DedicatedRoles))
                {
                    //Get Dedicated CarrierScacs
                    var dedicatedCarrierScacs = _context.CarrierScacs
                        .Include(carrierScac => carrierScac.Carrier)
                        .Where(carrierScac => carrierScac.IsDedicated)
                        .ToList();

                    entityResults
                        .AddRange(_mapper.Map<IEnumerable<UserFocusEntityData>>(dedicatedCarrierScacs));
                }

                // Carrier Roles
                if (UserHasRole(userSecurityAccessRoles, SecurityRoles.CarrierRoles))
                {
                    //Get CarrierScacs
                    var securityCarrierGroups = _context.UserCarrierScacs
                                                            .Include(userCarrierScac => userCarrierScac.CarrierScac)
                                                            .ThenInclude(userCarrierScac => userCarrierScac.Carrier)
                                                            .Where(userCarrierScac => userCarrierScac.UserId == userId)
                                                            .AsEnumerable()
                                                            .GroupBy(userCarrierScac => userCarrierScac.CarrierId)
                                                            .ToList();

                    foreach (var carrierGroup in securityCarrierGroups)
                    {
                        if (carrierGroup.Any(userCarrierScac => userCarrierScac.CarrierScac == null))
                        {
                            var carrierScacEntities = await _context.CarrierScacs
                                                                      .Include(carrierScac => carrierScac.Carrier)
                                                                      .Where(carrierScac => carrierScac.CarrierId == carrierGroup.Key
                                                                          && carrierScac.Carrier.IsLoadshopActive
                                                                          && carrierScac.IsActive
                                                                          && carrierScac.IsBookingEligible
                                                                          && (carrierScac.EffectiveDate == null || _dateTime.Today >= carrierScac.EffectiveDate)
                                                                          && (carrierScac.ExpirationDate == null || _dateTime.Today <= carrierScac.ExpirationDate))
                                                                      .ToListAsync();

                            entityResults
                                    .AddRange(_mapper.Map<IEnumerable<UserFocusEntityData>>(carrierScacEntities));
                        }
                        else
                        {
                            entityResults
                                    .AddRange(_mapper.Map<IEnumerable<UserFocusEntityData>>(carrierGroup.Select(group => group.CarrierScac)));
                        }
                    }
                }

                // Shippers
                if (UserHasRole(userSecurityAccessRoles, SecurityRoles.ShipperRoles))
                {
                    //Get Shippers
                    var shipperEntities = await _context.UserShippers
                                                    .Where(userShipper => userShipper.UserId == userId)
                                                    .Select(userShipper => userShipper.Customer)
                                                    .ToListAsync();

                    entityResults.AddRange(_mapper.Map<IEnumerable<UserFocusEntityData>>(shipperEntities));
                }
            }

            return entityResults.OrderBy(x => x.Name).ToList().AsReadOnly();
        }

        private static bool UserHasRole(IEnumerable<SecurityAccessRoleEntity> userSecurityAccessRoles, params string[] roleNames)
        {
            return userSecurityAccessRoles.Any(role => roleNames.Contains(role.AccessRoleName));
        }
    }
}
