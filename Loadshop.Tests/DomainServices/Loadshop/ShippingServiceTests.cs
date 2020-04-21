using AutoMapper;
using FluentAssertions;
using Loadshop.Data;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Common.Services.Data;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class ShippingServiceTests
    {
        public class GetLoadDetailByIdTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private const string STATE_NAME = "state";
            private const string USERNAME = "username";
            private static Guid VALID_LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_LOAD_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private static Guid VALID_CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_CUSTOMER_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private LoadEntity LOAD = new LoadEntity
            {
                LoadId = VALID_LOAD_ID,
                ManuallyCreated = true,
                Customer = new CustomerEntity
                {
                    IdentUserId = VALID_CUSTOMER_ID
                },
                LoadStops = new List<LoadStopEntity>()
                {
                    new LoadStopEntity()
                    {
                        StopNbr = 1,
                        Contacts = new List<LoadStopContactEntity>()
                        {
                            new LoadStopContactEntity()
                            {
                                FirstName = "first"
                            }
                        },
                        DeliveryLineItems = new List<LoadLineItemEntity>()
                        {
                            new LoadLineItemEntity()
                            {
                                LoadLineItemNumber = 1
                            }
                        }
                    }
                },
                Contacts = new List<LoadContactEntity>()
                {
                    new LoadContactEntity()
                    {
                        Display = "contact name"
                    }
                },
                LoadServiceTypes = new List<LoadServiceTypeEntity>()
                {
                    new LoadServiceTypeEntity()
                    {
                        ServiceTypeId = 1
                    }
                },
                PostedLoadCarrierGroups = new List<PostedLoadCarrierGroupEntity>()
                {

                }
            };

            public GetLoadDetailByIdTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .Build();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });

                _commonService = new Mock<ICommonService>();
                _commonService.Setup(x => x.GetUSCANStateProvince(It.IsAny<string>())).Returns(new StateData() { Name = STATE_NAME });

                _notificationService = new Mock<INotificationService>();
                _ratingService = new Mock<IRatingService>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void LoadNotFound()
            {
                _svc = CreateService();

                _svc.Invoking(x => x.GetLoadDetailById(INVALID_LOAD_ID)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void LoadNotManuallyCreated()
            {
                var load = LOAD;
                load.ManuallyCreated = false;

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Invoking(x => x.GetLoadDetailById(VALID_LOAD_ID)).Should().Throw<Exception>().WithMessage($"Load was not manually created");
            }

            [Fact]
            public void LoadNotAuthorizedForCustomer()
            {
                var load = LOAD;

                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>());
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Invoking(x => x.GetLoadDetailById(VALID_LOAD_ID)).Should().Throw<Exception>().WithMessage($"User is not authorized for customer: {load.CustomerId}");

            }

            [Fact]
            public void GetLoadDetailById()
            {
                _svc = CreateService();

                var response = _svc.GetLoadDetailById(VALID_LOAD_ID);
                response.Should().NotBeNull();
                response.LoadId.Should().Be(VALID_LOAD_ID);
                response.Contacts.Should().NotBeNullOrEmpty();
                response.ServiceTypes.Should().NotBeNullOrEmpty();
                response.LineItems.Should().NotBeNullOrEmpty();

                response.LoadStops.Should().NotBeNullOrEmpty();
                foreach (var item in response.LoadStops)
                {
                    item.Should().NotBeNull();
                    item.StateName.Should().Be(STATE_NAME);
                    item.Contacts.Should().NotBeNullOrEmpty();
                }
            }

            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }
        }

        public class GetDefaultLoadDetailTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private static Guid USER_ID = Guid.NewGuid();
            private static Guid IDENT_USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid IDENT_USER_ID_WITHOUT_CUSTOMER = Guid.Parse("11111111-1111-1111-1111-111111111112");
            private static Guid IDENT_USER_ID_WITHOUT_CUSTOMER_COMMODITY = Guid.Parse("11111111-1111-1111-1111-111111111113");
            private static Guid IDENT_USER_ID_WITH_COMMODITY = Guid.Parse("11111111-1111-1111-1111-111111111114");
            private static Guid CUSTOMER_ID = Guid.Parse("22222222-2222-2222-2222-222222222223");
            private static Guid CUSTOMER_ID_WITHOUT_COMMODITY = Guid.Parse("22222222-2222-2222-2222-222222222224");
            private static string COMMODITY = "commodity";
            private static string PROFILE_COMMODITY = "profile-commodity";

            private List<UserEntity> USERS = new List<UserEntity>()
            {
                new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = IDENT_USER_ID,
                    PrimaryCustomerId = CUSTOMER_ID,
                    FirstName = "first",
                    LastName = "last"
                },
                new UserEntity
                {
                    IdentUserId = IDENT_USER_ID_WITHOUT_CUSTOMER
                },
                new UserEntity
                {
                    IdentUserId = IDENT_USER_ID_WITHOUT_CUSTOMER_COMMODITY,
                    PrimaryCustomerId = CUSTOMER_ID_WITHOUT_COMMODITY
                },
                new UserEntity
                {
                    IdentUserId = IDENT_USER_ID_WITH_COMMODITY,
                    DefaultCommodity = PROFILE_COMMODITY
                }
            };
            private List<UserNotificationEntity> USER_NOTIFICATIONS = new List<UserNotificationEntity>()
            {
                new UserNotificationEntity
                {
                    UserId = USER_ID,
                    MessageTypeId = MessageTypeConstants.Email,
                    NotificationValue = "email@domain.com"
                },
                new UserNotificationEntity
                {
                    UserId = USER_ID,
                    MessageTypeId =  MessageTypeConstants.CellPhone,
                    NotificationValue = "123-456-7890"
                }
            };
            private List<CustomerEntity> CUSTOMERS = new List<CustomerEntity>()
            {
                new CustomerEntity()
                {
                    CustomerId = CUSTOMER_ID,
                    DefaultCommodity = COMMODITY
                },
                new CustomerEntity()
                {
                    CustomerId = CUSTOMER_ID_WITHOUT_COMMODITY
                }
            };

            public GetDefaultLoadDetailTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder()
                    .WithUsers(USERS)
                    .WithUserNotifications(USER_NOTIFICATIONS)
                    .WithCustomers(CUSTOMERS)
                    .Build();
                _securityService = new Mock<ISecurityService>();
                _commonService = new Mock<ICommonService>();
                _notificationService = new Mock<INotificationService>();
                _ratingService = new Mock<IRatingService>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);

                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void UserNotFound()
            {
                _svc = CreateService();

                var load = _svc.GetDefaultLoadDetail(Guid.NewGuid());
                ValidateLoadDetail(load);
            }

            private void ValidateLoadDetail(OrderEntryLoadDetailData load)
            {
                load.Should().NotBeNull();
                load.TransportationMode.Should().Be("TRUCK");

                load.LoadStops.Should().NotBeNullOrEmpty();
                load.LoadStops.Count.Should().Be(2);

                var firstStop = load.LoadStops[0];
                firstStop.Should().NotBeNull();
                firstStop.StopNbr.Should().Be(1);
                firstStop.StopType.Should().Be("Pickup");
                firstStop.Country.Should().Be("USA");
                firstStop.IsLive.Should().BeTrue();

                var secondStop = load.LoadStops[1];
                secondStop.Should().NotBeNull();
                secondStop.StopNbr.Should().Be(2);
                secondStop.StopType.Should().Be("Delivery");
                secondStop.Country.Should().Be("USA");
                secondStop.IsLive.Should().BeTrue();

                load.LineItems.Should().NotBeNullOrEmpty();
                load.LineItems.Count.Should().Be(1);

                var firstLine = load.LineItems[0];
                firstLine.Should().NotBeNull();
                firstLine.PickupStopNumber.Should().Be(1);
                firstLine.DeliveryStopNumber.Should().Be(2);
            }

            [Fact]
            public void NoCustomerForUser()
            {
                _svc = CreateService();

                var load = _svc.GetDefaultLoadDetail(IDENT_USER_ID_WITHOUT_CUSTOMER_COMMODITY);
                ValidateLoadDetail(load);
            }

            [Fact]
            public void NoDefaultCommodityForCustomer()
            {
                _svc = CreateService();

                var load = _svc.GetDefaultLoadDetail(IDENT_USER_ID_WITHOUT_CUSTOMER_COMMODITY);
                ValidateLoadDetail(load);
            }

            [Fact]
            public void DefaultCommodityForCustomer()
            {
                _svc = CreateService();

                var load = _svc.GetDefaultLoadDetail(IDENT_USER_ID);
                ValidateLoadDetail(load);
                load.Commodity.Should().Be(COMMODITY);
                load.Contacts.Should().NotBeNullOrEmpty();
                load.Contacts.Count.Should().Be(1);

                var contact = load.Contacts[0];
                contact.Should().NotBeNull();
                contact.Display.Should().Be("first last");
                contact.Email.Should().Be("email@domain.com");
                contact.Phone.Should().Be("123-456-7890");
            }

            [Fact]
            public void DefaultCommodityFromUserProfile()
            {
                _svc = CreateService();

                var load = _svc.GetDefaultLoadDetail(IDENT_USER_ID_WITH_COMMODITY);
                ValidateLoadDetail(load);
                load.Commodity.Should().Be(PROFILE_COMMODITY);
            }

            [Fact]
            public void ShouldBeEmptyPhoneIfNoNotifications()
            {
                var emailNotificationsOnly = USER_NOTIFICATIONS.Where(x => x.MessageTypeId == MessageTypeConstants.Email).ToList();

                _db = new MockDbBuilder()
                   .WithUsers(USERS)
                   .WithUserNotifications(emailNotificationsOnly)
                   .WithCustomers(CUSTOMERS)
                   .Build();

                _svc = CreateService();

                var load = _svc.GetDefaultLoadDetail(IDENT_USER_ID);
                ValidateLoadDetail(load);
                load.Commodity.Should().Be(COMMODITY);
                load.Contacts.Should().NotBeNullOrEmpty();
                load.Contacts.Count.Should().Be(1);

                var contact = load.Contacts[0];
                contact.Should().NotBeNull();
                contact.Display.Should().Be("first last");
                contact.Email.Should().Be("email@domain.com");
                contact.Phone.Should().BeNull();
            }

            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }
        }

        public class RemoveLoadTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private const string USERNAME = "username";
            private static Guid VALID_LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_LOAD_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private static Guid VALID_CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_CUSTOMER_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private LoadEntity VALID_LOAD = new LoadEntity
            {
                LoadId = VALID_LOAD_ID,
                Customer = new CustomerEntity
                {
                    IdentUserId = VALID_CUSTOMER_ID
                },
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Updated,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(5)
                    }
                },
                PostedLoadCarrierGroups = new List<PostedLoadCarrierGroupEntity>()
            };

            public RemoveLoadTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder().Build();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });
                _ratingService = new Mock<IRatingService>();
                _commonService = new Mock<ICommonService>();
                _notificationService = new Mock<INotificationService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithEmptyDb()
            {
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveLoad(VALID_LOAD_ID, USERNAME)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithInvalidLoadId()
            {
                var load = VALID_LOAD;
                load.LoadId = INVALID_LOAD_ID;
                load.CustomerId = Guid.NewGuid();

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                var loadId = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var customerId = Guid.NewGuid();


                _svc.Awaiting(x => x.RemoveLoad(loadId, USERNAME)).Should().Throw<Exception>().WithMessage($"User is not authorized for customer: {load.CustomerId}");
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithInvalidCustomerId()
            {
                var load = VALID_LOAD;

                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>());
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveLoad(VALID_LOAD_ID, USERNAME)).Should().Throw<Exception>().WithMessage("User is not authorized for customer: 00000000-0000-0000-0000-000000000000");

            }

            [Fact]
            public void ShouldThrowLoadMyNotBeRemovedWhenMissingLoadTransactions()
            {
                var load = VALID_LOAD;
                load.LoadTransactions = null;

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveLoad(VALID_LOAD_ID, USERNAME)).Should()
                    .Throw<Exception>()
                    .Where(x => x.Message.StartsWith("Load may not be removed"));

            }

            [Fact]
            public void ShouldThrowLoadMyNotBeRemovedWhenEmptyLoadTransactions()
            {
                var load = VALID_LOAD;
                load.LoadTransactions = new List<LoadTransactionEntity>();

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveLoad(VALID_LOAD_ID, USERNAME)).Should()
                    .Throw<Exception>()
                    .Where(x => x.Message.StartsWith("Load may not be removed"));

            }

            [Theory]
            [InlineData(TransactionTypes.Accepted)]
            [InlineData(TransactionTypes.Pending)]
            [InlineData(TransactionTypes.SentToShipperTender)]
            [InlineData(TransactionTypes.Removed)]
            [InlineData(TransactionTypes.PendingRemove)]
            public void ShouldThrowLoadMyNotBeRemovedWhenInvalidTransactionType(string transactionTypeId)
            {
                var load = VALID_LOAD;
                load.LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = transactionTypeId,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    }
                };

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveLoad(VALID_LOAD_ID, USERNAME)).Should()
                    .Throw<Exception>()
                    .Where(x => x.Message.StartsWith("Load may not be removed"));

            }

            [Theory]
            [InlineData(TransactionTypes.PendingAdd)]
            [InlineData(TransactionTypes.PendingUpdate)]
            public async Task ShouldDoNothingIfLoadAlreadyInInitialState(string transactionTypeId)
            {
                var load = VALID_LOAD;
                load.LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = transactionTypeId,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    }
                };

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                var response = await _svc.RemoveLoad(VALID_LOAD_ID, USERNAME);
                response.Should().NotBeNull();
                response.OnLoadshop.Should().BeFalse();
                _db.Verify(x => x.SaveChanges(USERNAME), Times.Never);
            }

            [Theory]
            [InlineData(TransactionTypes.Posted)]
            [InlineData(TransactionTypes.PendingRates)]
            [InlineData(TransactionTypes.New)]
            [InlineData(TransactionTypes.Updated)]
            [InlineData(TransactionTypes.PendingRemoveScac)]
            public async Task ShouldAddLoadTransactionIfValidState(string transactionTypeId)
            {
                var load = VALID_LOAD;
                load.LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = transactionTypeId,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    }
                };

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                var response = await _svc.RemoveLoad(VALID_LOAD_ID, USERNAME);
                response.Should().NotBeNull();
                response.OnLoadshop.Should().BeFalse();
                _db.Verify(x => x.SaveChangesAsync(USERNAME, It.IsAny<CancellationToken>()), Times.Once);
            }
            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }
        }

        public class RemoveCarrierFromLoadTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private const string USERNAME = "username";
            private static Guid VALID_LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid VALID_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_LOAD_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private static Guid VALID_CUSTOMER_ID = Guid.Parse("33333333-3333-3333-3333-333333333333");
            private static Guid INVALID_CUSTOMER_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private static RatingQuestionAnswerData RatingQuestionAnswer = new RatingQuestionAnswerData()
            {
                AdditionalComment = "test comment",
                AnswerYN = true
            };

            private LoadEntity VALID_LOAD = new LoadEntity
            {
                LoadId = VALID_LOAD_ID,
                Customer = new CustomerEntity
                {
                    IdentUserId = VALID_CUSTOMER_ID
                },
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)

                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Updated,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(5),
                        LoadTransactionId = VALID_LOAD_TRANSACTION_ID
                    }
                },
                Contacts = new List<LoadContactEntity>
                {
                    new LoadContactEntity()
                    {
                        Email = "fake@1234.com"
                    }
                }
            };

            private LoadClaimEntity VALID_LOAD_CLAIM = new LoadClaimEntity
            {
                CreateDtTm = NOW.AddDays(-1),
                User = new UserEntity
                {
                    Username = "User1234",
                    FirstName = "User",
                    LastName = "1234"
                },
                LoadTransactionId = VALID_LOAD_TRANSACTION_ID
            };

            public RemoveCarrierFromLoadTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder().WithLoadClaims(new List<LoadClaimEntity> { VALID_LOAD_CLAIM }).Build();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });
                _commonService = new Mock<ICommonService>();
                _ratingService = new Mock<IRatingService>();
                _notificationService = new Mock<INotificationService>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithEmptyDb()
            {
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveCarrierFromLoad(VALID_LOAD_ID, USERNAME, RatingQuestionAnswer)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithInvalidLoadId()
            {
                var load = VALID_LOAD;
                load.LoadId = INVALID_LOAD_ID;
                load.CustomerId = Guid.NewGuid();

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                var loadId = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var customerId = Guid.NewGuid();


                _svc.Awaiting(x => x.RemoveCarrierFromLoad(loadId, USERNAME, RatingQuestionAnswer)).Should().Throw<Exception>().WithMessage($"User is not authorized for customer: {load.CustomerId}");
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithInvalidCustomerId()
            {
                var load = VALID_LOAD;

                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>());
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveCarrierFromLoad(VALID_LOAD_ID, USERNAME, RatingQuestionAnswer)).Should().Throw<Exception>().WithMessage("User is not authorized for customer: 00000000-0000-0000-0000-000000000000");

            }

            [Fact]
            public void ShouldThrowLoadMyNotBeRemovedWhenMissingLoadTransactions()
            {
                var load = VALID_LOAD;
                load.LoadTransactions = null;

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveCarrierFromLoad(VALID_LOAD_ID, USERNAME, RatingQuestionAnswer)).Should()
                    .Throw<Exception>()
                    .Where(x => x.Message.StartsWith("Load Carrier may not be removed"));

            }

            [Fact]
            public void ShouldThrowLoadMyNotBeRemovedWhenEmptyLoadTransactions()
            {
                var load = VALID_LOAD;
                load.LoadTransactions = new List<LoadTransactionEntity>();

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveCarrierFromLoad(VALID_LOAD_ID, USERNAME, RatingQuestionAnswer)).Should()
                    .Throw<Exception>()
                    .Where(x => x.Message.StartsWith("Load Carrier may not be removed"));

            }

            [Theory]
            [InlineData(TransactionTypes.Accepted)]
            [InlineData(TransactionTypes.Pending)]
            [InlineData(TransactionTypes.SentToShipperTender)]
            [InlineData(TransactionTypes.Removed)]
            [InlineData(TransactionTypes.PendingRemove)]
            [InlineData(TransactionTypes.PendingRemoveScac)]
            public void ShouldThrowLoadMyNotBeRemovedWhenInvalidTransactionType(string transactionTypeId)
            {
                var load = VALID_LOAD;
                load.LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = transactionTypeId,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    }
                };

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .Build();
                _svc = CreateService();

                _svc.Awaiting(x => x.RemoveCarrierFromLoad(VALID_LOAD_ID, USERNAME, RatingQuestionAnswer)).Should()
                    .Throw<Exception>()
                    .Where(x => x.Message.StartsWith("Load Carrier may not be removed"));

            }

            [Theory]
            [InlineData(TransactionTypes.Pending)]
            [InlineData(TransactionTypes.PreTender)]
            [InlineData(TransactionTypes.Accepted)]
            [InlineData(TransactionTypes.SentToShipperTender)]
            public async Task ShouldRemoveLoadTransactionIfValidState(string transactionTypeId)
            {
                var load = VALID_LOAD;
                var transId = Guid.NewGuid();
                load.LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadTransactionId = transId,
                        TransactionTypeId = transactionTypeId,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    }
                };
                load.LatestTransactionTypeId = transactionTypeId;

                var loadClaims = new List<LoadClaimEntity>()
                {
                    new LoadClaimEntity()
                    {
                        LoadTransactionId = transId
                    }
                };

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithLoadClaims(loadClaims)
                    .Build();

                _ratingService.Setup(x => x.GetRatingQuestion(It.IsAny<Guid>())).ReturnsAsync(new RatingQuestionData());

                _svc = CreateService();

                var response = await _svc.RemoveCarrierFromLoad(VALID_LOAD_ID, USERNAME, RatingQuestionAnswer);
                response.Should().NotBeNull();
                response.OnLoadshop.Should().BeFalse();

                _ratingService.Verify(x => x.AddRatingQuestionAnswer(It.IsAny<RatingQuestionAnswerData>(), false), Times.Once);
                _ratingService.Verify(x => x.GetRatingQuestion(It.IsAny<Guid>()), Times.Once);
                _db.Verify(x => x.SaveChangesAsync(USERNAME, It.IsAny<CancellationToken>()), Times.Once);
            }
            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }

        }

        public class DeleteLoadTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;

            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private readonly Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private const string USERNAME = "username";
            private static Guid VALID_LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_LOAD_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private static Guid VALID_CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_CUSTOMER_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private LoadEntity VALID_LOAD;
            private List<LoadClaimEntity> LOAD_CLAIMS;
            private CustomerEntity CUSTOMER;

            public DeleteLoadTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder().Build();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });
                _commonService = new Mock<ICommonService>();
                _notificationService = new Mock<INotificationService>();
                _ratingService = new Mock<IRatingService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                InitSeedData();
                InitDb();
                InitService();
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoad(VALID_LOAD)
                    .WithLoadClaims(LOAD_CLAIMS)
                    .WithCustomer(CUSTOMER)
                    .Build();
            }

            private void InitService()
            {
                _svc = new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithEmptyDb()
            {
                _db = new MockDbBuilder().Build();
                InitService();

                _svc.Awaiting(x => x.DeleteLoad(VALID_LOAD_ID, USERNAME)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithInvalidLoadId()
            {
                VALID_LOAD.LoadId = INVALID_LOAD_ID;
                VALID_LOAD.CustomerId = Guid.NewGuid();

                var loadId = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var customerId = Guid.NewGuid();

                _svc.Awaiting(x => x.DeleteLoad(loadId, USERNAME))
                    .Should().Throw<Exception>()
                    .WithMessage($"User is not authorized for customer: {VALID_LOAD.CustomerId}");

            }

            [Fact]
            public void ShouldThrowLoadNotFoundWithInvalidCustomerId()
            {
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>());
                InitService();

                _svc.Awaiting(x => x.DeleteLoad(VALID_LOAD_ID, USERNAME)).Should().Throw<Exception>().WithMessage("User is not authorized for customer: 00000000-0000-0000-0000-000000000000");

            }

            [Fact]
            public async Task ShouldAddLoadTransaction()
            {
                VALID_LOAD.LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Removed,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    }
                };

                InitDb();
                InitService();

                var response = await _svc.DeleteLoad(VALID_LOAD_ID, USERNAME);
                response.Should().NotBeNull();
                response.OnLoadshop.Should().BeFalse();
                _db.Verify(x => x.SaveChangesAsync(USERNAME, It.IsAny<CancellationToken>()), Times.Once);
            }


            [Fact]
            public async Task ShouldAdd_RatingAnswer()
            {
                VALID_LOAD.LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadTransactionId = Guid.NewGuid(),
                        TransactionTypeId = TransactionTypes.Removed,
                        CreateDtTm = NOW - TimeSpan.FromHours(1)
                    }
                };
                LOAD_CLAIMS = new List<LoadClaimEntity>()
                {
                    new LoadClaimEntity()
                    {
                        LoadTransactionId = VALID_LOAD.LoadTransactions.First().LoadTransactionId
                    }
                };

                InitDb();
                InitService();

                var ratingQuestion = new RatingQuestionAnswerData()
                {
                    AdditionalComment = "test",
                    AnswerYN = true,
                    RatingQuestionId = Guid.NewGuid()
                };

                var response = await _svc.DeleteLoad(VALID_LOAD_ID, USERNAME, ratingQuestion);
                response.Should().NotBeNull();
                response.OnLoadshop.Should().BeFalse();
                _db.Verify(x => x.SaveChangesAsync(USERNAME, It.IsAny<CancellationToken>()), Times.Once);
                _ratingService.Verify(x => x.AddRatingQuestionAnswer(It.IsAny<RatingQuestionAnswerData>(), false), Times.Once);
            }


            private void InitSeedData()
            {

                CUSTOMER = new CustomerEntity
                {
                    UseFuelRerating = true,
                    FuelReratingNumberOfDays = 5
                };
                VALID_LOAD = new LoadEntity
                {
                    LoadId = VALID_LOAD_ID,
                    Customer = new CustomerEntity
                    {
                        IdentUserId = VALID_CUSTOMER_ID
                    },
                    LoadTransactions = new List<LoadTransactionEntity>
                    {
                        new LoadTransactionEntity
                        {
                            LoadTransactionId = Guid.NewGuid(),
                            TransactionTypeId = TransactionTypes.New,
                            CreateDtTm = NOW - TimeSpan.FromHours(1)
                        },
                        new LoadTransactionEntity
                        {
                            LoadTransactionId = Guid.NewGuid(),
                            TransactionTypeId = TransactionTypes.Updated,
                            CreateDtTm = NOW - TimeSpan.FromMinutes(5)
                        }
                    },
                    PostedLoadCarrierGroups = new List<PostedLoadCarrierGroupEntity>()
                };
                LOAD_CLAIMS = new List<LoadClaimEntity>()
                {
                    new LoadClaimEntity()
                    {
                        LoadTransactionId = VALID_LOAD.LoadTransactions.First().LoadTransactionId
                    }
                };
            }

        }

        public class GetLoadsForHomeTabTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private static Guid LOAD_ID1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_ID2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static Guid LOAD_ID3 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            private static Guid LOAD_ID4 = Guid.Parse("44444444-4444-4444-4444-444444444444");
            private static Guid LOAD_ID5 = Guid.Parse("55555555-5555-5555-5555-555555555555");
            private static Guid LOAD_ID6 = Guid.Parse("66666666-6666-6666-6666-666666666666");
            private static Guid LOAD_ID7 = Guid.Parse("77777777-7777-7777-7777-777777777777");

            private static Guid CUSTOMER_ID1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static CustomerEntity CUSTOMER_1 = new CustomerEntity
            {
                IdentUserId = CUSTOMER_ID1,
                CustomerId = CUSTOMER_ID1
            };
            private static CustomerEntity CUSTOMER_2 = new CustomerEntity
            {
                IdentUserId = CUSTOMER_ID2,
                CustomerId = CUSTOMER_ID2
            };

            private static LoadEntity LOAD_1 = new LoadEntity
            {
                LoadId = LOAD_ID1,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId,
                Stops = 3,
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(2)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Updated,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Removed,
                        CreateDtTm = NOW
                    }
                },
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = TransactionTypes.Removed,
                    CreateDtTm = NOW
                },
                LatestTransactionTypeId = TransactionTypes.Removed,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        LateDtTm = NOW + TimeSpan.FromMinutes(1)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        LateDtTm = NOW + TimeSpan.FromHours(5) + TimeSpan.FromMinutes(1)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 3,
                        LateDtTm = NOW + TimeSpan.FromHours(29) + TimeSpan.FromMinutes(1)
                    }

                }

            };
            private static LoadEntity LOAD_2 = new LoadEntity
            {
                LoadId = LOAD_ID2,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId,
                Stops = 3,
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(2)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Removed,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingAdd,
                        CreateDtTm = NOW
                    }
                },
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = TransactionTypes.PendingAdd,
                    CreateDtTm = NOW
                },
                LatestTransactionTypeId = TransactionTypes.PendingAdd,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        LateDtTm = NOW + TimeSpan.FromMinutes(2)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        LateDtTm = NOW + TimeSpan.FromHours(8) + TimeSpan.FromMinutes(2)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 3,
                        LateDtTm = NOW + TimeSpan.FromHours(12) + TimeSpan.FromMinutes(2)
                    }

                }
            };
            private static LoadEntity LOAD_3 = new LoadEntity
            {
                LoadId = LOAD_ID3,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId,
                Stops = 2,
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(3)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Updated,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(2)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Removed,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingAdd,
                        CreateDtTm = NOW
                    }
                },
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = TransactionTypes.PendingAdd,
                    CreateDtTm = NOW
                },
                LatestTransactionTypeId = TransactionTypes.PendingAdd,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        LateDtTm = NOW + TimeSpan.FromMinutes(3)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        LateDtTm = NOW + TimeSpan.FromHours(4) + TimeSpan.FromMinutes(3)
                    }

                }
            };
            private static LoadEntity LOAD_4 = new LoadEntity
            {
                LoadId = LOAD_ID4,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId,
                Stops = 2,
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingAdd,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(3)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingUpdate,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(2)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingRates,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Posted,
                        CreateDtTm = NOW
                    }
                },
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = TransactionTypes.Posted,
                    CreateDtTm = NOW
                },
                LatestTransactionTypeId = TransactionTypes.Posted,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        LateDtTm = NOW + TimeSpan.FromMinutes(4)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        LateDtTm = NOW + TimeSpan.FromHours(12) + TimeSpan.FromMinutes(4)
                    }

                }
            };
            private static LoadEntity LOAD_5 = new LoadEntity
            {
                LoadId = LOAD_ID5,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId,
                Stops = 2,
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingAdd,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(4)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingUpdate,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(3)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingRates,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(2)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Posted,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW
                    }
                },
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = TransactionTypes.New,
                    CreateDtTm = NOW
                },
                LatestTransactionTypeId = TransactionTypes.New,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        LateDtTm = NOW + TimeSpan.FromMinutes(5)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        LateDtTm = NOW + TimeSpan.FromHours(10) + TimeSpan.FromMinutes(5)
                    }

                }
            };
            private static LoadEntity LOAD_6 = new LoadEntity
            {
                LoadId = LOAD_ID6,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId,
                Stops = 2,
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingAdd,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(5)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingUpdate,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(4)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingRates,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(3)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Posted,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(2)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Updated,
                        CreateDtTm = NOW
                    }
                },
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = TransactionTypes.Updated,
                    CreateDtTm = NOW
                },
                LatestTransactionTypeId = TransactionTypes.Updated,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        LateDtTm = NOW + TimeSpan.FromMinutes(6)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        LateDtTm = NOW + TimeSpan.FromHours(10) + TimeSpan.FromMinutes(6)
                    }

                }
            };
            private static LoadEntity LOAD_7 = new LoadEntity
            {
                LoadId = LOAD_ID7,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId,
                Stops = 2,
                LoadTransactions = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingAdd,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(6)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingUpdate,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(5)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.PendingRates,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(4)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Posted,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(3)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.New,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(2)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Updated,
                        CreateDtTm = NOW - TimeSpan.FromMinutes(1)
                    },
                    new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.Removed,
                        CreateDtTm = NOW
                    }
                },
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = TransactionTypes.Removed,
                    CreateDtTm = NOW
                },
                LatestTransactionTypeId = TransactionTypes.Removed,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        LateDtTm = NOW + TimeSpan.FromMinutes(7)
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        LateDtTm = NOW + TimeSpan.FromHours(10) + TimeSpan.FromMinutes(7)
                    }

                }
            };
            private static UserEntity USER_1 = new UserEntity
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IdentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PrimaryCustomerId = CUSTOMER_ID1
            };
            private static UserEntity USER_2 = new UserEntity
            {
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IdentUserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                PrimaryCustomerId = CUSTOMER_ID2
            };


            public GetLoadsForHomeTabTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder().Build();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });
                _commonService = new Mock<ICommonService>();
                _notificationService = new Mock<INotificationService>();
                _ratingService = new Mock<IRatingService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void CurrentRemovedShouldReturnNoLoads()
            {
                var load = LOAD_1;
                var customers = new List<CustomerEntity>
                {
                    CUSTOMER_1,
                    CUSTOMER_2
                };

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithUser(USER_1)
                    .WithCustomers(customers)
                    .Build();
                _svc = CreateService();

                var response = _svc.GetLoadsForHomeTab(USER_1.IdentUserId);
                response.Should().NotBeNull();
                response.Count.Should().Be(0);
            }

            [Fact]
            public void WrongCustomerShouldReturnNoLoads()
            {
                var load = LOAD_1;
                var customers = new List<CustomerEntity>
                {
                    CUSTOMER_1,
                    CUSTOMER_2
                };

                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithUsers(new List<UserEntity> { USER_1, USER_2 })
                    .WithCustomers(customers)
                    .Build();
                _svc = CreateService();

                var response = _svc.GetLoadsForHomeTab(USER_2.IdentUserId);
                response.Should().NotBeNull();
                response.Count.Should().Be(0);
            }

            [Theory]
            [InlineData(TransactionTypes.Posted)]
            [InlineData(TransactionTypes.PendingRates)]
            [InlineData(TransactionTypes.New)]
            [InlineData(TransactionTypes.Updated)]
            [InlineData(TransactionTypes.PendingAdd)]
            [InlineData(TransactionTypes.PendingUpdate)]
            public void OneValid_OneRemoved_ShouldReturnOneLoad(string transactionTypeId)
            {
                //most current trans is valid for HomeTab
                LOAD_2.LoadTransactions[2].TransactionTypeId = transactionTypeId;

                var loads = new List<LoadEntity>
                {
                    LOAD_1,
                    LOAD_2
                };

                var customers = new List<CustomerEntity>
                {
                    CUSTOMER_1,
                    CUSTOMER_2
                };

                _db = new MockDbBuilder()
                   .WithLoads(loads)
                   .WithUsers(new List<UserEntity> { USER_1, USER_2 })
                   .WithCustomers(customers)
                   .Build();
                _svc = CreateService();

                var response = _svc.GetLoadsForHomeTab(USER_1.IdentUserId);
                response.Should().NotBeNull();
                response.Count.Should().Be(1);
            }

            [Fact]
            public void HomeTabSortTest()
            {
                var loads = new List<LoadEntity>
                {
                    LOAD_7,
                    LOAD_6,
                    LOAD_5,
                    LOAD_4,
                    LOAD_3,
                    LOAD_2,
                    LOAD_1
                };

                var customers = new List<CustomerEntity>
                {
                    CUSTOMER_1,
                    CUSTOMER_2
                };

                _db = new MockDbBuilder()
                   .WithLoads(loads)
                   .WithUser(USER_1)
                   .WithCustomers(customers)
                   .Build();
                _svc = CreateService();

                var response = _svc.GetLoadsForHomeTab(USER_1.IdentUserId);
                response.Should().NotBeNull();
                response.Count.Should().Be(4);
                response[0].LoadId.Should().Be("22222222-2222-2222-2222-222222222222");
                response[1].LoadId.Should().Be("33333333-3333-3333-3333-333333333333");
                response[2].LoadId.Should().Be("55555555-5555-5555-5555-555555555555");
                response[3].LoadId.Should().Be("66666666-6666-6666-6666-666666666666");
            }

            [Fact]
            public void InvalidUserThrowsException()
            {
                var loads = new List<LoadEntity>
                {
                    LOAD_7,
                    LOAD_6,
                    LOAD_5,
                    LOAD_4,
                    LOAD_3,
                    LOAD_2,
                    LOAD_1
                };

                var customers = new List<CustomerEntity>
                {
                    CUSTOMER_1,
                    CUSTOMER_2
                };

                _db = new MockDbBuilder()
                   .WithLoads(loads)
                   .WithUsers(new List<UserEntity> { USER_1, USER_2 })
                   .WithCustomers(customers)
                   .Build();
                _svc = CreateService();

                var invalidUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
                _svc.Invoking(x => x.GetLoadsForHomeTab(invalidUserId)).Should().Throw<Exception>().WithMessage("Unable to determine primary customer ID for IdentUserId*");
            }

            [Fact]
            public void NoLoadsReturnsEmptyList()
            {
                var customers = new List<CustomerEntity>
                {
                    CUSTOMER_1,
                    CUSTOMER_2
                };

                _db = new MockDbBuilder()
                   .WithUser(USER_1)
                   .WithCustomers(customers)
                   .Build();
                _svc = CreateService();

                var response = _svc.GetLoadsForHomeTab(USER_1.IdentUserId);
                response.Should().BeEmpty();
            }

            [Fact]
            public void LoadsWithNoStopsSortBasedOnTodaysDate()
            {
                var load2 = LOAD_2;
                load2.LoadStops = new List<LoadStopEntity>();
                var loads = new List<LoadEntity>
                {
                    LOAD_4,
                    LOAD_3,
                    load2,
                };
                var customers = new List<CustomerEntity>
                {
                    CUSTOMER_1,
                    CUSTOMER_2
                };

                _db = new MockDbBuilder()
                    .WithLoads(loads)
                    .WithUser(USER_1)
                    .WithCustomers(customers)
                    .Build();
                _svc = CreateService();

                var response = _svc.GetLoadsForHomeTab(USER_1.IdentUserId);
                response.Should().NotBeEmpty();
                response[0].LoadId.Should().Be("22222222-2222-2222-2222-222222222222");
                response[1].LoadId.Should().Be("33333333-3333-3333-3333-333333333333");
            }

            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }
        }

        public class LoadAuditLogsTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private static Guid LOAD_ID1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_ID2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private static Guid CUSTOMER_ID1 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            private static Guid CUSTOMER_ID2 = Guid.Parse("44444444-4444-4444-4444-444444444444");

            private static Guid IDENT_USER_ID = Guid.Parse("44444444-4444-4444-4444-444444444445");

            private static long AUDIT_ID1 = 1;
            private static long AUDIT_ID2 = 2;
            private static long AUDIT_ID3 = 3;
            private static long AUDIT_ID4 = 4;
            private static long AUDIT_ID5 = 5;

            private UserEntity USER = new UserEntity()
            {
                PrimaryCustomerId = CUSTOMER_ID1,
                IdentUserId = IDENT_USER_ID
            };

            private static CustomerEntity CUSTOMER_1 = new CustomerEntity
            {
                IdentUserId = CUSTOMER_ID1,
                CustomerId = CUSTOMER_ID1
            };

            private static CustomerEntity CUSTOMER_2 = new CustomerEntity
            {
                IdentUserId = CUSTOMER_ID2,
                CustomerId = CUSTOMER_ID2
            };

            private static LoadEntity LOAD_1 = new LoadEntity
            {
                LoadId = LOAD_ID1,
                Customer = CUSTOMER_1,
                CustomerId = CUSTOMER_1.CustomerId
            };
            private static LoadEntity LOAD_2 = new LoadEntity
            {
                LoadId = LOAD_ID2,
                Customer = CUSTOMER_2,
                CustomerId = CUSTOMER_2.CustomerId
            };

            private static LoadAuditLogEntity AUDIT_1 = new LoadAuditLogEntity { LoadAuditLogId = AUDIT_ID1, LoadId = LOAD_ID1, CreateDtTm = new DateTime(2019, 10, 10) };
            private static LoadAuditLogEntity AUDIT_2 = new LoadAuditLogEntity { LoadAuditLogId = AUDIT_ID2, LoadId = LOAD_ID1, CreateDtTm = new DateTime(2019, 10, 8) };
            private static LoadAuditLogEntity AUDIT_3 = new LoadAuditLogEntity { LoadAuditLogId = AUDIT_ID3, LoadId = LOAD_ID1, CreateDtTm = new DateTime(2019, 10, 9) };
            private static LoadAuditLogEntity AUDIT_4 = new LoadAuditLogEntity { LoadAuditLogId = AUDIT_ID4, LoadId = LOAD_ID1, CreateDtTm = new DateTime(2019, 10, 11) };
            private static LoadAuditLogEntity AUDIT_5 = new LoadAuditLogEntity { LoadAuditLogId = AUDIT_ID5, LoadId = LOAD_ID2, CreateDtTm = new DateTime(2019, 10, 12) };


            public LoadAuditLogsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _userContext.Setup(x => x.UserId).Returns(IDENT_USER_ID);
                _securityService = new Mock<ISecurityService>();
                _ratingService = new Mock<IRatingService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>()
                {
                    new CustomerData()
                    {
                        CustomerId = CUSTOMER_ID1
                    }
                });
                _commonService = new Mock<ICommonService>();
                _notificationService = new Mock<INotificationService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void UserDoesntBelongToCustomerOfRequestedLoad_ShouldThrowNotFoundException()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD_1).WithLoad(LOAD_2)
                    .WithLoadAuditLogs(new List<LoadAuditLogEntity> { AUDIT_1, AUDIT_2, AUDIT_3, AUDIT_4, AUDIT_5 })
                    .Build();
                _svc = CreateService();

                _svc.Invoking(_ => _svc.GetLoadAuditLogs(LOAD_ID1)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void LoadNotFoundForLoadId_ShouldThrowNotFoundException()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD_1)
                    .WithLoadAuditLogs(new List<LoadAuditLogEntity> { AUDIT_1, AUDIT_2, AUDIT_3, AUDIT_4, AUDIT_5 })
                    .Build();
                _svc = CreateService();

                _svc.Invoking(_ => _svc.GetLoadAuditLogs(LOAD_2.LoadId)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void NoLoadsInDbSet_ShouldThrowNotFoundException()
            {
                _db = new MockDbBuilder()
                    .WithLoadAuditLogs(new List<LoadAuditLogEntity> { AUDIT_1, AUDIT_2, AUDIT_3, AUDIT_4, AUDIT_5 })
                    .Build();
                _svc = CreateService();

                _svc.Invoking(_ => _svc.GetLoadAuditLogs(LOAD_2.LoadId)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void NoLoadAuditLogs_ShouldReturnEmptyList()
            {
                _db = new MockDbBuilder()
                    .WithUser(USER)
                    .WithLoad(LOAD_1)
                    .WithLoadAuditLogs(new List<LoadAuditLogEntity>())
                    .Build();
                _svc = CreateService();

                var response = _svc.GetLoadAuditLogs(LOAD_1.LoadId);
                response.Should().NotBeNull();
                response.Should().HaveCount(0);
            }

            [Fact]
            public void LoadAuditLogsExistForLoad_ShouldReturnRecordsInExpectedOrder()
            {
                _db = new MockDbBuilder()
                    .WithUser(USER)
                    .WithLoad(LOAD_1)
                    .WithLoadAuditLogs(new List<LoadAuditLogEntity> { AUDIT_1, AUDIT_2, AUDIT_3, AUDIT_4, AUDIT_5 })
                    .Build();
                _svc = CreateService();

                var response = _svc.GetLoadAuditLogs(LOAD_1.LoadId);
                response.Should().NotBeNull();
                response.Should().HaveCount(4);
                response[0].LoadAuditLogId.Should().Be(AUDIT_ID4);
                response[1].LoadAuditLogId.Should().Be(AUDIT_ID1);
                response[2].LoadAuditLogId.Should().Be(AUDIT_ID3);
                response[3].LoadAuditLogId.Should().Be(AUDIT_ID2);
            }

            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }
        }

        public class PostLoadsTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private MockDbBuilder _builder;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private static Guid CUST = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static Guid IDENT_USER_ID = Guid.Parse("22222222-2222-2222-2222-222222222224");
            private const string USERNAME = "username";
            private static Guid LOAD_ID_1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_ID_2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static Guid LOAD_ID_3 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            private static Guid INVALID_LOAD_ID = Guid.Parse("33333333-3333-3333-3333-333333333333");

            private UserEntity USER = new UserEntity()
            {
                PrimaryCustomerId = CUST,
                IdentUserId = IDENT_USER_ID
            };

            private CustomerApiEntity CUSTOMER_API_1 = new CustomerApiEntity()
            {
                CustomerId = CUST,
                CustomerApiTypeId = "FuelUpdate"
            };

            private List<LoadEntity> LOADS = new List<LoadEntity>
            {
                new LoadEntity
                {
                    CustomerId = CUST,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUST,
                        AllowEditingFuel = true
                    },
                    LoadId = LOAD_ID_1,
                    LoadTransactions = new List<LoadTransactionEntity>
                    {
                        new LoadTransactionEntity
                        {
                            LoadId = LOAD_ID_1,
                            TransactionTypeId = TransactionTypes.PendingAdd,
                            CreateDtTm = NOW
                        }
                    },
                    PostedLoadCarrierGroups = new List<PostedLoadCarrierGroupEntity>(),
                    LoadServiceTypes = new List<LoadServiceTypeEntity>()
                },
                new LoadEntity
                {
                    CustomerId = CUST,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUST,
                        AllowEditingFuel = true
                    },
                    LoadId = LOAD_ID_2,
                    LoadTransactions = new List<LoadTransactionEntity>
                    {
                        new LoadTransactionEntity
                        {
                            LoadId = LOAD_ID_2,
                            TransactionTypeId = TransactionTypes.PendingAdd,
                            CreateDtTm = NOW - TimeSpan.FromHours(2)
                        },
                        new LoadTransactionEntity
                        {
                            LoadId = LOAD_ID_2,
                            TransactionTypeId = TransactionTypes.PendingUpdate,
                            CreateDtTm = NOW
                        }
                    },
                    PostedLoadCarrierGroups = new List<PostedLoadCarrierGroupEntity>(),
                    LoadServiceTypes = new List<LoadServiceTypeEntity>()
                }
            };
            private PostLoadsRequest REQUEST = new PostLoadsRequest
            {
                RequestTime = DateTime.Parse("2019-10-17"),
                CurrentUsername = USERNAME,
                Loads = new List<PostingLoad>
                {
                    new PostingLoad
                    {
                        LoadId = LOAD_ID_1,
                        Comments = "Load comment",
                        Commodity = "Commodity",
                        LineHaulRate = 99.99M,
                        ShippersFSC = 10.00M,
                        CarrierIds = new List<string> { "Carrier1", "Carrier2" },
                        SmartSpotRate = 360.9384m,
                        DATGuardRate = 123m,
                        MachineLearningRate = 456m,
                        CarrierGroupIds = new List<long> { 123 }
                    },
                    new PostingLoad
                    {
                        LoadId = LOAD_ID_2,
                        Comments = "Load comment",
                        Commodity = "Commodity",
                        LineHaulRate = 99.99M,
                        ShippersFSC = 10.00M,
                        CarrierIds = new List<string>(),
                        SmartSpotRate = null,
                        DATGuardRate = null,
                        MachineLearningRate = null,
                        CarrierGroupIds = new List<long> { 123 },
                        ServiceTypeIds = new List<int>() {1,2}
                    }
                }
            };

            private List<ServiceTypeEntity> SERVICE_TYPES = new List<ServiceTypeEntity>()
            {
                new ServiceTypeEntity()
                {
                    ServiceTypeId =1 ,
                    Name = "hazmat"
                },
                new ServiceTypeEntity()
                {
                    ServiceTypeId =2 ,
                    Name = "high value"
                },
            };


            private List<CarrierScacData> ELIGIBLE_SCACS = new List<CarrierScacData>
            {
                new CarrierScacData
                {
                    CarrierId = "Carrier1",
                    Scac = "CAR1",
                    EffectiveDate = DateTime.Parse("2018-01-01"),
                    IsBookingEligible = true,
                    IsActive = true
                },
                new CarrierScacData
                {
                    CarrierId = "Carrier2",
                    Scac = "CAR2",
                    EffectiveDate = DateTime.Parse("2018-01-01"),
                    IsBookingEligible = true,
                    IsActive = true
                },
                new CarrierScacData
                {
                    CarrierId = "Carrier3",
                    Scac = "CAR3",
                    EffectiveDate = DateTime.Parse("2018-01-01"),
                    IsBookingEligible = true,
                    IsActive = true
                }
            };

            public PostLoadsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _userContext.Setup(x => x.UserId).Returns(IDENT_USER_ID);
                _ratingService = new Mock<IRatingService>();
                _builder = new MockDbBuilder();
                _db = _builder
                    .WithUser(USER)
                    .WithLoads(LOADS)
                    .WithServiceTypes(SERVICE_TYPES)
                    .Build();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetCustomerContractedScacsByPrimaryCustomer()).Returns(ELIGIBLE_SCACS);
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>()
                {
                    new CustomerData()
                    {
                        CustomerId = CUST
                    }
                });
                _commonService = new Mock<ICommonService>();
                _notificationService = new Mock<INotificationService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                _svc = CreateService();
            }

            [Fact]
            public void ThrowsOnNullRequest()
            {
                _svc.Awaiting(x => x.PostLoadsAsync(null)).Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void ThrowsOnNoCustomer()
            {
                var request = REQUEST;

                _userContext.Setup(x => x.UserId).Returns(Guid.NewGuid());
                _db = _builder
                    .WithUser(USER)
                    .WithLoads(LOADS)
                    .Build();

                _svc.Awaiting(x => x.PostLoadsAsync(request)).Should().Throw<ArgumentNullException>();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void ThrowsOnNoUsername(string username)
            {
                var request = REQUEST;
                request.CurrentUsername = username;

                _svc.Awaiting(x => x.PostLoadsAsync(request)).Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public async Task ReturnsErrorsOnNoLoads()
            {
                var request = REQUEST;
                request.Loads = new List<PostingLoad>();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Keys.Any(x => x == $"urn:root").Should().BeTrue();
            }

            [Fact]
            public async Task ReturnsErrorsOnAllLoadsNotFound()
            {
                var request = REQUEST;

                _db = new MockDbBuilder()
                    .WithUser(USER)
                    .WithServiceTypes(SERVICE_TYPES)
                    .Build();
                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(2);
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_1}").Should().BeTrue();
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_2}").Should().BeTrue();
            }

            [Fact]
            public async Task ReturnsErrorsOnAllLoadsWithInvalidServiceTypesNotFound()
            {
                var request = REQUEST;

                _db = new MockDbBuilder()
                    .WithUser(USER)
                    .Build();
                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(3);
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_1}").Should().BeTrue();
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_2}").Should().BeTrue();
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_2}:ServiceTypeIds").Should().BeTrue();
            }

            [Fact]
            public async Task ReturnsLoadErrorOnLoadNotFound()
            {
                var request = REQUEST;
                request.Loads.First().LoadId = INVALID_LOAD_ID;

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(1);
                response.ModelState.Keys.Any(x => x == $"urn:load:{INVALID_LOAD_ID}").Should().BeTrue();
            }

            [Theory]
            [InlineData(-1.0)]
            [InlineData(0.0)]
            public async Task ReturnsLoadErrorOnInvalidLineHaulRate(decimal lineHaulRate)
            {
                var request = REQUEST;
                request.Loads.First().LineHaulRate = lineHaulRate;

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(1);
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_1}:LineHaulRate").Should().BeTrue();
            }

            [Theory]
            [InlineData(-1.0)]
            [InlineData(0.0)]
            public async Task ReturnsLoadErrorOnInvalidShippersFSC(decimal shippersFSC)
            {
                var request = REQUEST;
                request.Loads.First().ShippersFSC = shippersFSC;

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(1);
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_1}:ShippersFSC").Should().BeTrue();
            }

            [Theory]
            [InlineData(TransactionTypes.Pending)]
            [InlineData(TransactionTypes.SentToShipperTender)]
            [InlineData(TransactionTypes.PreTender)]
            [InlineData(TransactionTypes.Accepted)]
            public async Task ReturnsLoadErrorOnLoadAlreadyBooked(string transactionType)
            {
                var request = REQUEST;

                var load = LOADS.First(x => x.LoadId == LOAD_ID_1);
                load.LatestTransactionTypeId = transactionType;

                _builder = new MockDbBuilder();
                _db = _builder
                    .WithUser(USER)
                    .WithServiceTypes(SERVICE_TYPES)
                    .WithLoad(load)
                    .Build();

                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(2);

                var errorKey = $"urn:load:{LOAD_ID_1}";
                response.ModelState.Keys.Any(x => x == errorKey).Should().BeTrue();
                response.ModelState[errorKey].Errors.First().ErrorMessage.Should().Be($"Load cannot be posted because it has already been booked. Current status: {transactionType}");
            }

            [Theory]
            [InlineData(TransactionTypes.PendingAdd)]
            [InlineData(TransactionTypes.PendingUpdate)]
            [InlineData(TransactionTypes.Posted)]
            [InlineData(TransactionTypes.PendingFuel)]
            [InlineData(TransactionTypes.PendingRates)]
            [InlineData(TransactionTypes.New)]
            [InlineData(TransactionTypes.Updated)]
            [InlineData(TransactionTypes.PendingRemoveScac)]
            [InlineData(TransactionTypes.PendingRemove)]
            [InlineData(TransactionTypes.Removed)]
            [InlineData(TransactionTypes.Delivered)]
            [InlineData(TransactionTypes.Error)]
            public async Task PostsLoadNotCurrentlyBooked(string transactionType)
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);

                var load = LOADS.First(x => x.LoadId == LOAD_ID_1);
                load.LatestTransactionTypeId = transactionType;

                _builder = new MockDbBuilder();
                _db = _builder
                    .WithUser(USER)
                    .WithLoad(load)
                    .Build();

                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().NotBeEmpty();
            }

            [Fact]
            public async Task SavesSingleLoadWithZeroShippersFSC()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);
                request.Loads.First().ShippersFSC = 0;

                var load = LOADS.First(x => x.LoadId == request.Loads.First().LoadId);
                load.Customer.AllowZeroFuel = true;

                _builder = new MockDbBuilder();
                _db = _builder
                    .WithUser(USER)
                    .WithLoad(load)
                    .Build();

                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().NotBeEmpty();
            }

            [Fact]
            public async Task SaveSingleLoadVerifyPostedCarrierGroup()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);

                var load = LOADS.First(x => x.LoadId == request.Loads.First().LoadId);
                load.PostedLoadCarrierGroups.Clear();

                _builder = new MockDbBuilder();
                _db = _builder
                    .WithUser(USER)
                    .WithLoad(load)
                    .Build();

                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().NotBeEmpty();
                response.PostedLoads.First().CarrierGroupIds.Should().HaveCount(1);
            }

            [Fact]
            public async Task SavesSingleLoadWithZeroShippersFSCAndFuelUpdateApi()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);
                request.Loads.First().ShippersFSC = 0;

                var load = LOADS.First(x => x.LoadId == request.Loads.First().LoadId);
                load.Customer.AllowZeroFuel = false;

                _builder = new MockDbBuilder();
                _db = _builder
                    .WithUser(USER)
                    .WithCustomerApi(CUSTOMER_API_1)
                    .WithLoad(load)
                    .Build();

                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().NotBeEmpty();
            }

            [Fact]
            public async Task ReturnsLoadErrorOnEditingFuelNotAllowed()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);
                request.Loads.First().ShippersFSC = 0;

                var load = LOADS.First(x => x.LoadId == request.Loads.First().LoadId);
                load.Customer.AllowEditingFuel = false;

                _builder = new MockDbBuilder();
                _db = _builder
                    .WithUser(USER)
                    .WithLoad(load)
                    .Build();

                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(1);
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_1}:ShippersFSC").Should().BeTrue();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public async Task ReturnsLoadErrorOnInvalidCommodity(string commodity)
            {
                var request = REQUEST;
                request.Loads.First().Commodity = commodity;

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(1);
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_1}:Commodity").Should().BeTrue();
            }

            [Fact]
            public async Task ReturnsLoadErrorOnEligibleCarrierScacsNotFound()
            {
                var request = REQUEST;

                _securityService.Setup(x => x.GetCustomerContractedScacsByPrimaryCustomer()).Returns(new List<CarrierScacData>());
                _svc = CreateService();

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeFalse();
                response.ModelState.Values.Should().HaveCount(2);
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_1}").Should().BeTrue();
                response.ModelState.Keys.Any(x => x == $"urn:load:{LOAD_ID_2}").Should().BeTrue();

                response.ModelState.Values.Any(x => x.Errors.Any(y => y.ErrorMessage.Contains("The following carriers have no eligible SCACs defined"))).Should().BeTrue();
                response.ModelState.Values.Any(x => x.Errors.Any(y => y.ErrorMessage.Contains("No eligible SCACs found"))).Should().BeTrue();
            }

            [Fact]
            public async Task SavesSingleLoad()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().NotBeEmpty();
            }

            [Fact]
            public async Task SavesSingleLoadWithProvidedCarriers()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().HaveCount(2);
            }

            [Fact]
            public async Task SavesSingleLoadWithNoCarriers()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(0);

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().HaveCount(3);
            }

            [Fact]
            public async Task SavesMultipleLoads()
            {
                var request = REQUEST;

                var response = await _svc.PostLoadsAsync(request);
                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Should().HaveCount(2);
                response.LoadCarrierScacs.Should().NotBeEmpty();
            }

            [Fact]
            public async Task SavesFirstLoadAndReturnsErrorsForOtherLoad()
            {
                var request = REQUEST;
                request.Loads.Last().LoadId = INVALID_LOAD_ID;

                var response = await _svc.PostLoadsAsync(request);

                // Errors and successful posted load Ids and Scacs present on single response
                response.IsSuccess.Should().BeFalse();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().HaveCount(2);
            }

            [Fact]
            public async Task SavesLastLoadAndReturnsErrorsForOtherLoad()
            {
                var request = REQUEST;
                request.Loads.First().LoadId = INVALID_LOAD_ID;

                var response = await _svc.PostLoadsAsync(request);

                // Errors and successful posted load Ids and Scacs present on single response
                response.IsSuccess.Should().BeFalse();
                response.PostedLoads.Should().HaveCount(1);
                response.LoadCarrierScacs.Should().HaveCount(3);
            }

            [Fact]
            public async Task SavesLoadHistoryForSingleLoad()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);

                var response =await  _svc.PostLoadsAsync(request);
                _builder.MockLoadHistories.Verify(_ => _.Add(It.Is<LoadHistoryEntity>(h => h.LoadId == request.Loads[0].LoadId)));
            }

            [Fact]
            public async Task SkipsLoadHistoryWhenLoadUnchanged()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(request.Loads.Count - 1);
                var requestLoad = request.Loads[0];
                var load = LOADS.First(_ => requestLoad.LoadId == _.LoadId);
                load.Comments = requestLoad.Comments;
                load.Commodity = requestLoad.Commodity;
                load.LineHaulRate = requestLoad.LineHaulRate;
                load.FuelRate = requestLoad.ShippersFSC;
                load.SmartSpotRate = requestLoad.SmartSpotRate;
                load.DATGuardRate = requestLoad.DATGuardRate;
                load.MachineLearningRate = requestLoad.MachineLearningRate;
                _db = _builder
                    .WithLoads(LOADS)
                    .Build();

                var response = await _svc.PostLoadsAsync(request);
                _builder.MockLoadHistories.Verify(_ => _.Add(It.IsAny<LoadHistoryEntity>()), Times.Never);
            }

            [Fact]
            public async Task SavesLoadHistoryForMultipleLoads()
            {
                var request = REQUEST;

                var response = await _svc.PostLoadsAsync(request);
                _builder.MockLoadHistories.Verify(_ => _.Add(It.Is<LoadHistoryEntity>(h => h.LoadId == request.Loads[0].LoadId)));
                _builder.MockLoadHistories.Verify(_ => _.Add(It.Is<LoadHistoryEntity>(h => h.LoadId == request.Loads[1].LoadId)));
            }

            [Fact]
            public async Task SavesServiceTypeWithRequest()
            {
                var request = REQUEST;
                request.Loads.RemoveAt(0); // remove 1st 

                var response = await _svc.PostLoadsAsync(request);

                response.IsSuccess.Should().BeTrue();
                response.PostedLoads.Count.Should().Be(1);
                response.PostedLoads.First().ServiceTypes.Count.Should().Be(2);

                _db.Verify(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }
        }

        public class HasFuelUpdateApiTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private MockDbBuilder _builder;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private ShippingService _svc;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private static Guid CUSTOMER_ID1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private CustomerApiEntity CUSTOMER_API_1 = new CustomerApiEntity()
            {
                CustomerId = CUSTOMER_ID1,
                CustomerApiTypeId = "FuelUpdate"
            };

            private CustomerApiEntity CUSTOMER_API_2 = new CustomerApiEntity()
            {
                CustomerId = CUSTOMER_ID2,
                CustomerApiTypeId = "ContractRate"
            };

            public HasFuelUpdateApiTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _securityService = new Mock<ISecurityService>();
                _commonService = new Mock<ICommonService>();
                _notificationService = new Mock<INotificationService>();
                _ratingService = new Mock<IRatingService>();
                _builder = new MockDbBuilder();
                _db = _builder
                    .WithCustomerApis(new List<CustomerApiEntity>()
                    {
                        CUSTOMER_API_1,
                        CUSTOMER_API_2
                    })
                    .Build();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                _svc = CreateService();
            }

            [Fact]
            public void HasFuelUpdateApi_NotFound()
            {
                _svc.HasFuelUpdateApi(Guid.NewGuid()).Should().BeFalse();
            }

            [Fact]
            public void HasFuelUpdateApi_NoFuelUpdateApi()
            {
                _svc.HasFuelUpdateApi(CUSTOMER_ID2).Should().BeFalse();
            }

            [Fact]
            public void HasFuelUpdateApi()
            {
                _svc.HasFuelUpdateApi(CUSTOMER_ID1).Should().BeTrue();
            }
            private ShippingService CreateService()
            {
                return new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }
        }

        public class GetLoadCarrierScacsTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<INotificationService> _notificationService;
            private Mock<IRatingService> _ratingService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;

            private IShippingService _svc;

            private const string STATE_NAME = "state";
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid VALID_LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid INVALID_LOAD_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static Guid VALID_CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string SCAC = "Scac";
            private static readonly string CARRIER_ID = "CarrierId";

            private LoadEntity LOAD = new LoadEntity
            {
                LoadId = VALID_LOAD_ID,
                LineHaulRate = 100M,
                CustomerId = VALID_CUSTOMER_ID,
                Customer = new CustomerEntity
                {
                    CustomerId = VALID_CUSTOMER_ID,
                    IdentUserId = VALID_CUSTOMER_ID
                }
            };
            private UserEntity USER = new UserEntity
            {
                IdentUserId = USER_ID,
                PrimaryCustomerId = VALID_CUSTOMER_ID
            };
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC = new LoadCarrierScacEntity
            {
                LoadId = VALID_LOAD_ID,
                Scac = SCAC,
                ContractRate = 900M
            };
            private CarrierScacEntity CARRIER_SCAC = new CarrierScacEntity
            {
                CarrierId = CARRIER_ID,
                Scac = SCAC,
                IsDedicated = false
            };

            public GetLoadCarrierScacsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(x => x.UserId).Returns(USER_ID);

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser())
                    .Returns(new List<CustomerData>()
                    {
                        new CustomerData
                        {
                            CustomerId = VALID_CUSTOMER_ID
                        }
                    });

                _commonService = new Mock<ICommonService>();
                _commonService.Setup(x => x.GetUSCANStateProvince(It.IsAny<string>())).Returns(new StateData() { Name = STATE_NAME });

                _notificationService = new Mock<INotificationService>();
                _ratingService = new Mock<IRatingService>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _dateTime.SetupGet(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                InitDb();
                InitService();
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithUser(USER)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithCarrierScacs(new List<CarrierScacEntity> { CARRIER_SCAC })
                    .Build();
            }

            private void InitService()
            {
                _svc = new ShippingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _notificationService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities);
            }

            [Fact]
            public void LoadNotFound()
            {
                _db = new MockDbBuilder().Build(); // Empty DB
                InitService();

                _svc.Invoking(x => x.GetLoadCarrierScacs(INVALID_LOAD_ID)).Should()
                    .Throw<Exception>()
                    .WithMessage("Load not found");
            }

            [Fact]
            public void LoadNotAuthorizedForCustomer()
            {
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>());
                InitService();

                _svc.Invoking(x => x.GetLoadCarrierScacs(VALID_LOAD_ID)).Should()
                    .Throw<Exception>()
                    .WithMessage($"User is not authorized for customer: {LOAD.CustomerId}");
            }

            [Fact]
            public void ReturnsLoadCarrierScacs()
            {
                var actual = _svc.GetLoadCarrierScacs(VALID_LOAD_ID);
                actual.Should().NotBeEmpty();
            }

            [Fact]
            public void NonDedicatedScac_ReturnsContractRate()
            {
                var actual = _svc.GetLoadCarrierScacs(VALID_LOAD_ID);
                actual.Should().NotBeEmpty();
                actual.First().ContractRate.Should().Be(LOAD_CARRIER_SCAC.ContractRate);
            }

            [Fact]
            public void DedicatedScac_ReturnsLineHaulRateAsContractRate()
            {
                CARRIER_SCAC.IsDedicated = true;
                InitDb();
                InitService();

                var actual = _svc.GetLoadCarrierScacs(VALID_LOAD_ID);
                actual.Should().NotBeEmpty();
                actual.First().ContractRate.Should().Be(LOAD.LineHaulRate);
            }


        }
    }
}
