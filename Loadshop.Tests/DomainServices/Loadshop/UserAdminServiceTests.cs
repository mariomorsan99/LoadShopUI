using System;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services;
using Xunit;
using FluentAssertions;
using Moq;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Loadshop.DomainServices.Common.Cache;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Microsoft.Extensions.Logging;
using Loadshop.DomainServices.Proxy.Tops.Loadshop;
using Loadshop.DomainServices.Utilities;
using LazyCache;
using LazyCache.Mocks;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class UserAdminServiceTests
    {
        public class GetCarrierUsersAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<ILogger<UserAdminService>> _logger;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ITopsLoadshopApiService> _topsLoadshopApiService;
            private readonly Mock<IDateTimeProvider> _dateTimeProvider;

            private IUserAdminService _svc;

            private List<UserEntity> USERS;
            private List<UserCarrierScacEntity> USER_CARRIER_SCACS;
            private List<SecurityAccessRoleEntity> ROLES;
            private List<SecurityUserAccessRoleEntity> USER_ROLES;

            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly Guid SYS_ADMIN_USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid LS_ADMIN_USER_ID = new Guid("22222222-2222-2222-2222-222222222222");
            private static readonly Guid CARRIER_ADMIN_USER_ID = new Guid("33333333-3333-3333-3333-333333333333");
            private static readonly Guid SYS_ADMIN_ROLE_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid LS_ADMIN_ROLE_ID = new Guid("22222222-2222-2222-2222-222222222222");
            private static readonly Guid CARRIER_ADMIN_ROLE_ID = new Guid("33333333-3333-3333-3333-333333333333");
            private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);

            public GetCarrierUsersAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _logger = new Mock<ILogger<UserAdminService>>();
                _userContext = new Mock<IUserContext>();
                _securityService = new Mock<ISecurityService>();
                _topsLoadshopApiService = new Mock<ITopsLoadshopApiService>();
                _dateTimeProvider = new Mock<IDateTimeProvider>();
                _dateTimeProvider.SetupGet(x => x.Now).Returns(NOW);
                _dateTimeProvider.SetupGet(x => x.Today).Returns(NOW);

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
                    .WithSecurityUserAccessRoles(USER_ROLES)
                    .WithSecurityAccessRoles(ROLES)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserAdminService(
                    _db.Object,
                    _mapper,
                    _logger.Object,
                    _userContext.Object,
                    _securityService.Object,
                    _topsLoadshopApiService.Object,
                    GetCacheManager());
            }
            private LoadShopCacheManager GetCacheManager()
            {
                var ucLogger = new Mock<ILogger<UserCacheManager>>();
                var appCache = new MockCachingService();
                return new LoadShopCacheManager(new UserCacheManager(_db.Object, _mapper, ucLogger.Object, _dateTimeProvider.Object, appCache));
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void NullOrWhitespaceCarrierId_ThrowsException(string carrierId)
            {
                Func<Task> action = () => _svc.GetCarrierUsersAsync(carrierId, false);
                action.Should().Throw<ArgumentNullException>(nameof(carrierId));
            }

            [Fact]
            public async Task InvalidCarrierId_ReturnsEmptyList()
            {
                var actual = await _svc.GetCarrierUsersAsync("Invalid", false);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task ReturnsUsersAssociatedWithCarrier()
            {
                var expected = new List<Guid> { SYS_ADMIN_USER_ID, LS_ADMIN_USER_ID, CARRIER_ADMIN_USER_ID };
                var actual = await _svc.GetCarrierUsersAsync(CARRIER_ID, false);
                actual.Should().NotBeEmpty();
                actual.Where(x => expected.Contains(x.UserId.Value)).Should().HaveCount(expected.Count());
            }

            [Fact]
            public async Task ReturnsUsersSortedByLastNameThenFirstName()
            {
                var expectedOrder = USERS
                    .Where(x => x.FirstName != "Not") // Exclude the Not Returned user from expected results
                    .OrderBy(x => x.LastName)
                    .ThenBy(x => x.FirstName)
                    .Select(x => x.FirstName)
                    .ToList();
                var actual = await _svc.GetCarrierUsersAsync(CARRIER_ID, false);
                actual.Should().NotBeEmpty();
                actual.Select(x => x.FirstName).Should().Equal(expectedOrder);
            }

            [Fact]
            public async Task DoesNotReturnAdminUsersWhenToldToExcludeThem()
            {
                var expected = new List<Guid> { CARRIER_ADMIN_USER_ID };
                var actual = await _svc.GetCarrierUsersAsync(CARRIER_ID, true);
                actual.Should().NotBeEmpty();
                actual.Should().HaveCount(expected.Count());
            }

            [Fact]
            public async Task DoesNotReturnsUsersNotAssociatedWithCarrier()
            {
                var actual = await _svc.GetCarrierUsersAsync(CARRIER_ID, false);
                actual.Where(x => x.FirstName == "Not").Should().BeEmpty();
            }

            private void InitSeedData()
            {
                ROLES = new List<SecurityAccessRoleEntity>
                {
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = SYS_ADMIN_ROLE_ID,
                        AccessRoleName = SecurityRoles.SystemAdmin
                    },
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = LS_ADMIN_ROLE_ID,
                        AccessRoleName = SecurityRoles.LSAdmin
                    },
                    new SecurityAccessRoleEntity
                    {
                        AccessRoleId = CARRIER_ADMIN_ROLE_ID,
                        AccessRoleName = SecurityRoles.CarrierAdmin
                    }
                };
                USER_ROLES = new List<SecurityUserAccessRoleEntity>
                {
                    new SecurityUserAccessRoleEntity
                    {
                        UserId = SYS_ADMIN_USER_ID,
                        AccessRoleId = SYS_ADMIN_ROLE_ID,
                        SecurityAccessRole = ROLES.First(x => x.AccessRoleId == SYS_ADMIN_ROLE_ID)
                    },
                    new SecurityUserAccessRoleEntity
                    {
                        UserId = SYS_ADMIN_USER_ID,
                        AccessRoleId = CARRIER_ADMIN_ROLE_ID,
                        SecurityAccessRole = ROLES.First(x => x.AccessRoleId == CARRIER_ADMIN_ROLE_ID)
                    },
                    new SecurityUserAccessRoleEntity
                    {
                        UserId = LS_ADMIN_USER_ID,
                        AccessRoleId = LS_ADMIN_ROLE_ID,
                        SecurityAccessRole = ROLES.First(x => x.AccessRoleId == LS_ADMIN_ROLE_ID)
                    },
                    new SecurityUserAccessRoleEntity
                    {
                        UserId = CARRIER_ADMIN_USER_ID,
                        AccessRoleId = CARRIER_ADMIN_ROLE_ID,
                        SecurityAccessRole = ROLES.First(x => x.AccessRoleId == CARRIER_ADMIN_ROLE_ID)
                    }
                };
                USER_CARRIER_SCACS = new List<UserCarrierScacEntity>
                {
                    new UserCarrierScacEntity
                    {
                        UserId = SYS_ADMIN_ROLE_ID,
                        CarrierId = CARRIER_ID
                    },
                    new UserCarrierScacEntity
                    {
                        UserId = LS_ADMIN_USER_ID,
                        CarrierId = CARRIER_ID
                    },
                    new UserCarrierScacEntity
                    {
                        UserId = CARRIER_ADMIN_USER_ID,
                        CarrierId = CARRIER_ID
                    }
                };
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = SYS_ADMIN_USER_ID,
                        FirstName = "System",
                        LastName = "Admin",
                        IsNotificationsEnabled = true,
                        UserNotifications = new List<UserNotificationEntity>
                        {
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email,
                                NotificationValue = "sysadmin@email.com"
                            },
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.CellPhone,
                                NotificationValue = "123-123-1234"
                            }
                        },
                        SecurityUserAccessRoles = USER_ROLES.Where(x => x.UserId == SYS_ADMIN_USER_ID).ToList(),
                        UserCarrierScacs = USER_CARRIER_SCACS.Where(x => x.UserId == SYS_ADMIN_USER_ID).ToList()
                    },
                    new UserEntity
                    {
                        UserId = LS_ADMIN_USER_ID,
                        FirstName = "LS",
                        LastName = "Admin",
                        IsNotificationsEnabled = false,
                        UserNotifications = new List<UserNotificationEntity>
                        {
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email,
                                NotificationValue = "lsadmin@email.com"
                            },
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.CellPhone,
                                NotificationValue = "123-123-1234"
                            }
                        },
                        SecurityUserAccessRoles = USER_ROLES.Where(x => x.UserId == LS_ADMIN_USER_ID).ToList(),
                        UserCarrierScacs = USER_CARRIER_SCACS.Where(x => x.UserId == LS_ADMIN_USER_ID).ToList()
                    },
                    new UserEntity
                    {
                        UserId = CARRIER_ADMIN_USER_ID,
                        FirstName = "Carrier",
                        LastName = "Admin",
                        IsNotificationsEnabled = false,
                        UserNotifications = new List<UserNotificationEntity>
                        {
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email,
                                NotificationValue = "carrieradmin@email.com"
                            },
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.CellPhone,
                                NotificationValue = "123-123-1234"
                            }
                        },
                        SecurityUserAccessRoles = USER_ROLES.Where(x => x.UserId == CARRIER_ADMIN_USER_ID).ToList(),
                        UserCarrierScacs = USER_CARRIER_SCACS.Where(x => x.UserId == CARRIER_ADMIN_USER_ID).ToList()
                    },
                    new UserEntity
                    {
                        UserId = Guid.Empty,
                        FirstName = "Not",
                        LastName = "Returned",
                        IsNotificationsEnabled = true,
                        UserNotifications = new List<UserNotificationEntity>
                        {
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email,
                                NotificationValue = "user1@email.com"
                            },
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.CellPhone,
                                NotificationValue = "123-123-1234"
                            }
                        },
                        SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>(),
                        UserCarrierScacs = new List<UserCarrierScacEntity>()
                    }
                };
            }
        }
    }
}
