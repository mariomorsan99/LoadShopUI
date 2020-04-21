using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.API.Models.DataModels;
using Loadshop.DomainServices.Common.Cache;
using Xunit;
using Loadshop.DomainServices.Loadshop.Services;
using Moq;
using Microsoft.Extensions.Logging;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Proxy.Tops.Loadshop;
using Loadshop.DomainServices.Utilities;
using LazyCache;
using LazyCache.Mocks;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class UserAdminServiceUnitTest : CrudServiceUnitTest<UserData, IUserAdminService>
    {
        protected Mock<LoadshopDataContext> _db;
        private readonly Mock<ILogger<UserAdminService>> _logger;
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;

        private static Guid ADMIN_USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
        private static Guid ADMIN_USER_IDENT_ID = new Guid("22222222-2222-2222-2222-222222222222");
        private static Guid CUSTOMER_ID = new Guid("33333333-3333-3333-3333-333333333333");
        private static Guid SYS_ADMIN_ROLE_ID = new Guid("44444444-4444-4444-4444-444444444444");
        private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);

        private static UserEntity ADMIN_USER => new UserEntity()
        {
            IdentUserId = ADMIN_USER_IDENT_ID,
            UserId = ADMIN_USER_ID,
            Username = "adminuser",
            FirstName = "Admin",
            LastName = "User",
            PrimaryScac = "KBXL",
            PrimaryCustomerId = CUSTOMER_ID,
            IsNotificationsEnabled = true,
            UserShippers = new List<UserShipperEntity>()
            {
                new UserShipperEntity
                {
                    CustomerId = CUSTOMER_ID
                }
            },
            UserCarrierScacs = new List<UserCarrierScacEntity>(),
            SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>()
            {
                new SecurityUserAccessRoleEntity()
                {
                    UserId = ADMIN_USER_ID,
                    AccessRoleId = SYS_ADMIN_ROLE_ID,
                    SecurityAccessRole = new SecurityAccessRoleEntity
                    {
                        AccessRoleId = SYS_ADMIN_ROLE_ID,
                        AccessRoleName = SecurityRoles.SystemAdmin,
                        AccessRoleLevel = 1,
                        SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>(),
                        SecurityAccessRoleAppActions = new List<SecurityAccessRoleAppActionEntity>
                        {
                            new SecurityAccessRoleAppActionEntity()
                            {
                                AccessRoleId = SYS_ADMIN_ROLE_ID,
                                AppActionId = "Some Action",
                                SecurityAppAction = new SecurityAppActionEntity()
                                {
                                    AppActionId = "Some Action Name"
                                }
                            }
                        }
                    }
                }
            }
        };

        private static UserData USER_DATA => new UserData()
        {
            UserId = ADMIN_USER_ID,
            IdentUserId = ADMIN_USER_IDENT_ID,
            Username = "adminuser",
            FirstName = "Admin",
            LastName = "User",
            IsNotificationsEnabled = true,
            PrimaryCustomerId = CUSTOMER_ID,
            PrimaryScac = "KBXL",
            CarrierScacs = new List<string>()
            {
                "KBXL"
            },
            ShipperIds = new List<Guid>()
            {
                CUSTOMER_ID
            },
            SecurityRoleIds = new List<Guid>()
            {
                SYS_ADMIN_ROLE_ID
            },
            UserNotifications = new List<UserNotificationData>(),
            CompanyName = "comp",
            Email = "email",
            SecurityRoles = new List<SecurityAccessRoleData>
            {
                new SecurityAccessRoleData
                {
                    AccessRoleLevel = 1
                }
            }
        };

        private static List<SecurityAccessRoleEntity> ACCESS_ROLES => new List<SecurityAccessRoleEntity>
        {
            new SecurityAccessRoleEntity
            {
                AccessRoleId = SYS_ADMIN_ROLE_ID,
                AccessRoleLevel = 1
            }
        };

        private static List<SecurityAccessRoleParentEntity> ACCESS_ROLE_PARENTS => new List<SecurityAccessRoleParentEntity>
        {
            new SecurityAccessRoleParentEntity
            {
                TopLevelAccessRoleId = SYS_ADMIN_ROLE_ID,
                ChildAccessRole = ACCESS_ROLES[0]
            }
        };

        private static List<CarrierScacEntity> CARRIER_SCACS => new List<CarrierScacEntity>
        {
            new CarrierScacEntity
            {
                CarrierId = "KBXL",
                Scac = "KBXL",
                ScacName = "KBXL",
                IsActive = true,
                IsBookingEligible = true,
                EffectiveDate = DateTime.MinValue,
                ExpirationDate = DateTime.MaxValue,
                Carrier = new CarrierEntity() { IsLoadshopActive = true }
            }
        };

        private static List<UserCarrierScacEntity> USER_CARRIER_SCACS => new List<UserCarrierScacEntity>
        {
            new UserCarrierScacEntity
            {
                UserId = ADMIN_USER_ID,
                CarrierId = "KBXL"
            }
        };

        public UserAdminServiceUnitTest(TestFixture fixture) : base(fixture)
        {
            _db = new MockDbBuilder()
                .WithUser(ADMIN_USER)
                .WithSecurityAccessRoles(ACCESS_ROLES)
                .WithSecurityAccessRoleParents(ACCESS_ROLE_PARENTS)
                .WithCarrierScacs(CARRIER_SCACS)
                .WithUserCarrierScacs(USER_CARRIER_SCACS)
                .Build();

            _logger = new Mock<ILogger<UserAdminService>>();

            var mockTopsLoadshopApiService = new Mock<ITopsLoadshopApiService>();
            mockTopsLoadshopApiService.Setup(_ => _.GetIdentityUser(It.IsAny<string>())).ReturnsAsync(
                new ResponseMessage<IdentityUserData>
                {
                    Data = new IdentityUserData
                        {UserName = USER_DATA.Username, Company = USER_DATA.CompanyName, Email = USER_DATA.Email}
                });

            _securityService.Setup(_ => _.GetAuthorizedCustomersforUserAsync()).ReturnsAsync((new List<CustomerData>()
            {
                new CustomerData { CustomerId = CUSTOMER_ID }
            }).AsReadOnly());
            _securityService.Setup(_ => _.GetAllMyAuthorizedCarriersAsync()).ReturnsAsync((new List<CarrierData>()).AsReadOnly());
            _securityService.Setup(_ => _.GetUserRolesAsync()).ReturnsAsync((new List<SecurityAccessRoleData>
            {
                new SecurityAccessRoleData { AccessRoleLevel = 1 }
            }).AsReadOnly());
            _securityService.Setup(_ => _.UserHasRoleAsync(It.IsAny<string[]>())).ReturnsAsync(true);
            _securityService.Setup(_ => _.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(true);
            _securityService.Setup(_ => _.GetAuthorizedScacsForCarrierAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync((new List<CarrierScacData>
                {
                    new CarrierScacData { CarrierId = "KBXL", Scac = "KBXL" }
                }).AsReadOnly());

            _userContext.SetupGet(_ => _.UserId).Returns(ADMIN_USER_IDENT_ID);

            _dateTimeProvider = new Mock<IDateTimeProvider>();
            _dateTimeProvider.SetupGet(x => x.Now).Returns(NOW);
            _dateTimeProvider.SetupGet(x => x.Today).Returns(NOW);

            CrudService = new UserAdminService(_db.Object, _mapper, _logger.Object, _userContext.Object,
                _securityService.Object, mockTopsLoadshopApiService.Object, GetCacheManager());
        }

        private LoadShopCacheManager GetCacheManager()
        {
            var ucLogger = new Mock<ILogger<UserCacheManager>>();
            var appCache = new MockCachingService();
            return new LoadShopCacheManager(new UserCacheManager(_db.Object, _mapper, ucLogger.Object, _dateTimeProvider.Object, appCache));
        }

        [Fact]
        public override async Task GetCollectionTest()
        {
            await GetCollectionTestHelper<UserData>();
        }

        [Fact]
        public override async Task GetByKeyTest()
        {
            var expectedData = USER_DATA;
            expectedData.SecurityRoles = null;//service nulls the roles after creation

            await GetByKeyTestHelper(expectedData, ADMIN_USER_ID);
        }

        [Fact]
        public override async Task CreateTest()
        {
            var expectedData = USER_DATA;
            expectedData.CarrierScacs.Clear();//because of the new GUID being assigned to the user we can't actually get any scacs back
            expectedData.SecurityRoles = null;//service nulls the roles after creation

            await CreateTestHelper(USER_DATA, expectedData, options => options.Excluding(u => u.UserId));
        }

        [Fact]
        public override async Task UpdateTest()
        {
            var expectedData = USER_DATA;
            expectedData.SecurityRoles = null;//service nulls the roles after creation

            await UpdateTestHelper(USER_DATA, expectedData, ADMIN_USER_ID);
        }

        [Fact]
        public override async Task DeleteTest()
        {
            await DeleteHelper(ADMIN_USER_ID);
        }
    }
}

