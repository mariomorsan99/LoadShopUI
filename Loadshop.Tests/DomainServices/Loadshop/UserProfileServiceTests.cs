using System;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services;
using Xunit;
using FluentAssertions;
using Moq;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using System.Collections.Generic;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class UserProfileServiceTests
    {
        public class GetPrimaryCustomerIdentUserIdTests
        {
            [Fact]
            public void DoNotNeedToTestSimpleDataRetrievalMethod()
            {
                true.Should().BeTrue();
            }
        }

        public class GetPrimaryCustomerIdTests
        {
            [Fact]
            public void DoNotNeedToTestSimpleDataRetrievalMethod()
            {
                true.Should().BeTrue();
            }
        }

        public class GetPrimaryCustomerOwnerTests
        {
            [Fact]
            public void DoNotNeedToTestSimpleDataRetrievalMethod()
            {
                true.Should().BeTrue();
            }
        }

        public class IsViewOnlyUserAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<ICarrierService> _carrierService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISMSService> _smsService;
            private readonly Mock<IAgreementDocumentService> _agreementDocumentService;

            private IUserProfileService _svc;

            private List<UserEntity> USERS;
            private CarrierScacEntity PRIMARY_SCAC_ENTITY;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string CARRIER_ID = "CarrierId";

            public IsViewOnlyUserAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _carrierService = new Mock<ICarrierService>();
                _carrierService.Setup(x => x.IsActiveCarrier(It.IsAny<string>())).Returns(true);
                _carrierService.Setup(x => x.IsPlanningEligible(It.IsAny<string>())).Returns(true);
                _commonService = new Mock<ICommonService>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetUserRolesAsync()).ReturnsAsync(new List<SecurityAccessRoleData>());
                _userContext = new Mock<IUserContext>();
                _smsService = new Mock<ISMSService>();
                _agreementDocumentService = new Mock<IAgreementDocumentService>();

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
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserProfileService(
                    _db.Object,
                    _mapper,
                    _carrierService.Object,
                    _commonService.Object,
                    _securityService.Object,
                    _userContext.Object,
                    _smsService.Object,
                    _agreementDocumentService.Object);
            }

            [Fact]
            public void NoUsers_ThrowsException()
            {
                USERS = new List<UserEntity>();
                InitDb();
                InitService();

                Func<Task> action = () => _svc.IsViewOnlyUserAsync(USER_ID);
                action.Should().Throw<EntityNotFoundException>($"UserProfile not found for id {USER_ID}");
            }

            [Fact]
            public void InvalidUsers_ThrowsException()
            {
                Func<Task> action = () => _svc.IsViewOnlyUserAsync(Guid.Empty);
                action.Should().Throw<EntityNotFoundException>($"UserProfile not found for id {Guid.Empty}");
            }

            [Fact]
            public async Task NoRoles_ActiveCarrier_PlanningEligible_IsNotViewOnly()
            {
                var actual = await _svc.IsViewOnlyUserAsync(USER_ID);
                actual.Should().BeFalse();
            }

            [Fact]
            public async Task NoRoles_InactiveCarrier_PlanningEligible_IsViewOnly()
            {
                _carrierService.Setup(x => x.IsActiveCarrier(It.IsAny<string>())).Returns(false);
                InitService();

                var actual = await _svc.IsViewOnlyUserAsync(USER_ID);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task NoRoles_ActiveCarrier_NotPlanningEligible_IsViewOnly()
            {
                _carrierService.Setup(x => x.IsPlanningEligible(It.IsAny<string>())).Returns(false);
                InitService();

                var actual = await _svc.IsViewOnlyUserAsync(USER_ID);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task CarrierUserViewOnlyRole_ActiveCarrier_PlanningEligible_IsViewOnly()
            {
                var role = new SecurityAccessRoleData
                {
                    AccessRoleName = SecurityRoles.CarrierUserViewOnly
                };
                _securityService.Setup(x => x.GetUserRolesAsync()).ReturnsAsync(new List<SecurityAccessRoleData> { role });
                InitService();

                var actual = await _svc.IsViewOnlyUserAsync(USER_ID);
                actual.Should().BeTrue();
            }

            [Fact]
            public async Task ShipperUserViewOnlyRole_ActiveCarrier_PlanningEligible_IsViewOnly()
            {
                var role = new SecurityAccessRoleData
                {
                    AccessRoleName = SecurityRoles.ShipperUserViewOnly
                };
                _securityService.Setup(x => x.GetUserRolesAsync()).ReturnsAsync(new List<SecurityAccessRoleData> { role });
                InitService();

                var actual = await _svc.IsViewOnlyUserAsync(USER_ID);
                actual.Should().BeTrue();
            }

            private void InitSeedData()
            {
                PRIMARY_SCAC_ENTITY = new CarrierScacEntity
                {
                    CarrierId = CARRIER_ID
                };
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        IdentUserId = USER_ID,
                        PrimaryScacEntity = PRIMARY_SCAC_ENTITY
                    }
                };
            }
        }

        public class GetUserProfileAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<ICarrierService> _carrierService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISMSService> _smsService;
            private readonly Mock<IAgreementDocumentService> _agreementDocumentService;

            private IUserProfileService _svc;

            private UserEntity USER;
            private CarrierScacEntity PRIMARY_SCAC_ENTITY;
            private List<MessageTypeEntity> MESSAGE_TYPES;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string CARRIER_ID = "CarrierId";

            public GetUserProfileAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _carrierService = new Mock<ICarrierService>();
                _carrierService.Setup(x => x.IsActiveCarrier(It.IsAny<string>())).Returns(true);
                _carrierService.Setup(x => x.IsPlanningEligible(It.IsAny<string>())).Returns(true);
                _commonService = new Mock<ICommonService>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetUserRolesAsync()).ReturnsAsync(new List<SecurityAccessRoleData>());
                _securityService.Setup(x => x.GetUserRoles()).Returns(new List<SecurityAccessRoleData>());
                _userContext = new Mock<IUserContext>();
                _smsService = new Mock<ISMSService>();
                _agreementDocumentService = new Mock<IAgreementDocumentService>();
                _agreementDocumentService.Setup(x => x.HasUserAgreedToLatestTermsAndPrivacy(It.IsAny<Guid>())).ReturnsAsync(value: true);

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
                    .WithUser(USER)
                    .WithMessageTypes(MESSAGE_TYPES)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserProfileService(
                    _db.Object,
                    _mapper,
                    _carrierService.Object,
                    _commonService.Object,
                    _securityService.Object,
                    _userContext.Object,
                    _smsService.Object,
                    _agreementDocumentService.Object);
            }

            [Fact]
            public void NoUsers_ThrowsException()
            {
                USER = new UserEntity();
                InitDb();
                InitService();

                Func<Task> action = () => _svc.GetUserProfileAsync(USER_ID);
                action.Should().Throw<EntityNotFoundException>($"UserProfile not found for id {USER_ID}");
            }

            [Fact]
            public void InvalidUsers_ThrowsException()
            {
                Func<Task> action = () => _svc.GetUserProfileAsync(Guid.Empty);
                action.Should().Throw<EntityNotFoundException>($"UserProfile not found for id {Guid.Empty}");
            }

            [Fact]
            public async Task MissingEmailUserNotificationIsInserted()
            {
                USER.UserNotifications = new List<UserNotificationEntity>();

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.GetUserProfileAsync(USER_ID);
                builder.MockUserNotifications.Verify(x => x.Add(It.IsAny<UserNotificationEntity>()), Times.Once);
            }

            [Fact]
            public async Task MissingUserNotificationsAreReturned_ExcludingEmailSingleCarrierScac()
            {
                var expectedCount = MESSAGE_TYPES
                    .Where(x => x.MessageTypeId != MessageTypeConstants.Email_SingleCarrierScac)
                    .Count();
                USER.UserNotifications = new List<UserNotificationEntity>();
                InitDb();
                InitService();

                var actual = await _svc.GetUserProfileAsync(USER_ID);
                actual.UserNotifications.Should().HaveCount(expectedCount);
            }

            [Fact]
            public async Task ReturnsUserProfile()
            {
                var actual = await _svc.GetUserProfileAsync(USER_ID);
                actual.Should().NotBeNull();
            }

            private void InitSeedData()
            {
                PRIMARY_SCAC_ENTITY = new CarrierScacEntity
                {
                    CarrierId = CARRIER_ID
                };
                MESSAGE_TYPES = new List<MessageTypeEntity>
                {
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.Email,
                        MessageTypeDesc = "Email"
                    },
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.Email_SingleCarrierScac,
                        MessageTypeDesc = "Email Single Carrier Scac"
                    },
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.Phone,
                        MessageTypeDesc = "Phone"
                    },
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        MessageTypeDesc = "Cell Phone"
                    }
                };
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID,
                    PrimaryScacEntity = PRIMARY_SCAC_ENTITY,
                    UserNotifications = new List<UserNotificationEntity>
                        {
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email,
                                NotificationEnabled = true,
                                NotificationValue = "user@email.com"
                            }
                        }
                };
            }
        }

        public class SaveUserProfileAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<ICarrierService> _carrierService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISMSService> _smsService;
            private readonly Mock<IAgreementDocumentService> _agreementDocumentService;

            private IUserProfileService _svc;

            private UserProfileData USER_DATA;
            private UserEntity USER;
            private CarrierScacEntity PRIMARY_SCAC_ENTITY;
            private List<MessageTypeEntity> MESSAGE_TYPES;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string USERNAME = "username";
            private static readonly string CARRIER_ID = "CarrierId";

            public SaveUserProfileAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _carrierService = new Mock<ICarrierService>();
                _carrierService.Setup(x => x.IsActiveCarrier(It.IsAny<string>())).Returns(true);
                _carrierService.Setup(x => x.IsPlanningEligible(It.IsAny<string>())).Returns(true);
                _commonService = new Mock<ICommonService>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetUserRolesAsync()).ReturnsAsync(new List<SecurityAccessRoleData>());
                _securityService.Setup(x => x.GetUserRoles()).Returns(new List<SecurityAccessRoleData>());
                _userContext = new Mock<IUserContext>();
                _smsService = new Mock<ISMSService>();
                _smsService.Setup(x => x.ValidateNumber(It.IsAny<string>())).ReturnsAsync(true);
                _agreementDocumentService = new Mock<IAgreementDocumentService>();
                _agreementDocumentService.Setup(x => x.HasUserAgreedToLatestTermsAndPrivacy(It.IsAny<Guid>())).ReturnsAsync(value: true);

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
                    .WithUser(USER)
                    .WithMessageTypes(MESSAGE_TYPES)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserProfileService(
                    _db.Object,
                    _mapper,
                    _carrierService.Object,
                    _commonService.Object,
                    _securityService.Object,
                    _userContext.Object,
                    _smsService.Object,
                    _agreementDocumentService.Object);
            }

            [Fact]
            public void NoUsers_ThrowsException()
            {
                USER = new UserEntity();
                InitDb();
                InitService();

                Func<Task> action = () => _svc.SaveUserProfileAsync(USER_DATA, USERNAME);
                action.Should().Throw<EntityNotFoundException>($"UserProfile not found for id {USER_ID}");
            }

            [Fact]
            public void InvalidUsers_ThrowsException()
            {
                Func<Task> action = () => _svc.SaveUserProfileAsync(new UserProfileData { UserId = Guid.Empty }, USERNAME);
                action.Should().Throw<EntityNotFoundException>($"UserProfile not found for id {Guid.Empty}");
            }

            [Fact]
            public async Task NullUserNotifications_ReturnsErrors()
            {
                USER_DATA.UserNotifications = null;
                var response = await _svc.SaveUserProfileAsync(USER_DATA, USERNAME);
                response.ModelState.IsValid.Should().BeFalse();
                var errors = response.ModelState["urn:UserProfile"];
                errors.Should().NotBeNull();
                errors.Errors.Should().NotBeEmpty();
            }

            [Fact]
            public async Task EmptyUserNotifications_ReturnsErrors()
            {
                USER_DATA.UserNotifications = new List<UserNotificationData>();
                var response = await _svc.SaveUserProfileAsync(USER_DATA, USERNAME);
                response.ModelState.IsValid.Should().BeFalse();
                var errors = response.ModelState["urn:UserProfile"];
                errors.Should().NotBeNull();
                errors.Errors.Should().NotBeEmpty();
            }

            [Fact]
            public async Task MissingEmailNotification_ReturnsErrors()
            {
                USER_DATA.UserNotifications.RemoveAll(x => x.MessageTypeId == MessageTypeConstants.Email);
                var response = await _svc.SaveUserProfileAsync(USER_DATA, USERNAME);
                response.ModelState.IsValid.Should().BeFalse();
                var errors = response.ModelState["urn:UserProfile"];
                errors.Should().NotBeNull();
                errors.Errors.Should().NotBeEmpty();
            }

            [Fact]
            public async Task MissingPhoneOrCellPhoneNotification_ReturnsErrors()
            {
                USER_DATA.UserNotifications.RemoveAll(x => x.MessageTypeId == MessageTypeConstants.Phone || x.MessageTypeId == MessageTypeConstants.CellPhone);
                var response = await _svc.SaveUserProfileAsync(USER_DATA, USERNAME);
                response.ModelState.IsValid.Should().BeFalse();
                var errors = response.ModelState["urn:UserProfile"];
                errors.Should().NotBeNull();
                errors.Errors.Should().NotBeEmpty();
            }

            [Fact]
            public async Task InvalidCellPhoneNumber_ReturnsErrors()
            {
                _smsService.Setup(x => x.ValidateNumber(It.IsAny<string>())).ReturnsAsync(false);
                InitService();

                var response = await _svc.SaveUserProfileAsync(USER_DATA, USERNAME);
                response.ModelState.IsValid.Should().BeFalse();
                var errors = response.ModelState["urn:UserProfile"];
                errors.Should().NotBeNull();
                errors.Errors.Should().NotBeEmpty();
            }

            [Fact]
            public async Task SavesSuccessfully()
            {
                var response = await _svc.SaveUserProfileAsync(USER_DATA, USERNAME);
                response.Should().NotBeNull();
                _db.Verify(x => x.SaveChangesAsync(USERNAME, default(CancellationToken)), Times.Once);
            }

            private void InitSeedData()
            {
                USER_DATA = new UserProfileData
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID,
                    UserNotifications = new List<UserNotificationData>
                    {
                        new UserNotificationData
                        {
                            MessageTypeId = MessageTypeConstants.Email
                        },
                        new UserNotificationData
                        {
                            MessageTypeId = MessageTypeConstants.CellPhone
                        },
                        new UserNotificationData
                        {
                            MessageTypeId = MessageTypeConstants.Phone
                        }
                    }
                };
                PRIMARY_SCAC_ENTITY = new CarrierScacEntity
                {
                    CarrierId = CARRIER_ID
                };
                MESSAGE_TYPES = new List<MessageTypeEntity>
                {
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.Email,
                        MessageTypeDesc = "Email"
                    },
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.Email_SingleCarrierScac,
                        MessageTypeDesc = "Email Single Carrier Scac"
                    },
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.Phone,
                        MessageTypeDesc = "Phone"
                    },
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        MessageTypeDesc = "Cell Phone"
                    }
                };
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID,
                    PrimaryScacEntity = PRIMARY_SCAC_ENTITY,
                    UserNotifications = new List<UserNotificationEntity>
                        {
                            new UserNotificationEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email,
                                NotificationEnabled = true,
                                NotificationValue = "user@email.com"
                            }
                        }
                };
            }
        }

        public class UpdateUserDataTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<ICarrierService> _carrierService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISMSService> _smsService;
            private readonly Mock<IAgreementDocumentService> _agreementDocumentService;

            private IUserProfileService _svc;

            private UserEntity USER;
            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string USERNAME = "username";
            private static readonly string FIRST_NAME = "first";
            private static readonly string LAST_NAME = "last";

            public UpdateUserDataTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _carrierService = new Mock<ICarrierService>();
                _commonService = new Mock<ICommonService>();
                _securityService = new Mock<ISecurityService>();
                _userContext = new Mock<IUserContext>();
                _smsService = new Mock<ISMSService>();
                _agreementDocumentService = new Mock<IAgreementDocumentService>();

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
                    .WithUser(USER)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserProfileService(
                    _db.Object,
                    _mapper,
                    _carrierService.Object,
                    _commonService.Object,
                    _securityService.Object,
                    _userContext.Object,
                    _smsService.Object,
                    _agreementDocumentService.Object);
            }

            [Fact]
            public void NoUsers_NoChangesSaved()
            {
                USER = new UserEntity();
                InitDb();
                InitService();

                _svc.UpdateUserData(USER_ID, USERNAME, FIRST_NAME, LAST_NAME);
                _db.Verify(x => x.SaveChanges(USERNAME), Times.Never);
            }

            [Fact]
            public void InvalidUserId_NoChangesSaved()
            {
                _svc.UpdateUserData(Guid.Empty, USERNAME, FIRST_NAME, LAST_NAME);
                _db.Verify(x => x.SaveChanges(USERNAME), Times.Never);
            }

            [Fact]
            public void SavesChanges()
            {
                _svc.UpdateUserData(USER_ID, USERNAME, FIRST_NAME, LAST_NAME);
                _db.Verify(x => x.SaveChanges(USERNAME), Times.Once);
            }

            private void InitSeedData()
            {
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID
                };
            }
        }

        public class UpdateFocusEntityAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<ICarrierService> _carrierService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISMSService> _smsService;
            private readonly Mock<IAgreementDocumentService> _agreementDocumentService;

            private IUserProfileService _svc;

            private UserEntity USER;
            private UserFocusEntityData USER_FOCUS_ENTITY;
            private List<CarrierEntity> CARRIERS;
            private List<UserCarrierScacEntity> USER_CARRIER_SCACS;
            private List<UserShipperEntity> USER_SHIPPERS;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string USERNAME = "username";
            private static readonly string SCAC = "SCAC";
            private static readonly string CARRIER_ID = "CarrierId";

            private UserFocusEntityData SHIPPER_ENTITY = new UserFocusEntityData
            {
                Id = CUSTOMER_ID.ToString(),
                Type = UserFocusEntityType.Shipper,
            };
            private UserFocusEntityData CARRIER_SCAC_ENTITY = new UserFocusEntityData
            {
                Id = SCAC,
                Type = UserFocusEntityType.CarrierScac,
            };

            public UpdateFocusEntityAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _carrierService = new Mock<ICarrierService>();
                _commonService = new Mock<ICommonService>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetCustomerContractedScacsByPrimaryCustomer()).Returns(new List<CarrierScacData>());
                _securityService.Setup(x => x.GetUserRoles()).Returns(new List<SecurityAccessRoleData>());
                _securityService.Setup(x => x.UserHasRoleAsync(It.IsAny<string[]>())).ReturnsAsync(true);
                _userContext = new Mock<IUserContext>();
                _smsService = new Mock<ISMSService>();
                _agreementDocumentService = new Mock<IAgreementDocumentService>();

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
                    .WithUser(USER)
                    .WithCarriers(CARRIERS)
                    .WithUserCarrierScacs(USER_CARRIER_SCACS)
                    .WithUserShippers(USER_SHIPPERS)
                    .WithCarrierScacs(new List<CarrierScacEntity>())
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserProfileService(
                    _db.Object,
                    _mapper,
                    _carrierService.Object,
                    _commonService.Object,
                    _securityService.Object,
                    _userContext.Object,
                    _smsService.Object,
                    _agreementDocumentService.Object);
            }

            [Fact]
            public void NoUsers_ThrowsException()
            {
                var expected = $"*UserEntity could not be found for id: {USER_ID}*";
                USER = new UserEntity();
                InitDb();
                InitService();

                _svc.Awaiting(x => x.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void InvalidUserId_ThrowsException()
            {
                var invalidUserId = new Guid("22222222-2222-2222-2222-222222222222");
                var expected = $"*UserEntity could not be found for id: {invalidUserId}*";

                _svc.Awaiting(x => x.UpdateFocusEntityAsync(invalidUserId, USER_FOCUS_ENTITY, USERNAME))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void MissingIdentUserId_ThrowsException()
            {
                var expected = "*identUserId cannot be null*";
                _svc.Awaiting(x => x.UpdateFocusEntityAsync(Guid.Empty, USER_FOCUS_ENTITY, USERNAME))
                    .Should().Throw<ArgumentNullException>()
                    .WithMessage(expected);
            }

            [Fact]
            public void MissingUserFocusEntityData_ThrowsException()
            {
                var expected = "*userFocusEntityData cannot be null*";
                _svc.Awaiting(x => x.UpdateFocusEntityAsync(USER_ID, null, USERNAME))
                    .Should().Throw<ArgumentNullException>()
                    .WithMessage(expected);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingUserName_ThrowsException(string username)
            {
                var expected = "*userName cannot be null*";
                _svc.Awaiting(x => x.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, username))
                    .Should().Throw<ArgumentNullException>()
                    .WithMessage(expected);
            }

            [Fact]
            public void Shipper_InvalidCustomerId_ThrowsException()
            {
                var entityId = "invalid";
                var expected = $"*Invalid customer id: {entityId}*";
                USER_FOCUS_ENTITY.Id = entityId;

                _svc.Awaiting(x => x.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME))
                   .Should().Throw<Exception>()
                   .WithMessage(expected);
            }

            [Fact]
            public async Task Shipper_NotAdmin_DoesNotAddUserShipper()
            {
                _securityService.Setup(x => x.UserHasRoleAsync(It.IsAny<string[]>())).ReturnsAsync(false);

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var result = await _svc.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME);
                builder.MockUserShippers.Verify(x => x.Add(It.IsAny<UserShipperEntity>()), Times.Never);
            }

            [Fact]
            public async Task Shipper_Admin_HasShipper_DoesNotAddUserShipper()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var result = await _svc.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME);
                builder.MockUserShippers.Verify(x => x.Add(It.IsAny<UserShipperEntity>()), Times.Never);
            }

            [Fact]
            public async Task Shipper_Admin_DoesNotHaveShipper_UserShipperAdded()
            {
                USER_SHIPPERS = new List<UserShipperEntity>();

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var result = await _svc.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME);
                builder.MockUserShippers.Verify(x => x.Add(It.IsAny<UserShipperEntity>()), Times.Once);
            }

            [Fact]
            public async Task CarrierScac_NotAdmin_DoesNotAddUserCarrierScac()
            {
                USER_FOCUS_ENTITY = CARRIER_SCAC_ENTITY;
                _securityService.Setup(x => x.UserHasRoleAsync(It.IsAny<string[]>())).ReturnsAsync(false);

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var result = await _svc.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME);
                builder.MockUserCarrierScacs.Verify(x => x.Add(It.IsAny<UserCarrierScacEntity>()), Times.Never);
            }

            [Fact]
            public async Task CarrierScac_Admin_AddsUserCarrierScac()
            {
                USER_FOCUS_ENTITY = CARRIER_SCAC_ENTITY;

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var result = await _svc.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME);
                builder.MockUserCarrierScacs.Verify(x => x.Add(It.IsAny<UserCarrierScacEntity>()), Times.Once);
            }

            [Fact]
            public void CarrierScac_Admin_NotCarrierIdFound_ThrowsException()
            {
                USER_FOCUS_ENTITY = CARRIER_SCAC_ENTITY;
                CARRIERS.First().CarrierId = null;

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var expected = $"Cannot find Carrier for SCAC: {SCAC}";
                _svc.Awaiting(x => x.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME))
                   .Should().Throw<Exception>()
                   .WithMessage(expected);
            }

            [Fact]
            public async Task CarrierScac_Admin_HasUserCarrierScacAlready_NoUserCarrierScacAdded()
            {
                USER_FOCUS_ENTITY = CARRIER_SCAC_ENTITY;
                USER_CARRIER_SCACS.Add(new UserCarrierScacEntity
                {
                    UserId = USER_ID,
                    CarrierId = CARRIER_ID,
                    Scac = SCAC
                });

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var result = await _svc.UpdateFocusEntityAsync(USER_ID, USER_FOCUS_ENTITY, USERNAME);
                builder.MockUserCarrierScacs.Verify(x => x.Add(It.IsAny<UserCarrierScacEntity>()), Times.Never);
            }

            private void InitSeedData()
            {
                USER_FOCUS_ENTITY = SHIPPER_ENTITY;
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID
                };
                CARRIERS = new List<CarrierEntity>
                {
                    new CarrierEntity
                    {
                        CarrierId = CARRIER_ID,
                        CarrierScacs = new List<CarrierScacEntity>
                        {
                            new CarrierScacEntity
                            {
                                Scac = SCAC
                            }
                        }
                    }
                };
                USER_CARRIER_SCACS = new List<UserCarrierScacEntity>();
                USER_SHIPPERS = new List<UserShipperEntity>
                {
                    new UserShipperEntity
                    {
                        CustomerId = CUSTOMER_ID,
                        UserId = USER_ID
                    }
                };
            }
        }

        public class CreateUserProfileAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<ICarrierService> _carrierService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISMSService> _smsService;
            private readonly Mock<IAgreementDocumentService> _agreementDocumentService;

            private IUserProfileService _svc;

            private List<UserEntity> USERS;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string EMAIL = "user@email.com";
            private static readonly string USERNAME = "username";
            private static readonly string FIRST_NAME = "First";
            private static readonly string LAST_NAME = "Last";

            private static readonly string SCAC = "SCAC";

            public CreateUserProfileAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _carrierService = new Mock<ICarrierService>();
                _carrierService.Setup(x => x.IsActiveCarrier(It.IsAny<string>())).Returns(true);
                _carrierService.Setup(x => x.IsPlanningEligible(It.IsAny<string>())).Returns(true);
                _commonService = new Mock<ICommonService>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac()).Returns(new List<CarrierScacData>());
                _userContext = new Mock<IUserContext>();
                _smsService = new Mock<ISMSService>();
                _agreementDocumentService = new Mock<IAgreementDocumentService>();

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
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserProfileService(
                    _db.Object,
                    _mapper,
                    _carrierService.Object,
                    _commonService.Object,
                    _securityService.Object,
                    _userContext.Object,
                    _smsService.Object,
                    _agreementDocumentService.Object);
            }

            [Fact]
            public void UserExists_ThrowsException()
            {
                USERS.Add(new UserEntity
                {
                    IdentUserId = USER_ID
                });
                InitDb();
                InitService();

                var expected = "User already exists";
                _svc.Awaiting(x => x.CreateUserProfileAsync(USER_ID, CARRIER_ID, EMAIL, USERNAME, FIRST_NAME, LAST_NAME))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void UserInserted()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = _svc.CreateUserProfileAsync(USER_ID, CARRIER_ID, EMAIL, USERNAME, FIRST_NAME, LAST_NAME);
                builder.MockUsers.Verify(x => x.Add(It.IsAny<UserEntity>()), Times.Once);
                _db.Verify(x => x.SaveChangesAsync(USERNAME, default), Times.Once);
            }

            [Fact]
            public void UserInserted_PrimaryScacSetProperly()
            {
                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac())
                    .Returns(new List<CarrierScacData>
                    {
                        new CarrierScacData
                        {
                            Scac = SCAC
                        }
                    });
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                UserEntity added = null;
                builder.MockUsers.Setup(x => x.Add(It.IsAny<UserEntity>())).Callback((UserEntity _) => { added = _; });

                var actual = _svc.CreateUserProfileAsync(USER_ID, CARRIER_ID, EMAIL, USERNAME, FIRST_NAME, LAST_NAME);
                added.PrimaryScac.Should().Be(SCAC);
            }

            [Fact]
            public void UserInserted_NewUserNotificationsCreated()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                UserEntity added = null;
                builder.MockUsers.Setup(x => x.Add(It.IsAny<UserEntity>())).Callback((UserEntity _) => { added = _; });

                var actual = _svc.CreateUserProfileAsync(USER_ID, CARRIER_ID, EMAIL, USERNAME, FIRST_NAME, LAST_NAME);
                added.UserNotifications.Should().HaveCount(2);
                added.UserNotifications
                    .First(x => x.MessageTypeId == MessageTypeConstants.Email)
                    .NotificationValue.Should().Be(EMAIL);
            }

            [Fact]
            public void UserInserted_BaseUserPropertiesMapped()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                UserEntity added = null;
                builder.MockUsers.Setup(x => x.Add(It.IsAny<UserEntity>())).Callback((UserEntity _) => { added = _; });

                var actual = _svc.CreateUserProfileAsync(USER_ID, CARRIER_ID, EMAIL, USERNAME, FIRST_NAME, LAST_NAME);
                added.IdentUserId.Should().Be(USER_ID);
                added.Username.Should().Be(USERNAME);
                added.FirstName.Should().Be(FIRST_NAME);
                added.LastName.Should().Be(LAST_NAME);
                added.IsNotificationsEnabled.Should().BeTrue();
            }

            private void InitSeedData()
            {
                USERS = new List<UserEntity>();
            }
        }
    }
}
