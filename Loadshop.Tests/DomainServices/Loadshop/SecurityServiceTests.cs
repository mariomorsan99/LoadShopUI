using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loadshop.DomainServices.Common.Cache;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;
using LazyCache;
using LazyCache.Mocks;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class SecurityServiceTests
    {
        public class AsyncApiTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly Mock<IUserContext> _userContext;
            private readonly IMapper _mapper;
            private readonly Mock<ILogger<SecurityService>> _logger;
            private readonly Mock<IDateTimeProvider> _dateTime;

            private ISecurityService _svc;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid ROLE_ID_1 = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid ROLE_ID_2 = new Guid("22222222-2222-2222-2222-222222222222");
            private static readonly string APP_ACTION_ID_1 = "app.action.id.1";
            private static readonly string APP_ACTION_ID_2 = "app.action.id.2";
            private static readonly string ROLE_NAME_1 = "Role One";
            private static readonly string ROLE_NAME_2 = "Role Two";

            private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);

            private List<UserEntity> USERS;
            private List<SecurityAccessRoleEntity> ROLES;
            private List<SecurityUserAccessRoleEntity> USER_ACCESS_ROLES;
            private List<SecurityAccessRoleAppActionEntity> ROLE_APP_ACTIONS;
            private List<SecurityAppActionEntity> APP_ACTIONS;

            public AsyncApiTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(x => x.UserId).Returns(USER_ID);
                _logger = new Mock<ILogger<SecurityService>>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);

                InitSeedData();
                InitDb();
                InitService();
            }

            private void InitDb(MockDbBuilder builder = null)
            {
                if (builder == null)
                {
                    builder = new MockDbBuilder();
                }
                _db = builder
                    .WithUsers(USERS)
                    .WithSecurityAccessRoles(ROLES)
                    .WithSecurityUserAccessRoles(USER_ACCESS_ROLES)
                    .WithSecurityAccessRoleAppActions(ROLE_APP_ACTIONS)
                    .WithSecurityAppActions(APP_ACTIONS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SecurityService(_db.Object, _userContext.Object, _mapper, _logger.Object, _dateTime.Object, GetCacheManager());
            }

            private LoadShopCacheManager GetCacheManager()
            {
                var ucLogger = new Mock<ILogger<UserCacheManager>>();
                var appCache = new MockCachingService();
                return new LoadShopCacheManager(new UserCacheManager(_db.Object, _mapper, ucLogger.Object, _dateTime.Object, appCache));
            }

            [Fact]
            public async Task GetUserRolesAsync_ReturnsRoles()
            {
                var actual = await _svc.GetUserRolesAsync();
                actual.Should().NotBeEmpty();
                actual.First(x => x.AccessRoleId == ROLE_ID_1).Should().NotBeNull();
            }

            [Fact]
            public async Task GetUserRolesAsync_NoUserIdInContext_NoOverride_NoRoles()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                var actual = await _svc.GetUserRolesAsync();
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task GetUserRolesAsync_NoUserIdInContext_WithOverride_ReturnsRoles()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = USER_ID;

                var actual = await _svc.GetUserRolesAsync();
                actual.Should().NotBeEmpty();
                actual.First(x => x.AccessRoleId == ROLE_ID_1).Should().NotBeNull();
            }

            [Fact]
            public async Task GetUserRolesAsync_InvalidUserId_NoRoles()
            {
                _userContext.SetupGet(x => x.UserId).Returns(Guid.Empty);
                var actual = await _svc.GetUserRolesAsync();
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task GetUserRolesAsync_NoUserIdInContext_InvalidOverride_NoRoles()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = Guid.Empty;

                var actual = await _svc.GetUserRolesAsync();
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task UserHasActionAsync_ValidActions_True()
            {
                var actual = await _svc.UserHasActionAsync(APP_ACTION_ID_1, APP_ACTION_ID_2);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task UserHasActionAsync_NoUserIdInContext_NoOverride_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                var actual = await _svc.UserHasActionAsync(APP_ACTION_ID_1, APP_ACTION_ID_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasActionAsync_NoUserIdInContext_WithOverride_True()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = USER_ID;

                var actual = await _svc.UserHasActionAsync(APP_ACTION_ID_1, APP_ACTION_ID_2);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task UserHasActionAsync_InvalidUserId_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(Guid.Empty);
                var actual = await _svc.UserHasActionAsync(APP_ACTION_ID_1, APP_ACTION_ID_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasActionAsync_NoUserIdInContext_InvalidOverride_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = Guid.Empty;

                var actual = await _svc.UserHasActionAsync(APP_ACTION_ID_1, APP_ACTION_ID_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasRoleAsync_ById_ValidActions_True()
            {
                var actual = await _svc.UserHasRoleAsync(ROLE_ID_1, ROLE_ID_2);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task UserHasRoleAsync_ById_NoUserIdInContext_NoOverride_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                var actual = await _svc.UserHasRoleAsync(ROLE_ID_1, ROLE_ID_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasRoleAsync_ById_NoUserIdInContext_WithOverride_True()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = USER_ID;

                var actual = await _svc.UserHasRoleAsync(ROLE_ID_1, ROLE_ID_2);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task UserHasRoleAsync_ById_InvalidUserId_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(Guid.Empty);
                var actual = await _svc.UserHasRoleAsync(ROLE_ID_1, ROLE_ID_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasRoleAsync_ById_NoUserIdInContext_InvalidOverride_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = Guid.Empty;

                var actual = await _svc.UserHasRoleAsync(ROLE_ID_1, ROLE_ID_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasRoleAsync_ByName_ValidActions_True()
            {
                var actual = await _svc.UserHasRoleAsync(ROLE_NAME_1, ROLE_NAME_2);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task UserHasRoleAsync_ByName_NoUserIdInContext_NoOverride_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                var actual = await _svc.UserHasRoleAsync(ROLE_NAME_1, ROLE_NAME_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasRoleAsync_ByName_NoUserIdInContext_WithOverride_True()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = USER_ID;

                var actual = await _svc.UserHasRoleAsync(ROLE_NAME_1, ROLE_NAME_2);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task UserHasRoleAsync_ByName_InvalidUserId_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(Guid.Empty);
                var actual = await _svc.UserHasRoleAsync(ROLE_NAME_1, ROLE_NAME_2);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task UserHasRoleAsync_ByName_NoUserIdInContext_InvalidOverride_False()
            {
                _userContext.SetupGet(x => x.UserId).Returns(value: null);
                _svc.OverrideUserIdentId = Guid.Empty;

                var actual = await _svc.UserHasRoleAsync(ROLE_NAME_1, ROLE_NAME_2);
                actual.Should().BeFalse();
            }

            private void InitSeedData()
            {
                USER_ACCESS_ROLES = new List<SecurityUserAccessRoleEntity>
                {
                    new SecurityUserAccessRoleEntity
                    {
                        AccessRoleId = ROLE_ID_1,
                        UserId = USER_ID,
                    },
                    new SecurityUserAccessRoleEntity
                    {
                        AccessRoleId = ROLE_ID_2,
                        UserId = USER_ID,
                    }
                };
                APP_ACTIONS = new List<SecurityAppActionEntity>
                {
                    new SecurityAppActionEntity
                    {
                        AppActionId = APP_ACTION_ID_1,
                        AppActionDescription = "Action One"
                    },
                    new SecurityAppActionEntity
                    {
                        AppActionId = APP_ACTION_ID_2,
                        AppActionDescription = "Action Two"
                    }
                };
                ROLE_APP_ACTIONS = new List<SecurityAccessRoleAppActionEntity>
                {
                    new SecurityAccessRoleAppActionEntity
                    {
                        AccessRoleId = ROLE_ID_1,
                        AppActionId = APP_ACTION_ID_1,
                        SecurityAppAction = APP_ACTIONS.First(x => x.AppActionId == APP_ACTION_ID_1)
                    },
                    new SecurityAccessRoleAppActionEntity
                    {
                        AccessRoleId = ROLE_ID_2,
                        AppActionId = APP_ACTION_ID_2,
                        SecurityAppAction = APP_ACTIONS.First(x => x.AppActionId == APP_ACTION_ID_2)
                    }
                };
                ROLES = new List<SecurityAccessRoleEntity>
                {
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = ROLE_ID_1,
                        AccessRoleName = ROLE_NAME_1,
                        AccessRoleLevel = 0,
                        SecurityUserAccessRoles = USER_ACCESS_ROLES.Where(x => x.AccessRoleId == ROLE_ID_1).ToList(),
                        SecurityAccessRoleAppActions = ROLE_APP_ACTIONS.Where(x => x.AccessRoleId == ROLE_ID_1).ToList()
                    },
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = ROLE_ID_2,
                        AccessRoleName = ROLE_NAME_2,
                        AccessRoleLevel = 0,
                        SecurityUserAccessRoles = USER_ACCESS_ROLES.Where(x => x.AccessRoleId == ROLE_ID_2).ToList(),
                        SecurityAccessRoleAppActions = ROLE_APP_ACTIONS.Where(x => x.AccessRoleId == ROLE_ID_2).ToList()
                    }
                };
                //Recreate User Access Roles for linking
                USER_ACCESS_ROLES = new List<SecurityUserAccessRoleEntity>
                {
                    new SecurityUserAccessRoleEntity
                    {
                        AccessRoleId = ROLE_ID_1,
                        UserId = USER_ID,
                        SecurityAccessRole = ROLES.FirstOrDefault(x => x.AccessRoleId == ROLE_ID_1)
                    },
                    new SecurityUserAccessRoleEntity
                    {
                        AccessRoleId = ROLE_ID_2,
                        UserId = USER_ID,
                        SecurityAccessRole = ROLES.FirstOrDefault(x => x.AccessRoleId == ROLE_ID_2)
                    }
                };
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        IdentUserId = USER_ID,
                        SecurityUserAccessRoles = USER_ACCESS_ROLES.Where(x => x.UserId == USER_ID).ToList()
                    }
                };
            }
        }

        public class AuthorizedCarrierScacsTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly Mock<IUserContext> _userContext;
            private readonly IMapper _mapper;
            private readonly Mock<ILogger<SecurityService>> _logger;
            private readonly Mock<IDateTimeProvider> _dateTime;

            private ISecurityService _svc;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC_1 = "ABCD";
            private static readonly string SCAC_2 = "EFGH";

            private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);

            private List<UserEntity> USERS;
            private List<UserCarrierScacEntity> USER_CARRIER_SCACS;
            private List<CarrierScacEntity> CARRIER_SCACS;

            public AuthorizedCarrierScacsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(x => x.UserId).Returns(USER_ID);
                _logger = new Mock<ILogger<SecurityService>>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);

                InitSeedData();
                InitDb();
                InitService();
            }

            private void InitDb(MockDbBuilder builder = null)
            {
                if (builder == null)
                {
                    builder = new MockDbBuilder();
                }
                _db = builder
                    .WithUsers(USERS)
                    .WithUserCarrierScacs(USER_CARRIER_SCACS)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SecurityService(_db.Object, _userContext.Object, _mapper, _logger.Object, _dateTime.Object, GetCacheManager());
            }

            private LoadShopCacheManager GetCacheManager()
            {
                var ucLogger = new Mock<ILogger<UserCacheManager>>();
                var appCache = new MockCachingService();
                return new LoadShopCacheManager(new UserCacheManager(_db.Object, _mapper, ucLogger.Object, _dateTime.Object, appCache));
            }

            [Fact]
            public async Task GetAuthorizedScacsForCarrierAsync_ReturnsCarrierScacs()
            {
                var actual = await _svc.GetAuthorizedScacsForCarrierAsync(CARRIER_ID, USER_ID);
                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(1);
                actual.Any(x => x.Scac == SCAC_1).Should().BeTrue();
            }

            [Fact]
            public async Task GetAuthorizedScacsForCarrierAsync_NullUserCarrierScacScac_ReturnsCarrierScacs()
            {
                USER_CARRIER_SCACS.First().Scac = null;
                InitDb();
                InitService();

                var actual = await _svc.GetAuthorizedScacsForCarrierAsync(CARRIER_ID, USER_ID);
                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(2);
                actual.Any(x => x.Scac == SCAC_1).Should().BeTrue();
                actual.Any(x => x.Scac == SCAC_2).Should().BeTrue();
            }

            [Fact]
            public async Task GetAuthorizedScacsForCarrierAsync_NoUserCarrierScacs_ReturnsEmptyList()
            {
                USER_CARRIER_SCACS = new List<UserCarrierScacEntity>();
                InitDb();
                InitService();

                var actual = await _svc.GetAuthorizedScacsForCarrierAsync(CARRIER_ID, USER_ID);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task GetAuthorizedScacsForCarrierAsync_InvalidUserId_ReturnsEmptyList()
            {
                var actual = await _svc.GetAuthorizedScacsForCarrierAsync(CARRIER_ID, Guid.Empty);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task GetAuthorizedScacsForCarrierAsync_NoContextUser_ReturnsEmptyList()
            {
                _userContext.SetupGet(x => x.UserId).Returns(Guid.Empty);
                InitDb();
                InitService();

                var actual = await _svc.GetCurrentUserAuthorizedScacsForCarrierAsync(CARRIER_ID);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task GetAuthorizedScacsForCarrierAsync_CarrierScacNullEffectiveDate_NullEffectiveScacNotReturned()
            {
                CARRIER_SCACS.First(x => x.Scac == SCAC_1).EffectiveDate = null;
                InitDb();
                InitService();

                var actual = await _svc.GetCurrentUserAuthorizedScacsForCarrierAsync(CARRIER_ID);
                actual.Where(x => x.Scac == SCAC_1).Should().BeEmpty();
            }

            [Fact]
            public async Task GetCurrentUserAuthorizedScacsForCarrierAsync_ReturnsCarrierScacs()
            {
                var actual = await _svc.GetCurrentUserAuthorizedScacsForCarrierAsync(CARRIER_ID);
                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(1);
                actual.Any(x => x.Scac == SCAC_1).Should().BeTrue();
            }

            [Fact]
            public async Task GetCurrentUserAuthorizedScacsForCarrierAsync_NullUserCarrierScacScac_ReturnsCarrierScacs()
            {
                USER_CARRIER_SCACS.First().Scac = null;
                InitDb();
                InitService();

                var actual = await _svc.GetCurrentUserAuthorizedScacsForCarrierAsync(CARRIER_ID);
                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(2);
                actual.Any(x => x.Scac == SCAC_1).Should().BeTrue();
                actual.Any(x => x.Scac == SCAC_2).Should().BeTrue();
            }

            [Fact]
            public async Task GetCurrentUserAuthorizedScacsForCarrierAsync_NoUserCarrierScacs_ReturnsEmptyList()
            {
                USER_CARRIER_SCACS = new List<UserCarrierScacEntity>();
                InitDb();
                InitService();

                var actual = await _svc.GetCurrentUserAuthorizedScacsForCarrierAsync(CARRIER_ID);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task GetAuthorizedScasForCarrierByPrimaryScacAsync_ReturnsCarrierScacs()
            {
                var actual = await _svc.GetAuthorizedScasForCarrierByPrimaryScacAsync();
                actual.Should().NotBeEmpty();
                actual.Count.Should().Be(1);
                actual.Any(x => x.Scac == SCAC_1).Should().BeTrue();
            }

            [Fact]
            public async Task GetAuthorizedScasForCarrierByPrimaryScacAsync_NoContextUser_ReturnsEmptyList()
            {
                _userContext.SetupGet(x => x.UserId).Returns(Guid.Empty);
                InitDb();
                InitService();

                var actual = await _svc.GetAuthorizedScasForCarrierByPrimaryScacAsync();
                actual.Should().BeEmpty();
            }


            private void InitSeedData()
            {
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        IdentUserId = USER_ID,
                        PrimaryScac = SCAC_1
                    }
                };
                CARRIER_SCACS = new List<CarrierScacEntity>
                {
                    new CarrierScacEntity
                    {
                        Scac = SCAC_1,
                        CarrierId = CARRIER_ID,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            IsLoadshopActive = true
                        },
                        IsActive = true,
                        IsBookingEligible = true,
                        EffectiveDate = NOW.AddDays(-5),
                        ExpirationDate = NOW.AddDays(5)
                    },
                    new CarrierScacEntity
                    {
                        Scac = SCAC_2,
                        CarrierId = CARRIER_ID,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            IsLoadshopActive = true
                        },
                        IsActive = true,
                        IsBookingEligible = true,
                        EffectiveDate = NOW.AddDays(-5),
                        ExpirationDate = NOW.AddDays(5)
                    }
                };
                USER_CARRIER_SCACS = new List<UserCarrierScacEntity>
                {
                    new UserCarrierScacEntity
                    {
                        UserId = USER_ID,
                        Scac = SCAC_1,
                        User = USERS.First(x => x.UserId == USER_ID),
                        CarrierId = CARRIER_ID,
                        CarrierScac = CARRIER_SCACS.First(x => x.Scac == SCAC_1)
                    }
                };
            }
        }

        public class AllAuthorizedEntitiesTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly Mock<IUserContext> _userContext;
            private readonly IMapper _mapper;
            private readonly Mock<ILogger<SecurityService>> _logger;
            private readonly Mock<IDateTimeProvider> _dateTime;

            private ISecurityService _svc;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string APP_ACTION_ID_1 = "app.action.id.1";
            private static readonly string APP_ACTION_ID_2 = "app.action.id.2";
            private static readonly string SYSTEM_ADMIN_NAME = SecurityRoles.SystemAdmin;
            private static readonly Guid SYSTEM_ADMIN_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string SHIPPER_USER_NAME = SecurityRoles.ShipperUser;
            private static readonly Guid SHIPPER_USER_ID = new Guid("22222222-2222-2222-2222-222222222222");
            private static readonly string CARRIER_USER_NAME = SecurityRoles.CarrierAdmin;
            private static readonly Guid CARRIER_USER_ID = new Guid("33333333-3333-3333-3333-333333333333");

            private static readonly Guid CUSTOMER_ID_1 = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid CUSTOMER_ID_2 = new Guid("22222222-2222-2222-2222-222222222222");
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC_1 = "ABCD";
            private static readonly string SCAC_2 = "EFGH";

            private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);

            private List<UserEntity> USERS;
            private List<SecurityAccessRoleEntity> ROLES;
            private List<SecurityUserAccessRoleEntity> USER_ACCESS_ROLES;
            private List<SecurityAccessRoleAppActionEntity> ROLE_APP_ACTIONS;
            private List<SecurityAppActionEntity> APP_ACTIONS;
            private List<UserCarrierScacEntity> USER_CARRIER_SCACS;
            private List<CarrierScacEntity> CARRIER_SCACS;
            private List<CustomerEntity> CUSTOMERS;
            private List<UserShipperEntity> USER_SHIPPERS;

            public AllAuthorizedEntitiesTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(x => x.UserId).Returns(USER_ID);
                _logger = new Mock<ILogger<SecurityService>>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);

                InitSeedData();
                InitDb();
                InitService();
            }

            private void InitDb(MockDbBuilder builder = null)
            {
                if (builder == null)
                {
                    builder = new MockDbBuilder();
                }
                _db = builder
                    .WithUsers(USERS)
                    .WithSecurityAccessRoles(ROLES)
                    .WithSecurityUserAccessRoles(USER_ACCESS_ROLES)
                    .WithSecurityAccessRoleAppActions(ROLE_APP_ACTIONS)
                    .WithSecurityAppActions(APP_ACTIONS)
                    .WithUserCarrierScacs(USER_CARRIER_SCACS)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .WithCustomers(CUSTOMERS)
                    .WithUserShippers(USER_SHIPPERS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SecurityService(_db.Object, _userContext.Object, _mapper, _logger.Object, _dateTime.Object, GetCacheManager());
            }

            private LoadShopCacheManager GetCacheManager()
            {
                var ucLogger = new Mock<ILogger<UserCacheManager>>();
                var appCache = new MockCachingService();
                return new LoadShopCacheManager(new UserCacheManager(_db.Object, _mapper, ucLogger.Object, _dateTime.Object, appCache));
            }

            [Fact]
            public async Task GetAllMyAuthorizedEntitiesAsync_SystemAdmin_ReturnsAllEntities()
            {
                InitSeedData(SYSTEM_ADMIN_ID);
                InitDb();
                InitService();

                var actual = await _svc.GetAllMyAuthorizedEntitiesAsync();
                actual.Should().NotBeEmpty();
                actual.Where(x => x.Type == UserFocusEntityType.CarrierScac).Should().HaveCount(2);
                actual.Where(x => x.Type == UserFocusEntityType.Shipper).Should().HaveCount(2);
            }

            [Fact]
            public async Task GetAllMyAuthorizedEntitiesAsync_ShipperUser_ReturnsOnlyShipperEntities()
            {
                InitSeedData(SHIPPER_USER_ID);
                InitDb();
                InitService();

                var actual = await _svc.GetAllMyAuthorizedEntitiesAsync();
                actual.Should().NotBeEmpty();
                actual.Where(x => x.Type == UserFocusEntityType.CarrierScac).Should().BeEmpty();
                actual.Where(x => x.Type == UserFocusEntityType.Shipper).Should().HaveCount(2);
            }

            [Fact]
            public async Task GetAllMyAuthorizedEntitiesAsync_CarrierUser_ReturnsOnlyCarrierScacs()
            {
                InitSeedData(CARRIER_USER_ID);
                InitDb();
                InitService();

                var actual = await _svc.GetAllMyAuthorizedEntitiesAsync();
                actual.Should().NotBeEmpty();
                actual.Where(x => x.Type == UserFocusEntityType.CarrierScac).Should().HaveCount(1);
                actual.Where(x => x.Type == UserFocusEntityType.Shipper).Should().BeEmpty();
            }


            private void InitSeedData(Guid? userRoleId = null)
            {
                CARRIER_SCACS = new List<CarrierScacEntity>
                {
                    new CarrierScacEntity
                    {
                        Scac = SCAC_1,
                        CarrierId = CARRIER_ID,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            IsLoadshopActive = true
                        },
                        IsActive = true,
                        IsBookingEligible = true,
                        EffectiveDate = NOW.AddDays(-5),
                        ExpirationDate = NOW.AddDays(5)
                    },
                    new CarrierScacEntity
                    {
                        Scac = SCAC_2,
                        CarrierId = CARRIER_ID,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            IsLoadshopActive = true
                        },
                        IsActive = true,
                        IsBookingEligible = true,
                        EffectiveDate = NOW.AddDays(-5),
                        ExpirationDate = NOW.AddDays(5)
                    }
                };
                APP_ACTIONS = new List<SecurityAppActionEntity>
                {
                    new SecurityAppActionEntity
                    {
                        AppActionId = APP_ACTION_ID_1,
                        AppActionDescription = "Action One"
                    },
                    new SecurityAppActionEntity
                    {
                        AppActionId = APP_ACTION_ID_2,
                        AppActionDescription = "Action Two"
                    }
                };
                ROLE_APP_ACTIONS = new List<SecurityAccessRoleAppActionEntity>
                {
                    new SecurityAccessRoleAppActionEntity
                    {
                        AccessRoleId = SYSTEM_ADMIN_ID,
                        AppActionId = APP_ACTION_ID_1,
                        SecurityAppAction = APP_ACTIONS.First(x => x.AppActionId == APP_ACTION_ID_1)
                    },
                    new SecurityAccessRoleAppActionEntity
                    {
                        AccessRoleId = SHIPPER_USER_ID,
                        AppActionId = APP_ACTION_ID_2,
                        SecurityAppAction = APP_ACTIONS.First(x => x.AppActionId == APP_ACTION_ID_2)
                    },
                    new SecurityAccessRoleAppActionEntity
                    {
                        AccessRoleId = CARRIER_USER_ID,
                        AppActionId = APP_ACTION_ID_2,
                        SecurityAppAction = APP_ACTIONS.First(x => x.AppActionId == APP_ACTION_ID_2)
                    }
                };
                USER_ACCESS_ROLES = new List<SecurityUserAccessRoleEntity>
                {
                    new SecurityUserAccessRoleEntity
                    {
                        AccessRoleId = userRoleId ?? SYSTEM_ADMIN_ID,
                        UserId = USER_ID
                    }
                };
                ROLES = new List<SecurityAccessRoleEntity>
                {
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = SYSTEM_ADMIN_ID,
                        AccessRoleName = SYSTEM_ADMIN_NAME,
                        AccessRoleLevel = 0,
                        SecurityUserAccessRoles = USER_ACCESS_ROLES.Where(x => x.AccessRoleId == SYSTEM_ADMIN_ID).ToList(),
                        SecurityAccessRoleAppActions = ROLE_APP_ACTIONS.Where(x => x.AccessRoleId == SYSTEM_ADMIN_ID).ToList()
                    },
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = SHIPPER_USER_ID,
                        AccessRoleName = SHIPPER_USER_NAME,
                        AccessRoleLevel = 0,
                        SecurityUserAccessRoles = USER_ACCESS_ROLES.Where(x => x.AccessRoleId == SHIPPER_USER_ID).ToList(),
                        SecurityAccessRoleAppActions = ROLE_APP_ACTIONS.Where(x => x.AccessRoleId == SHIPPER_USER_ID).ToList()

                    },
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = CARRIER_USER_ID,
                        AccessRoleName = CARRIER_USER_NAME,
                        AccessRoleLevel = 0,
                        SecurityUserAccessRoles = USER_ACCESS_ROLES.Where(x => x.AccessRoleId == CARRIER_USER_ID).ToList(),
                        SecurityAccessRoleAppActions = ROLE_APP_ACTIONS.Where(x => x.AccessRoleId == CARRIER_USER_ID).ToList()
                    }
                };
                // Recreate UserAccessRoles
                USER_ACCESS_ROLES = new List<SecurityUserAccessRoleEntity>
                {
                    new SecurityUserAccessRoleEntity
                    {
                        AccessRoleId = userRoleId ?? SYSTEM_ADMIN_ID,
                        UserId = USER_ID,
                        SecurityAccessRole = ROLES.FirstOrDefault(x => x.AccessRoleId == (userRoleId ?? SYSTEM_ADMIN_ID))
                    }
                };
                CUSTOMERS = new List<CustomerEntity>
                {
                    new CustomerEntity
                    {
                        CustomerId = CUSTOMER_ID_1,
                        Name = "Customer 1"
                    },
                    new CustomerEntity
                    {
                        CustomerId = CUSTOMER_ID_2,
                        Name = "Customer 2"
                    }
                };
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        IdentUserId = USER_ID,
                        PrimaryScac = SCAC_1,
                        SecurityUserAccessRoles = USER_ACCESS_ROLES.Where(x => x.UserId == USER_ID).ToList()
                    }
                };
                USER_CARRIER_SCACS = new List<UserCarrierScacEntity>
                {
                    new UserCarrierScacEntity
                    {
                        UserId = USER_ID,
                        Scac = SCAC_1,
                        User = USERS.First(x => x.UserId == USER_ID),
                        CarrierId = CARRIER_ID,
                        CarrierScac = CARRIER_SCACS.First(x => x.Scac == SCAC_1)
                    }
                };
                USER_SHIPPERS = new List<UserShipperEntity>
                {
                    new UserShipperEntity
                    {
                        CustomerId = CUSTOMER_ID_1,
                        Customer = CUSTOMERS.First(x => x.CustomerId == CUSTOMER_ID_1),
                        UserId = USER_ID,
                        User = USERS.First(x => x.UserId == USER_ID)
                    },
                    new UserShipperEntity
                    {
                        CustomerId = CUSTOMER_ID_2,
                        Customer = CUSTOMERS.First(x => x.CustomerId == CUSTOMER_ID_2),
                        UserId = USER_ID,
                        User = USERS.First(x => x.UserId == USER_ID)
                    }
                };
            }
        }
    }
}
