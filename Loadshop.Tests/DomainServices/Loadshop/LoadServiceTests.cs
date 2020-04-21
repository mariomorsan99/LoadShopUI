using AutoMapper;
using FluentAssertions;
using Loadshop.Data;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Loadshop.Services.Repositories;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Validation.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMS.Infrastructure.Common.Configuration;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class LoadService_Client_Tests
    {
        public class CreateLoadAuditLogEntryAsyncTests
        {
            [Fact]
            public void DoNotTestStoredProcedureCalls()
            {
                true.Should().BeTrue();
            }
        }

        public class GetAllOpenLoadsTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;
            private Mock<IShippingService> _shippingService;
            private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<ILoadQueryRepository> _loadQueryRepsoitory;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);

            private LoadService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LATEST_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string EQUIPMENT_ID = "EquipmentId";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC = "SCAC";

            private LoadEntity LOAD;
            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private List<LoadStopEntity> LOAD_STOPS;

            public GetAllOpenLoadsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();

                _commonService = new Mock<ICommonService>();
                _commonService.Setup(x => x.GetCarrierVisibilityTypes(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new List<string>
                    {
                        CarrierVisibilityTypes.Project44,
                        CarrierVisibilityTypes.TopsToGo
                    });

                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _shippingService = new Mock<IShippingService>();
                _validationService = new Mock<ILoadValidationService>();
                _config = new Mock<IConfigurationRoot>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);

                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.UserHasRole(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac()).Returns(new List<CarrierScacData>
                {
                    new CarrierScacData
                    {
                        Scac = SCAC,
                    }
                });

                _loadshopFeeService = new Mock<ILoadshopFeeService>();

                _loadQueryRepsoitory = new Mock<ILoadQueryRepository>();

                InitSeedData();
                InitDb();
                InitLoadService();
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadStops(LOAD_STOPS)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .WithCustomer(CUSTOMER)
                    .Build();
            }

            private void InitLoadService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                    _db.Object,
                    _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepsoitory.Object);
            }

            [Fact]
            public void BlockedBySecurityGuard_ThrowsException()
            {
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(false);
                InitLoadService();

                _svc.Invoking(x => x.GetAllOpenLoads(USER_ID))
                    .Should().Throw<UnauthorizedAccessException>()
                    .WithMessage("User does not have one of the specified action*");
            }

            [Fact]
            public void NoLoadCarrierScacs_NonAdmin_NotReturned()
            {
                _securityService
                    .Setup(x => x.UserHasRole(It.IsAny<string[]>()))
                    .Returns(false);
                LOAD.CarrierScacs = new List<LoadCarrierScacEntity>();
                InitDb();
                InitLoadService();

                var result = _svc.GetAllOpenLoads(USER_ID);
                result.Should().BeEmpty();
            }

            [Fact]
            public void HighContractRate_NonAdmin_NotReturned()
            {
                _securityService
                   .Setup(x => x.UserHasRole(It.IsAny<string[]>()))
                   .Returns(false);
                LOAD.CarrierScacs.First().ContractRate = LOAD.LineHaulRate + 1;
                InitDb();
                InitLoadService();

                var result = _svc.GetAllOpenLoads(USER_ID);
                result.Should().BeEmpty();
            }

            [Theory]
            [InlineData(TransactionTypes.New)]
            [InlineData(TransactionTypes.Updated)]
            public void CallsApplyLoadshopFee(string transactionTypeId)
            {
                LOAD.LatestTransactionTypeId = transactionTypeId;
                var expectedRate = 9999.99m;
                var expectedFeeData = new LoadshopFeeData();

                _securityService.Setup(ss => ss.UserHasRole(SecurityRoles.AdminRoles)).Returns(false);
                _loadshopFeeService.Setup(_ => _.ApplyLoadshopFee(It.IsAny<string>(), It.IsAny<ILoadFeeData>(), It.IsAny<IList<CustomerEntity>>()))
                    .Callback((string scac, ILoadFeeData loadFeeData, IList<CustomerEntity> customers) =>
                    {
                        loadFeeData.LineHaulRate = expectedRate;
                        loadFeeData.FeeData = expectedFeeData;
                    });
                _loadQueryRepsoitory.Setup(lqr => lqr.GetLoadsForCarrierMarketplace(It.IsAny<string[]>(), It.IsAny<string>())).Returns(new List<LoadViewData>
                {
                    new LoadViewData()
                    {
                        FeeData = new LoadshopFeeData()
                        {

                        }
                    }
                }.AsQueryable());
                InitDb();
                InitLoadService();

                var result = _svc.GetAllOpenLoads(USER_ID);

                result.Should().HaveCount(1);
                result.First().LineHaulRate.Should().Be(expectedRate);
                result.First().FeeData.Should().Be(expectedFeeData);
                _loadshopFeeService.Verify(_ => _.ApplyLoadshopFee(It.IsAny<string>(), It.IsAny<ILoadFeeData>(), It.IsAny<IList<CustomerEntity>>()), Times.Once);
            }

            private void InitSeedData()
            {

                VALID_USER = new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID,
                    PrimaryCustomerId = CUSTOMER_ID,
                    PrimaryScac = SCAC,
                    PrimaryScacEntity = new CarrierScacEntity
                    {
                        CarrierId = CARRIER_ID
                    }
                };
                LOAD_CARRIER_SCAC = new LoadCarrierScacEntity
                {
                    LoadCarrierScacId = LOAD_CARRIER_SCAC_ID,
                    LoadId = LOAD_ID,
                    Scac = SCAC,
                    ContractRate = 9.99M,
                };
                LOAD_STOPS = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 1,
                    },
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 2
                    }
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 100M,
                    ManuallyCreated = false,
                    Contacts = new List<LoadContactEntity>
                    {
                        new LoadContactEntity
                        {
                            LoadContactId = Guid.NewGuid(),
                            LoadId = LOAD_ID,
                            Phone = "123-123-1234",
                            Email = "test@email.com"
                        }
                    },
                    Stops = 2,
                    EquipmentId = EQUIPMENT_ID,
                    Equipment = new EquipmentEntity
                    {
                        EquipmentId = EQUIPMENT_ID,
                        EquipmentDesc = "Equipment",
                    },
                    LatestLoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                    LatestTransactionTypeId = TransactionTypes.New,
                    CarrierScacs = new List<LoadCarrierScacEntity>()
                    {
                        LOAD_CARRIER_SCAC
                    }
                };
                CUSTOMER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID
                };
            }
        }

        public class GetBookedLoadsTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;
            private Mock<IShippingService> _shippingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;

            private LoadService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LATEST_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string EQUIPMENT_ID = "EquipmentId";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC = "SCAC";

            private LoadEntity LOAD;
            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private List<LoadStopEntity> LOAD_STOPS;
            private List<LoadTransactionEntity> LOAD_TRANSACTIONS;
            private List<LoadClaimEntity> LOAD_CLAIMS;
            private List<CarrierScacEntity> CARRIER_SCACS;
            private List<CarrierEntity> CARRIERS;
            private List<CustomerCarrierScacContractEntity> CUST_CARRIER_SCAC_CONTRACTS;

            public GetBookedLoadsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
                _commonService = new Mock<ICommonService>();

                _commonService.Setup(x => x.GetCarrierVisibilityTypes(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new List<string>
                    {
                        CarrierVisibilityTypes.Project44,
                        CarrierVisibilityTypes.TopsToGo
                    });

                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _shippingService = new Mock<IShippingService>();
                _validationService = new Mock<ILoadValidationService>();
                _config = new Mock<IConfigurationRoot>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);

                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.UserHasRole(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac()).Returns(new List<CarrierScacData>
                {
                    new CarrierScacData
                    {
                        Scac = SCAC,
                    }
                });
                _loadshopFeeService = new Mock<ILoadshopFeeService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                InitSeedData();
                InitDb();
                InitLoadService();
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadStops(LOAD_STOPS)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithLoadTransactions(LOAD_TRANSACTIONS)
                    .WithLoadClaims(LOAD_CLAIMS)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .WithCarriers(CARRIERS)
                    .WithCustomerCarrierScacContracts(CUST_CARRIER_SCAC_CONTRACTS)
                    .WithUser(VALID_USER)
                    .WithCustomer(CUSTOMER)
                    .Build();
            }

            private void InitLoadService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                    _db.Object,
                    _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepository.Object);
            }

            [Fact]
            public void BlockedBySecurityGuard_ThrowsException()
            {
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(false);
                InitLoadService();

                _svc.Invoking(x => x.GetBookedLoads(USER_ID))
                    .Should().Throw<UnauthorizedAccessException>()
                    .WithMessage("User does not have one of the specified action*");
            }



            [Fact]
            public void NoLoadClaims_NotReturned()
            {
                LOAD_CLAIMS = new List<LoadClaimEntity>();
                InitDb();
                InitLoadService();

                var result = _svc.GetBookedLoads(USER_ID);
                result.Should().BeEmpty();
            }

            [Fact]
            public void UnauthorizedScac_NotReturned()
            {
                _securityService.Reset();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.UserHasRole(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac())
                    .Returns(new List<CarrierScacData>());
                InitLoadService();

                var result = _svc.GetBookedLoads(USER_ID);
                result.Should().BeEmpty();
            }

            [Theory]
            [InlineData(TransactionTypes.Accepted)]
            [InlineData(TransactionTypes.Pending)]
            [InlineData(TransactionTypes.SentToShipperTender)]
            [InlineData(TransactionTypes.PreTender)]
            public void ShouldNotCallApplyLoadshopFee(string transactionTypeId)
            {
                LOAD.LatestTransactionTypeId = transactionTypeId;
                LOAD_TRANSACTIONS.First().TransactionTypeId = transactionTypeId;

                _securityService.Setup(ss => ss.UserHasRole(SecurityRoles.AdminRoles)).Returns(false);
                _loadQueryRepository.Setup(lqr => lqr.GetLoadsForCarrierWithLoadClaim(It.IsAny<string[]>(), It.IsAny<string[]>())).Returns(new List<LoadViewData>()
                {
                    new LoadViewData()
                }
                .AsQueryable());
                InitDb();
                InitLoadService();

                var result = _svc.GetBookedLoads(USER_ID);
                result.Should().HaveCount(1);

                _loadshopFeeService.Verify(_ => _.ApplyLoadshopFee(It.IsAny<string>(), It.IsAny<ILoadFeeData>(), It.IsAny<IList<CustomerEntity>>()), Times.Never);
            }

            private void InitSeedData()
            {

                VALID_USER = new UserEntity
                {
                    UserId = USER_ID,
                    Username = "username",
                    IdentUserId = USER_ID,
                    PrimaryCustomerId = CUSTOMER_ID,
                    PrimaryScac = SCAC,
                    UserNotifications = new List<UserNotificationEntity>
                    {
                        new UserNotificationEntity
                        {
                            UserId = USER_ID,
                            IsDefault = true,
                            NotificationEnabled = true,
                            NotificationValue = "Notification",
                        }
                    }
                };
                LOAD_CARRIER_SCAC = new LoadCarrierScacEntity
                {
                    LoadCarrierScacId = LOAD_CARRIER_SCAC_ID,
                    LoadId = LOAD_ID,
                    Scac = SCAC,
                    ContractRate = 9.99M,
                };
                LOAD_STOPS = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 1,
                    },
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 2
                    }
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        LoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                        TransactionTypeId = TransactionTypes.Accepted,
                    }
                };
                LOAD_CLAIMS = new List<LoadClaimEntity>
                {
                    new LoadClaimEntity()
                    {
                        LoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                        Scac = SCAC,
                        UserId = USER_ID,
                        CreateDtTm = NOW,
                        BillingLoadId = "BillingLoadId",
                        BillingLoadDisplay = "BillingLoadDisplay",
                        VisibilityPhoneNumber = "123-123-1234",
                        VisibilityTruckNumber = "TRK-1234",
                        MobileExternallyEntered = false
                    }
                };
                CARRIER_SCACS = new List<CarrierScacEntity>
                {
                    new CarrierScacEntity
                    {
                        Scac = SCAC,
                        CarrierId = CARRIER_ID
                    }
                };
                CARRIERS = new List<CarrierEntity>
                {
                    new CarrierEntity()
                    {
                        CarrierId = CARRIER_ID,
                        CarrierName = "123 Carrier"
                    }
                };
                CUST_CARRIER_SCAC_CONTRACTS = new List<CustomerCarrierScacContractEntity>
                {
                    new CustomerCarrierScacContractEntity
                    {
                        Scac = SCAC,
                        CustomerId = CUSTOMER_ID
                    }
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 100M,
                    ManuallyCreated = false,
                    Contacts = new List<LoadContactEntity>
                    {
                        new LoadContactEntity
                        {
                            LoadContactId = Guid.NewGuid(),
                            LoadId = LOAD_ID,
                            Phone = "123-123-1234",
                            Email = "test@email.com"
                        }
                    },
                    Stops = 2,
                    EquipmentId = EQUIPMENT_ID,
                    Equipment = new EquipmentEntity
                    {
                        EquipmentId = EQUIPMENT_ID,
                        EquipmentDesc = "Equipment",
                    },
                    LatestLoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                    LatestTransactionTypeId = TransactionTypes.Accepted,
                };
                CUSTOMER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID
                };
            }
        }

        public class GetDeliveredLoadsTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;
            private Mock<IShippingService> _shippingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;

            private LoadService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LATEST_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string EQUIPMENT_ID = "EquipmentId";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC = "SCAC";

            private LoadEntity LOAD;
            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private List<LoadStopEntity> LOAD_STOPS;
            private List<LoadTransactionEntity> LOAD_TRANSACTIONS;
            private List<LoadClaimEntity> LOAD_CLAIMS;
            private List<CarrierScacEntity> CARRIER_SCACS;
            private List<CarrierEntity> CARRIERS;
            private List<CustomerCarrierScacContractEntity> CUST_CARRIER_SCAC_CONTRACTS;

            public GetDeliveredLoadsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
                _commonService = new Mock<ICommonService>();

                _commonService.Setup(x => x.GetCarrierVisibilityTypes(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new List<string>
                    {
                        CarrierVisibilityTypes.Project44,
                        CarrierVisibilityTypes.TopsToGo
                    });

                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _shippingService = new Mock<IShippingService>();
                _validationService = new Mock<ILoadValidationService>();
                _config = new Mock<IConfigurationRoot>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);

                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.UserHasRole(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac()).Returns(new List<CarrierScacData>
                {
                    new CarrierScacData
                    {
                        Scac = SCAC,
                    }
                });

                _loadshopFeeService = new Mock<ILoadshopFeeService>();

                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                InitSeedData();
                InitDb();
                InitLoadService();
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadStops(LOAD_STOPS)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithLoadTransactions(LOAD_TRANSACTIONS)
                    .WithLoadClaims(LOAD_CLAIMS)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .WithCarriers(CARRIERS)
                    .WithCustomerCarrierScacContracts(CUST_CARRIER_SCAC_CONTRACTS)
                    .WithUser(VALID_USER)
                    .WithCustomer(CUSTOMER)
                    .Build();
            }

            private void InitLoadService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                    _db.Object,
                    _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepository.Object);
            }

            [Fact]
            public void BlockedBySecurityGuard_ThrowsException()
            {
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(false);
                InitLoadService();

                _svc.Invoking(x => x.GetDeliveredLoads(USER_ID))
                    .Should().Throw<UnauthorizedAccessException>()
                    .WithMessage("User does not have one of the specified action*");
            }

            [Fact]
            public void NoLoadClaims_NotReturned()
            {
                LOAD_CLAIMS = new List<LoadClaimEntity>();
                InitDb();
                InitLoadService();

                var result = _svc.GetDeliveredLoads(USER_ID);
                result.Should().BeEmpty();
            }

            [Fact]
            public void UnauthorizedScac_NotReturned()
            {
                _securityService.Reset();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.UserHasRole(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac())
                    .Returns(new List<CarrierScacData>());
                InitLoadService();

                var result = _svc.GetDeliveredLoads(USER_ID);
                result.Should().BeEmpty();
            }


            [Fact]
            public void ShouldNotCallApplyLoadshopFee()
            {
                _loadQueryRepository.Setup(lqr => lqr.GetLoadsForCarrierWithLoadClaim(It.IsAny<string[]>(), It.IsAny<string[]>())).Returns(new List<LoadViewData>()
                {
                    new LoadViewData()
                }
               .AsQueryable());

                var result = _svc.GetDeliveredLoads(USER_ID);
                result.Should().HaveCount(1);

                _loadshopFeeService.Verify(_ => _.ApplyLoadshopFee(It.IsAny<string>(), It.IsAny<ILoadFeeData>(), It.IsAny<IList<CustomerEntity>>()), Times.Never);
            }

            private void InitSeedData()
            {

                VALID_USER = new UserEntity
                {
                    UserId = USER_ID,
                    Username = "username",
                    IdentUserId = USER_ID,
                    PrimaryCustomerId = CUSTOMER_ID,
                    PrimaryScac = SCAC,
                    UserNotifications = new List<UserNotificationEntity>
                    {
                        new UserNotificationEntity
                        {
                            UserId = USER_ID,
                            IsDefault = true,
                            NotificationEnabled = true,
                            NotificationValue = "Notification",
                        }
                    }
                };
                LOAD_CARRIER_SCAC = new LoadCarrierScacEntity
                {
                    LoadCarrierScacId = LOAD_CARRIER_SCAC_ID,
                    LoadId = LOAD_ID,
                    Scac = SCAC,
                    ContractRate = 9.99M,
                };
                LOAD_STOPS = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 1,
                    },
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 2
                    }
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        LoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                        TransactionTypeId = TransactionTypes.Delivered,
                    }
                };
                LOAD_CLAIMS = new List<LoadClaimEntity>
                {
                    new LoadClaimEntity()
                    {
                        LoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                        Scac = SCAC,
                        UserId = USER_ID,
                        CreateDtTm = NOW,
                        BillingLoadId = "BillingLoadId",
                        BillingLoadDisplay = "BillingLoadDisplay",
                        VisibilityPhoneNumber = "123-123-1234",
                        VisibilityTruckNumber = "TRK-1234",
                        MobileExternallyEntered = false
                    }
                };
                CARRIER_SCACS = new List<CarrierScacEntity>
                {
                    new CarrierScacEntity
                    {
                        Scac = SCAC,
                        CarrierId = CARRIER_ID
                    }
                };
                CARRIERS = new List<CarrierEntity>
                {
                    new CarrierEntity()
                    {
                        CarrierId = CARRIER_ID,
                        CarrierName = "123 Carrier"
                    }
                };
                CUST_CARRIER_SCAC_CONTRACTS = new List<CustomerCarrierScacContractEntity>
                {
                    new CustomerCarrierScacContractEntity
                    {
                        Scac = SCAC,
                        CustomerId = CUSTOMER_ID
                    }
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 100M,
                    ManuallyCreated = false,
                    Contacts = new List<LoadContactEntity>
                    {
                        new LoadContactEntity
                        {
                            LoadContactId = Guid.NewGuid(),
                            LoadId = LOAD_ID,
                            Phone = "123-123-1234",
                            Email = "test@email.com"
                        }
                    },
                    Stops = 2,
                    EquipmentId = EQUIPMENT_ID,
                    Equipment = new EquipmentEntity
                    {
                        EquipmentId = EQUIPMENT_ID,
                        EquipmentDesc = "Equipment",
                    },
                    LatestLoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                    LatestTransactionTypeId = TransactionTypes.Delivered,
                };
                CUSTOMER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID
                };
            }
        }

        public class GetLoadByIdTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;
            public Mock<IShippingService> _shippingService;
            public Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;

            private LoadService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private Guid BOOKED_USER_ID = Guid.Parse("11111111-2222-1111-1111-111111111111");
            private static readonly string SCAC = "SCAC";
            private static readonly string BOOKED_CARRIER = "BookedCarrier";

            private LoadDetailViewEntity LOAD_DETAIL_VIEW;
            private LoadEntity LOAD;
            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private UserEntity BOOKED_USER;
            private List<ServiceTypeEntity> SERVICE_TYPES;
            private List<LoadServiceTypeEntity> LOAD_SERVICE_TYPES;

            public GetLoadByIdTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
                _commonService = new Mock<ICommonService>();
                _config = new Mock<IConfigurationRoot>();
                _validationService = new Mock<ILoadValidationService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _shippingService = new Mock<IShippingService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _securityService.Setup(x => x.GetAllMyAuthorizedEntities()).Returns(new List<UserFocusEntityData>
                {
                    new UserFocusEntityData
                    {
                        Id = CUSTOMER_ID.ToString("D"),
                        Name = "Blah",
                        Group = "Blah",
                        Type = UserFocusEntityType.Shipper
                    }
                });

                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
                _loadshopFeeService = new Mock<ILoadshopFeeService>();

                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                InitSeedData();
                InitDb();
                InitLoadService();
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoadDetailViews(new List<LoadDetailViewEntity> { LOAD_DETAIL_VIEW })
                    .WithLoad(LOAD)
                    .WithUser(VALID_USER)
                    .WithUser(BOOKED_USER)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithServiceTypes(SERVICE_TYPES)
                    .WithLoadServiceTypes(LOAD_SERVICE_TYPES)
                    .WithCustomer(CUSTOMER)
                    .Build();

                _loadQueryRepository.Setup(lqr => lqr.GetLoadDetailViews(It.IsAny<GetLoadDetailOptions>()))
                    .Returns(new List<LoadDetailViewEntity> { LOAD_DETAIL_VIEW });
            }

            private void InitLoadService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                    _db.Object,
                    _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepository.Object);
            }

            [Fact]
            public void ThrowsUnauthorizedWhenSecurityServiceSaysSo()
            {
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(false);
                InitLoadService();
                _svc.Awaiting(x => x.GetLoadByIdAsync(LOAD_ID, USER_ID)).Should()
                    .Throw<UnauthorizedAccessException>();
            }

            [Fact]
            public void ThrowsExceptionWhenLoadViewDetailNotFound()
            {
                _db = new MockDbBuilder().Build();

                _loadQueryRepository.Setup(lqr => lqr.GetLoadDetailViews(It.IsAny<GetLoadDetailOptions>()))
                    .Returns(new List<LoadDetailViewEntity>());
                InitLoadService();

                _svc.Awaiting(x => x.GetLoadByIdAsync(LOAD_ID, USER_ID)).Should()
                    .Throw<Exception>()
                    .WithMessage($"Load {LOAD_ID} not found");
            }

            [Fact]
            public async Task ReturnsLoadWhenUserIsShipperWithAccessToLoad()
            {
                _securityService.Setup(x => x.GetAllMyAuthorizedEntities()).Returns(new List<UserFocusEntityData>
                {
                    new UserFocusEntityData
                    {
                        Id = CUSTOMER_ID.ToString("D"),
                        Name = "Blah",
                        Group = "Blah",
                        Type = UserFocusEntityType.Shipper
                    }
                });
                InitLoadService();
                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);
                load.Should().NotBeNull();
                load.LoadId.Should().Be(LOAD_ID);
            }

            [Fact]
            public async Task ReturnsLoadWhenUserHasScacAccessToLoad()
            {
                _securityService.Setup(x => x.GetAllMyAuthorizedEntities()).Returns(new List<UserFocusEntityData>
                {
                    new UserFocusEntityData
                    {
                        Id = SCAC,
                        Name = "Blah",
                        Group = "Blah",
                        Type = UserFocusEntityType.CarrierScac
                    }
                });
                InitLoadService();
                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);
                load.Should().NotBeNull();
                load.LoadId.Should().Be(LOAD_ID);
            }

            [Fact]
            public async Task LineHaulRateShouldMatchUsersContractRate()
            {
                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);
                load.Should().NotBeNull();
                load.LoadId.Should().Be(LOAD_ID);
                load.LineHaulRate.Should().Be(LOAD_CARRIER_SCAC.ContractRate);
            }

            [Fact]
            public async Task BookedUserReturnedIfUsersPrimaryScacCarrierMatchesBookedCarrier()
            {
                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);
                load.Should().NotBeNull();
                load.BookedUser.Should().NotBeNull();
                load.BookedUser.UserName.Should().NotBeNullOrWhiteSpace();
                load.BookedUser.UserName.Should().Be(LOAD_DETAIL_VIEW.BookedUser);
                load.BookedUser.Email.Should().Be("test@test.com");
                load.BookedUser.CellPhoneNumbers.Count().Should().Be(1);
                load.BookedUser.PhoneNumbers.Count().Should().Be(1);
            }

            [Fact]
            public async Task BookedUserShouldBePopulatedIfUserCarrierDoesNotMatchBookedCarrier()
            {
                LOAD_DETAIL_VIEW.BookedUserCarrierName = "does not match";
                InitDb();
                InitLoadService();

                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);
                load.Should().NotBeNull();
                load.BookedUser.Should().NotBeNull();
            }

            [Fact]
            public async Task ServiceTypesShouldNotBeAddedToComments()
            {
                var expected = string.Join(", ", SERVICE_TYPES.Select(x => x.Name).ToList());
                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);
                load.Comments.Should().BeNull();
                load.ServiceTypes.Count.Should().Be(2);
            }

            [Fact]
            public async Task ShouldApplyLoadshopFees()
            {
                var expectedRate = 9999.99m;
                var expectedFeeData = new LoadshopFeeData();
                _loadshopFeeService.Setup(_ => _.ApplyLoadshopFee(It.IsAny<string>(), It.IsAny<ILoadFeeData>(), It.IsAny<IList<CustomerEntity>>()))
                    .Callback((string scac, ILoadFeeData loadFeeData, IList<CustomerEntity> customers) =>
                    {
                        loadFeeData.LineHaulRate = expectedRate;
                        loadFeeData.FeeData = expectedFeeData;
                    });
                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);

                load.LineHaulRate.Should().Be(expectedRate);
                load.FeeData.Should().Be(expectedFeeData);
            }

            [Theory]
            [InlineData(TransactionTypes.New)]
            [InlineData(TransactionTypes.Updated)]
            public async Task ShouldCallApplyLoadshopFee(string transactionType)
            {
                LOAD_DETAIL_VIEW.TransactionTypeId = transactionType;
                InitDb();
                InitLoadService();

                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);

                _loadshopFeeService.Verify(_ => _.ApplyLoadshopFee(It.IsAny<string>(), It.IsAny<ILoadFeeData>(), It.IsAny<IList<CustomerEntity>>()), Times.Once);
            }

            [Theory]
            [InlineData(TransactionTypes.Accepted)]
            [InlineData(TransactionTypes.PreTender)]
            [InlineData(TransactionTypes.Pending)]
            [InlineData(TransactionTypes.Delivered)]
            public async Task ShouldNotCallApplyLoadshopFee(string transactionType)
            {
                LOAD_DETAIL_VIEW.TransactionTypeId = transactionType;
                InitDb();
                InitLoadService();

                var load = await _svc.GetLoadByIdAsync(LOAD_ID, USER_ID);

                _loadshopFeeService.Verify(_ => _.ApplyLoadshopFee(It.IsAny<string>(), It.IsAny<ILoadFeeData>(), It.IsAny<IList<CustomerEntity>>()), Times.Never);
            }



            private void InitSeedData()
            {
                LOAD_SERVICE_TYPES = new List<LoadServiceTypeEntity>
                {
                    new LoadServiceTypeEntity
                    {
                        LoadId = LOAD_ID,
                        ServiceTypeId = 1
                    },
                    new LoadServiceTypeEntity
                    {
                        LoadId = LOAD_ID,
                        ServiceTypeId = 2
                    }
                };
                LOAD_DETAIL_VIEW = new LoadDetailViewEntity
                {
                    LoadId = LOAD_ID,
                    BookedUserId = BOOKED_USER_ID,
                    BookedUser = "BookedUsers",
                    BookedUserCarrierName = BOOKED_CARRIER,
                    LoadServiceTypes = LOAD_SERVICE_TYPES
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 100M,
                    ManuallyCreated = false,
                    Contacts = new List<LoadContactEntity>
                    {
                        new LoadContactEntity
                        {
                            LoadContactId = Guid.NewGuid(),
                            LoadId = LOAD_ID,
                            Phone = "123-123-1234",
                            Email = "test@email.com"
                        }
                    },
                    LoadStops = new List<LoadStopEntity>
                    {
                        new LoadStopEntity
                        {
                            LoadId = LOAD_ID,
                            StopNbr = 1,
                        },
                        new LoadStopEntity
                        {
                            LoadId = LOAD_ID,
                            StopNbr = 2
                        }
                    },
                };
                LOAD_CARRIER_SCAC = new LoadCarrierScacEntity
                {
                    LoadCarrierScacId = LOAD_CARRIER_SCAC_ID,
                    LoadId = LOAD_ID,
                    Scac = SCAC,
                    ContractRate = 9.99M,
                    CarrierScac = new CarrierScacEntity()
                    {
                        Scac = SCAC
                    }
                };
                VALID_USER = new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID,
                    PrimaryCustomerId = CUSTOMER_ID,
                    PrimaryScac = LOAD_CARRIER_SCAC.Scac,
                    PrimaryScacEntity = new CarrierScacEntity
                    {
                        CarrierId = BOOKED_CARRIER
                    },
                    UserNotifications = new List<UserNotificationEntity>
                    {
                        new UserNotificationEntity
                        {
                            UserId = USER_ID,
                            IsDefault = true,
                            NotificationEnabled = true,
                            NotificationValue = "Notification",
                        }
                    }
                };
                BOOKED_USER = new UserEntity
                {
                    UserId = BOOKED_USER_ID,
                    FirstName = "test",
                    IdentUserId = BOOKED_USER_ID,
                    PrimaryCustomerId = CUSTOMER_ID,
                    PrimaryScac = LOAD_CARRIER_SCAC.Scac,
                    PrimaryScacEntity = new CarrierScacEntity
                    {
                        CarrierId = BOOKED_CARRIER
                    },
                    UserNotifications = new List<UserNotificationEntity>
                    {
                        new UserNotificationEntity
                        {
                            UserId = BOOKED_USER_ID,
                            IsDefault = true,
                            NotificationEnabled = true,
                            NotificationValue = "1111111111",
                            MessageTypeId = MessageTypeConstants.Phone
                        },
                        new UserNotificationEntity
                        {
                            UserId = BOOKED_USER_ID,
                            IsDefault = true,
                            NotificationEnabled = true,
                            NotificationValue = "test@test.com",
                            MessageTypeId = MessageTypeConstants.Email
                        },
                        new UserNotificationEntity
                        {
                            UserId = BOOKED_USER_ID,
                            IsDefault = true,
                            NotificationEnabled = true,
                            NotificationValue = "2222222222",
                            MessageTypeId = MessageTypeConstants.CellPhone
                        }
                    }
                };
                SERVICE_TYPES = new List<ServiceTypeEntity>
                {
                    new ServiceTypeEntity
                    {
                        ServiceTypeId = 1,
                        Name = "Service 1",
                    },
                    new ServiceTypeEntity
                    {
                        ServiceTypeId = 2,
                        Name = "Service 2",
                    }
                };
                CUSTOMER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID
                };
            }
        }

        public class PendingAcceptLoadTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;

            private Mock<ILoadQueryRepository> _loadQueryRepository;
            //private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;

            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;

            private LoadService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LATEST_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static string EQUIPMENT_ID = "EquipmentId";
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static string SCAC = "SCAC";

            private LoadEntity LOAD;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;

            private LoadData LOAD_DATA;

            public PendingAcceptLoadTests(TestFixture fixture)
            {
                InitSeedData();

                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();

                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
                _commonService = new Mock<ICommonService>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                _config = new Mock<IConfigurationRoot>();

                _validationService = new Mock<ILoadValidationService>();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

                _loadshopFeeService = new Mock<ILoadshopFeeService>();

                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                InitLoadService();
            }

            private void InitLoadService()
            {
                _svc = new LoadService(
                    new Mock<ILogger<LoadService>>().Object,
                    _db.Object, _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepository.Object);
            }

            [Fact]
            public void MissingRequiredSecurityAction_ThrowsException()
            {
                _securityService.Reset();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(false);
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<UnauthorizedAccessException>();
            }

            [Fact]
            public void LoadNotFound_ThrowsException()
            {
                LOAD_DATA.LoadId = Guid.Empty;
                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().WithMessage("Load not found");
            }

            [Fact]
            public void InvalidUserId_ThrowsException()
            {
                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, Guid.Empty);
                action.Should().Throw<ValidationException>().WithMessage("Invalid User");
            }

            [Fact]
            public void UsersPrimaryScacNotAllowedToBookLoad_ThrowsException()
            {
                var invalidScac = "BAD";
                VALID_USER.PrimaryScac = invalidScac;
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().WithMessage($"{invalidScac} is not allowed to book this load");
            }

            [Fact]
            public void UsersPrimaryScacContractRateGreaterThanLineHaulRate_ThrowsException()
            {
                LOAD_CARRIER_SCAC.ContractRate = LOAD_DATA.LineHaulRate + 1;
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().WithMessage($"{LOAD_CARRIER_SCAC.Scac} is not allowed to book this load");
            }

            [Fact]
            public void LoadIsNotNewOrUpdated_CannotBeBooked_ThrowsException()
            {
                LOAD.LatestLoadTransaction.TransactionTypeId = "Random";
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().WithMessage("This load is not eligible for booking at this time");
            }

            [Fact]
            public void NoLatestTransaction_CannotBeBooked_ThrowsException()
            {
                LOAD.LatestLoadTransaction = null;
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().WithMessage("This load is not eligible for booking at this time");
            }

            [Fact]
            public void PendingEmailSent()
            {
                _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                _notifyService.Verify(x => x.SendPendingEmail(LOAD, VALID_USER, LOAD.Contacts.First(), It.IsAny<string>(), It.IsAny<LoadClaimEntity>()), Times.Once);
            }

            [Fact]
            public void PendingLoadTransactionIsAdded()
            {
                var expected = new LoadTransactionEntity
                {
                    LoadId = LOAD_ID,
                    TransactionTypeId = TransactionTypes.Pending,
                    Claim = new LoadClaimEntity()
                    {
                        LineHaulRate = LOAD_CARRIER_SCAC.ContractRate.Value,
                        FuelRate = LOAD.FuelRate,
                        Scac = VALID_USER.PrimaryScac,
                        UserId = VALID_USER.UserId,
                    }
                };

                var builder = new MockDbBuilder();
                _db = builder
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();
                LoadTransactionEntity added = null;
                builder.MockLoadTransactions.Setup(_ => _.Add(It.IsAny<LoadTransactionEntity>())).Callback((LoadTransactionEntity _) => { added = _; });
                InitLoadService();

                _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                added.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void LoadHistoryIsAdded()
            {
                var builder = new MockDbBuilder();
                _db = builder
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                builder.MockLoadHistories.Verify(x => x.Add(It.IsAny<LoadHistoryEntity>()), Times.Once);
            }

            [Fact]
            public void SaveChangesIsCalled()
            {
                var savingUserName = "username";
                _userContext.SetupGet(ctx => ctx.UserName).Returns(savingUserName);
                InitLoadService();

                _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                _db.Verify(x => x.SaveChanges(savingUserName), Times.Once);
            }

            [Fact]
            public void ReturnsSameLoadDataItWasCalledWith()
            {
                var actual = _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                actual.Should().BeEquivalentTo(LOAD_DATA);
            }

            [Fact]
            public void IsScacAllowedToBookLoad_ScacNotFound()
            {
                var user = VALID_USER;
                user.PrimaryScac = user.PrimaryScac + "invalid";
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().Where(x => x.Message.EndsWith("is not allowed to book this load"));
            }

            [Fact]
            public void IsScacAllowedToBookLoad_ContractRateTooHigh()
            {
                var loadCarrierScac = LOAD_CARRIER_SCAC;
                loadCarrierScac.ContractRate = LOAD.LineHaulRate + 5;
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(loadCarrierScac)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().Where(x => x.Message.EndsWith("is not allowed to book this load"));
            }

            [Fact]
            public void IsScacAllowedToBookLoad_UseOnlyScac()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithLoadCarrierScacRestriction(new LoadCarrierScacRestrictionEntity()
                    {
                        LoadId = LOAD.LoadId,
                        Scac = "USE ONLY SCAC",
                        LoadCarrierScacRestrictionTypeId = Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.UseOnly)
                    })
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().Where(x => x.Message.EndsWith("is not allowed to book this load"));
            }

            [Fact]
            public void IsScacAllowedToBookLoad_DoNotUseScac()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithLoadCarrierScacRestriction(new LoadCarrierScacRestrictionEntity()
                    {
                        LoadId = LOAD.LoadId,
                        Scac = VALID_USER.PrimaryScac,
                        LoadCarrierScacRestrictionTypeId = Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.DoNotUse)
                    })
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                Action action = () => _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
                action.Should().Throw<ValidationException>().Where(x => x.Message.EndsWith("is not allowed to book this load"));
            }

            [Fact]
            public void IsScacAllowedToBookLoad_Success_ContractRate()
            {
                var loadCarrierScac = LOAD_CARRIER_SCAC;
                loadCarrierScac.ContractRate = LOAD.LineHaulRate;
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(loadCarrierScac)
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
            }

            [Fact]
            public void IsScacAllowedToBookLoad_Success_UseOnly()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithLoadCarrierScacRestriction(new LoadCarrierScacRestrictionEntity()
                    {
                        LoadId = LOAD.LoadId,
                        Scac = VALID_USER.PrimaryScac,
                        LoadCarrierScacRestrictionTypeId = Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.UseOnly)
                    })
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
            }

            [Fact]
            public void IsScacAllowedToBookLoad_Success_DoNotUse()
            {
                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithLoadCarrierScacRestriction(new LoadCarrierScacRestrictionEntity()
                    {
                        LoadId = LOAD.LoadId,
                        Scac = "DO NOT USE SCAC",
                        LoadCarrierScacRestrictionTypeId = Convert.ToInt32(LoadCarrierScacRestrictionTypeEnum.DoNotUse)
                    })
                    .WithUser(VALID_USER)
                    .Build();
                InitLoadService();

                _svc.PendingAcceptLoad(LOAD_DATA, USER_ID);
            }

            private void InitSeedData()
            {
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 100M,
                    ManuallyCreated = false,
                    Contacts = new List<LoadContactEntity>
                    {
                        new LoadContactEntity
                        {
                            LoadContactId = Guid.NewGuid(),
                            LoadId = LOAD_ID,
                            Phone = "123-123-1234",
                            Email = "test@email.com"
                        }
                    },
                    LoadStops = new List<LoadStopEntity>
                    {
                        new LoadStopEntity
                        {
                            LoadId = LOAD_ID,
                            StopNbr = 1,
                        },
                        new LoadStopEntity
                        {
                            LoadId = LOAD_ID,
                            StopNbr = 2
                        }
                    },
                    EquipmentId = EQUIPMENT_ID,
                    Equipment = new EquipmentEntity
                    {
                        EquipmentId = EQUIPMENT_ID,
                        EquipmentDesc = "Equipment",
                    },
                    LatestLoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                    LatestLoadTransaction = new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        LoadTransactionId = LATEST_LOAD_TRANSACTION_ID,
                        TransactionTypeId = TransactionTypes.New
                    }
                };
                LOAD_CARRIER_SCAC = new LoadCarrierScacEntity
                {
                    LoadCarrierScacId = LOAD_CARRIER_SCAC_ID,
                    LoadId = LOAD_ID,
                    Scac = SCAC,
                    ContractRate = 9.99M,
                    CarrierScac = new CarrierScacEntity()
                    {
                        Scac = SCAC
                    }
                };
                VALID_USER = new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID,
                    PrimaryCustomerId = CUSTOMER_ID,
                    PrimaryScac = LOAD_CARRIER_SCAC.Scac,
                    UserNotifications = new List<UserNotificationEntity>
                    {
                        new UserNotificationEntity
                        {
                            UserId = USER_ID,
                            IsDefault = true,
                            NotificationEnabled = true,
                            NotificationValue = "Notification",
                        }
                    }
                };
                LOAD_DATA = new LoadData
                {
                    LoadId = LOAD_ID,
                    Scac = LOAD_CARRIER_SCAC.Scac,
                    LineHaulRate = 100M
                };
            }
        }

        public class GetNumLoadsRequiringVisibilityInfoTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;
            private Mock<IShippingService> _shippingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;

            private LoadService _svc;

            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_ID_2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static Guid LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_TRANSACTION_ID_2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly int HOURS_UNTIL_PICKUP = 8;

            private List<LoadEntity> BOOKED_LOADS;
            private List<UserEntity> USERS;
            private List<CarrierScacEntity> CARRIER_SCACS;
            private List<LoadTransactionEntity> LOAD_TRANSACTIONS;
            private List<LoadClaimEntity> LOAD_CLAIMS;

            private List<CarrierScacData> CARRIER_SCAC_DATA;

            public GetNumLoadsRequiringVisibilityInfoTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _shippingService = new Mock<IShippingService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();

                _commonService = new Mock<ICommonService>();
                _commonService.Setup(x => x.GetCarrierVisibilityTypes(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new List<string>
                    {
                        CarrierVisibilityTypes.Project44,
                        CarrierVisibilityTypes.TopsToGo
                    });

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);

                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                _config = new Mock<IConfigurationRoot>();
                _config.SetupGet(x => x["LoadBoardVisibilityHoursUntilPickup"]).Returns(HOURS_UNTIL_PICKUP.ToString());

                _validationService = new Mock<ILoadValidationService>();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);


                _loadshopFeeService = new Mock<ILoadshopFeeService>();

                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                InitSeedData();
                InitDb();
                InitLoadService();

                _securityService.Setup(x => x.GetAuthorizedScasForCarrierByPrimaryScac()).Returns(CARRIER_SCAC_DATA.AsReadOnly());
            }

            private void InitLoadService()
            {
                _svc = new LoadService(
                    new Mock<ILogger<LoadService>>().Object,
                    _db.Object, _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepository.Object);
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoads(BOOKED_LOADS)
                    .WithUsers(USERS)
                    .WithLoadClaims(LOAD_CLAIMS)
                    .WithLoadTransactions(LOAD_TRANSACTIONS)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
            }

            [Fact]
            public void NoUsers_ThrowsException()
            {
                USERS = new List<UserEntity>();
                InitDb();
                InitLoadService();

                _svc.Invoking(x => x.GetNumLoadsRequiringVisibilityInfo(USER_ID))
                    .Should().Throw<Exception>()
                    .WithMessage("Invalid User");
            }

            [Fact]
            public void InvalidUserId_ThrowsException()
            {
                USERS = new List<UserEntity>();
                InitDb();
                InitLoadService();

                _svc.Invoking(x => x.GetNumLoadsRequiringVisibilityInfo(Guid.Empty))
                    .Should().Throw<Exception>()
                    .WithMessage("Invalid User");
            }

            [Fact]
            public void NoBookedLoads_ReturnsNUll()
            {
                BOOKED_LOADS = new List<LoadEntity>();
                InitDb();
                InitLoadService();

                var actual = _svc.GetNumLoadsRequiringVisibilityInfo(USER_ID);
                actual.Should().BeNull();
            }

            [Fact]
            public void NoCarrierVisibilityTypes_ReturnsNull()
            {
                _commonService.Reset();
                _commonService.Setup(x => x.GetCarrierVisibilityTypes(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(new List<string>());

                var actual = _svc.GetNumLoadsRequiringVisibilityInfo(USER_ID);
                actual.Should().BeNull();
            }

            [Fact]
            public void ReturnsNumOfLoadsRequiringVisibilityWarning()
            {
                var yesterday = DateTime.Now.AddDays(-1);
                _loadQueryRepository.Setup(lqr => lqr.GetLoadsForCarrierWithLoadClaim(It.IsAny<string[]>(), It.IsAny<string[]>()))
                   .Returns(new List<LoadViewData>()
                   {
                        new LoadViewData
                        {
                            ShowVisibilityWarning = true
                        }
                   }.AsQueryable());
                _loadQueryRepository.Setup(lqr => lqr.ShouldShowVisibility(It.IsAny<DateTime?>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<DateTime>(), It.IsAny<LoadViewData>())).Returns(true);

                var actual = _svc.GetNumLoadsRequiringVisibilityInfo(USER_ID);
                actual.NumRequiringInfo.Should().Be(1);
            }

            [Fact]
            public void ReturnsApplicableDate()
            {
                _loadQueryRepository.Setup(lqr => lqr.GetLoadsForCarrierWithLoadClaim(It.IsAny<string[]>(), It.IsAny<string[]>()))
                   .Returns(new List<LoadViewData>()
                   {
                        new LoadViewData
                        {

                        }
                   }.AsQueryable());

                var actual = _svc.GetNumLoadsRequiringVisibilityInfo(USER_ID);
                actual.ApplicableDate.Should().Be(NOW + TimeSpan.FromHours(HOURS_UNTIL_PICKUP));
            }

            [Fact]
            public void MissingHoursUntilPickupConfigSetting_DefaultsToEightHours()
            {
                _loadQueryRepository.Setup(lqr => lqr.GetLoadsForCarrierWithLoadClaim(It.IsAny<string[]>(), It.IsAny<string[]>()))
                    .Returns(new List<LoadViewData>()
                    {
                        new LoadViewData
                        {

                        }
                    }.AsQueryable());
                var defaultHoursUntilPickup = 8;
                var actual = _svc.GetNumLoadsRequiringVisibilityInfo(USER_ID);
                actual.ApplicableDate.Should().Be(NOW + TimeSpan.FromHours(defaultHoursUntilPickup));
            }

            private void InitSeedData()
            {
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        IdentUserId = USER_ID,
                        PrimaryCustomerId = CUSTOMER_ID,
                        PrimaryScac = "KBXL",
                        PrimaryScacEntity = new CarrierScacEntity
                        {
                            CarrierId = CARRIER_ID
                        },
                        UserNotifications = new List<UserNotificationEntity>
                        {
                            new UserNotificationEntity
                            {
                                UserId = USER_ID,
                                IsDefault = true,
                                NotificationEnabled = true,
                                NotificationValue = "Notification",
                            }
                        }
                    }
                };
                BOOKED_LOADS = new List<LoadEntity>
                {
                    new LoadEntity
                    {
                        LoadId = LOAD_ID,
                        LatestTransactionTypeId = "Accepted",
                        Equipment = new EquipmentEntity()
                        {
                            EquipmentDesc = "BIG TRUCK",
                            CategoryId = "BIG",
                            CategoryEquipmentDesc = "REALLY BIG TRUCK"
                        }
                    },
                    new LoadEntity
                    {
                        LoadId = LOAD_ID_2,
                        LatestTransactionTypeId = "Accepted",
                        Equipment = new EquipmentEntity()
                        {
                            EquipmentDesc = "BIG TRUCK",
                            CategoryId = "BIG",
                            CategoryEquipmentDesc = "REALLY BIG TRUCK"
                        }
                    }
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>()
                {
                    new LoadTransactionEntity()
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        LoadId = LOAD_ID
                    },
                    new LoadTransactionEntity()
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID_2,
                        LoadId = LOAD_ID_2
                    }
                };
                LOAD_CLAIMS = new List<LoadClaimEntity>()
                {
                    new LoadClaimEntity()
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        Scac = "KBXL"
                    },
                    new LoadClaimEntity()
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID_2,
                        Scac = "KBXL"
                    }
                };
                CARRIER_SCAC_DATA = new List<CarrierScacData>()
                {
                    new CarrierScacData()
                    {
                        Scac = "KBXL"
                    }
                };
                CARRIER_SCACS = new List<CarrierScacEntity>()
                {
                    new CarrierScacEntity()
                    {
                        Scac = "KBXL"
                    }
                };
            }
        }
    }

    public class LoadService_Customer_API_Tests
    {
        public class AcceptLoadTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;
            private Mock<IShippingService> _shippingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;

            private LoadService _svc;

            private static readonly Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string REFERENCE_LOAD_ID = "KL123456";
            private static readonly string PLATFORM_PLUS_LOAD_ID = "LS-123456";
            private static readonly long LOADBOARD_ID = 1234567;
            private static readonly Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string USERNAME = "username";
            private static readonly string SCAC = "SCAC";

            private LoadEntity LOAD;
            private LoadDetailData LOAD_DATA;
            private List<LoadTransactionEntity> LOAD_TRANSACTIONS;

            public AcceptLoadTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
                _commonService = new Mock<ICommonService>();
                _config = new Mock<IConfigurationRoot>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _validationService = new Mock<ILoadValidationService>();
                _securityService = new Mock<ISecurityService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _shippingService = new Mock<IShippingService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);

                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
                _loadshopFeeService = new Mock<ILoadshopFeeService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();

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
                    .WithLoad(LOAD)
                    .WithLoadTransactions(LOAD_TRANSACTIONS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new LoadService(
                    new Mock<ILogger<LoadService>>().Object,
                    _db.Object,
                    _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepository.Object);
            }

            [Fact]
            public void NoLoads_LoadNotFound_ThrowsException()
            {
                _db = new MockDbBuilder().Build(); // Empty
                InitService();

                var expected = "Load not found";
                Action action = () => _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage(expected);
            }

            [Fact]
            public void NoRefLoadId_NoPlatformPlusLoadId_ThrowsException()
            {
                LOAD.ReferenceLoadId = null;
                LOAD.PlatformPlusLoadId = null;
                InitDb();
                InitService();

                var expected = "Load not found";
                Action action = () => _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage(expected);
            }

            [Fact]
            public void NoRefLoadId_FoundByPlatformPlusLoadId()
            {
                LOAD.ReferenceLoadId = null;
                LOAD_DATA.ReferenceLoadId = PLATFORM_PLUS_LOAD_ID;
                InitDb();
                InitService();

                var actual = _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                actual.Should().NotBeNull();
            }

            [Fact]
            public void NoLatestLoadTransaction_ThrowsException()
            {
                LOAD.LatestLoadTransaction = null;
                InitDb();
                InitService();

                var expected = $"Load cannot be accepted because it hasn't been booked. Current load status: {LOAD.LatestTransactionTypeId}";
                Action action = () => _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage(expected);
            }

            [Fact]
            public void NoClaimOnTransaction_ThrowsException()
            {
                LOAD.LatestLoadTransaction.Claim = null;
                InitDb();
                InitService();

                var expected = $"Load cannot be accepted because it hasn't been booked. Current load status: {LOAD.LatestTransactionTypeId}";
                Action action = () => _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage(expected);
            }

            [Theory]
            [InlineData(TransactionTypes.PendingAdd)]
            [InlineData(TransactionTypes.PendingUpdate)]
            [InlineData(TransactionTypes.Posted)]
            [InlineData(TransactionTypes.PendingFuel)]
            [InlineData(TransactionTypes.PendingRates)]
            [InlineData(TransactionTypes.New)]
            [InlineData(TransactionTypes.Updated)]
            [InlineData(TransactionTypes.PreTender)]
            [InlineData(TransactionTypes.Accepted)]
            [InlineData(TransactionTypes.PendingRemoveScac)]
            [InlineData(TransactionTypes.PendingRemove)]
            [InlineData(TransactionTypes.Removed)]
            [InlineData(TransactionTypes.Delivered)]
            [InlineData(TransactionTypes.Error)]
            public void InvalidLatestTransactionTypeId_ThrowsException(string transactionTypeId)
            {
                LOAD.LatestTransactionTypeId = transactionTypeId;
                InitDb();
                InitService();

                var expected = $"Load cannot be accepted because it hasn't been booked. Current load status: {LOAD.LatestTransactionTypeId}";
                Action action = () => _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage(expected);
            }

            [Fact]
            public void AcceptedTransactionIsAdded()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                LoadTransactionEntity added = null;
                builder.MockLoadTransactions.Setup(x => x.Add(It.IsAny<LoadTransactionEntity>()))
                    .Callback((LoadTransactionEntity _) => { added = _; });

                _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                added.Should().NotBeNull();
            }

            [Fact]
            public void AcceptedTransactionIsFilledOutCorrectly()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                LoadTransactionEntity added = null;
                builder.MockLoadTransactions.Setup(x => x.Add(It.IsAny<LoadTransactionEntity>()))
                    .Callback((LoadTransactionEntity _) => { added = _; });

                _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                added.LoadId.Should().Be(LOAD.LoadId);
                added.LoadBoardId.Should().Be(LOAD_DATA.LoadBoardId);
                added.TransactionTypeId.Should().Be(TransactionTypes.Accepted);
                added.Claim.Should().NotBeNull();
                added.Claim.LineHaulRate.Should().Be(LOAD.LineHaulRate);
                added.Claim.FuelRate.Should().Be(LOAD.FuelRate);
                added.Claim.Scac.Should().Be(LOAD.LatestLoadTransaction.Claim.Scac);
                added.Claim.UserId.Should().Be(LOAD.LatestLoadTransaction.Claim.UserId);
                added.Claim.BillingLoadId.Should().Be(LOAD_DATA.BillingLoadId);
                added.Claim.BillingLoadDisplay.Should().Be(LOAD_DATA.BillingLoadDisplay);
            }

            [Fact]
            public void NoPendingTransaction_NoPendingTransactionIsUpdated()
            {
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                LoadTransactionEntity pending = null;
                builder.MockLoadTransactions.Setup(x => x.Update(It.IsAny<LoadTransactionEntity>()))
                    .Callback((LoadTransactionEntity _) => { pending = _; });

                _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                pending.Should().BeNull();
            }

            [Fact]
            public void PendingTransactionUpdatedWithLoadBoardId()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                LoadTransactionEntity pending = null;
                builder.MockLoadTransactions.Setup(x => x.Update(It.IsAny<LoadTransactionEntity>()))
                    .Callback((LoadTransactionEntity _) => { pending = _; });

                _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                pending.Should().NotBeNull();
                pending.LoadBoardId.Should().Be(LOAD_DATA.LoadBoardId);
            }

            [Fact]
            public void LoadHistoryIsAdded()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                LoadHistoryEntity added = null;
                builder.MockLoadHistories.Setup(x => x.Add(It.IsAny<LoadHistoryEntity>()))
                    .Callback((LoadHistoryEntity _) => { added = _; });

                _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                added.Should().NotBeNull();
            }

            [Fact]
            public void ChangesAreSaved()
            {
                _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                _db.Verify(x => x.SaveChanges(USERNAME), Times.Once);
            }

            [Fact]
            public void InputIsReturnedForSomeUnknownReason()
            {
                var actual = _svc.AcceptLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                actual.Should().Be(LOAD_DATA);
            }

            private void InitSeedData()
            {
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadId = REFERENCE_LOAD_ID,
                    PlatformPlusLoadId = PLATFORM_PLUS_LOAD_ID,
                    CustomerId = CUSTOMER_ID,
                    Customer = new CustomerEntity
                    {
                        IdentUserId = CUSTOMER_ID
                    },
                    LatestTransactionTypeId = TransactionTypes.SentToShipperTender,
                    LatestLoadTransaction = new LoadTransactionEntity
                    {
                        TransactionTypeId = TransactionTypes.SentToShipperTender,
                        Claim = new LoadClaimEntity
                        {
                            UserId = USER_ID,
                            Scac = SCAC
                        }
                    }
                };
                LOAD_DATA = new LoadDetailData
                {
                    ReferenceLoadId = REFERENCE_LOAD_ID,
                    LoadBoardId = LOADBOARD_ID,
                    BillingLoadId = "BillingLoadId",
                    BillingLoadDisplay = "BillingLoadDisplay",
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        TransactionTypeId = TransactionTypes.Pending
                    }
                };
            }
        }

        public class DeleteLoadTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<INotificationService> _notifyService;
            private readonly Mock<IUserContext> _userContext;
            private Mock<ICommonService> _commonService;
            private IConfigurationRoot _configRoot;
            private Mock<ILoadValidationService> _validationService;
            private Mock<ISecurityService> _securityService;
            private Mock<ISpecialInstructionsService> _specialInstructionsService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISmartSpotPriceService> _smartSpotService;
            private Mock<IShippingService> _shippingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private Mock<IConfigurationRoot> _config;
            private Mock<ILoadshopDocumentService> _loadshopDocumentService;
            private Mock<IDateTimeProvider> _dateTime;
            private static readonly DateTime NOW = new DateTime(2020, 1, 1);
            private ServiceUtilities _serviceUtilities;
            private Mock<ILoadshopFeeService> _loadshopFeeService;

            private LoadService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static string REFERENCE_LOAD_ID = "KL123456";
            private static long LOADBOARD_ID = 1234567;
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static string USERNAME = "username";

            private LoadEntity LOAD;

            private LoadDetailData LOAD_DATA;

            public DeleteLoadTests(TestFixture fixture)
            {
                InitSeedData();

                _db = new MockDbBuilder()
                    .WithLoad(LOAD)
                    .Build();

                _mapper = fixture.Mapper;
                _notifyService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
                _commonService = new Mock<ICommonService>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _mileageService = new Mock<IMileageService>();
                _validationService = new Mock<ILoadValidationService>();
                _securityService = new Mock<ISecurityService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _loadshopDocumentService = new Mock<ILoadshopDocumentService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);
                _dateTime.Setup(x => x.Today).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
                _loadshopFeeService = new Mock<ILoadshopFeeService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                _config = new Mock<IConfigurationRoot>();

                InitLoadService();
            }

            private void InitLoadService()
            {
                _svc = new LoadService(
                    new Mock<ILogger<LoadService>>().Object,
                    _db.Object,
                    _mapper,
                    _notifyService.Object,
                    _userContext.Object,
                    _commonService.Object,
                    _config.Object,
                    _validationService.Object,
                    _securityService.Object,
                    _specialInstructionsService.Object,
                    _mileageService.Object,
                    _smartSpotService.Object,
                    _loadshopDocumentService.Object,
                    _dateTime.Object,
                    _serviceUtilities,
                    _loadshopFeeService.Object,
                    _loadQueryRepository.Object);
            }

            [Fact]
            public void NoLoads_LoadNotFound_ThrowsException()
            {
                _db = new MockDbBuilder().Build(); // Empty
                InitLoadService();

                Action action = () => _svc.DeleteLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage("Load not found");
            }

            [Fact]
            public void InvalidRefLoadId_LoadNotFound_ThrowsException()
            {
                LOAD_DATA.ReferenceLoadId = "invalid";
                Action action = () => _svc.DeleteLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage("Load not found");
            }

            [Fact]
            public void InvalidCustomerId_LoadNotFound_ThrowsException()
            {
                var customerId = Guid.Empty;
                Action action = () => _svc.DeleteLoad(LOAD_DATA, customerId, USERNAME);
                action.Should().Throw<ValidationException>().WithMessage("Load not found");
            }

            [Fact]
            public void RemovedLoadTransactionIsAdded()
            {
                var expected = new LoadTransactionEntity
                {
                    LoadId = LOAD_ID,
                    LoadBoardId = LOADBOARD_ID,
                    TransactionTypeId = TransactionTypes.Removed,
                };

                var builder = new MockDbBuilder();
                _db = builder
                    .WithLoad(LOAD)
                    .Build();

                LoadTransactionEntity added = null;
                builder.MockLoadTransactions.Setup(_ => _.Add(It.IsAny<LoadTransactionEntity>())).Callback((LoadTransactionEntity _) => { added = _; });
                InitLoadService();

                _svc.DeleteLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                added.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void SaveChangesIsCalled()
            {
                _svc.DeleteLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                _db.Verify(x => x.SaveChanges(USERNAME), Times.Once);
            }

            [Fact]
            public void ReturnsSameLoadDataItWasCalledWith()
            {
                var actual = _svc.DeleteLoad(LOAD_DATA, CUSTOMER_ID, USERNAME);
                actual.Should().BeEquivalentTo(LOAD_DATA);
            }

            private void InitSeedData()
            {
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadId = REFERENCE_LOAD_ID,
                    CustomerId = CUSTOMER_ID,
                    Customer = new CustomerEntity
                    {
                        IdentUserId = CUSTOMER_ID
                    }
                };
                LOAD_DATA = new LoadDetailData
                {
                    ReferenceLoadId = REFERENCE_LOAD_ID,
                    LoadBoardId = LOADBOARD_ID
                };
            }
        }

        public class GetAllOpenLoadsByCustomerIdTests
        {
            public GetAllOpenLoadsByCustomerIdTests() { }

            /// <summary>
            /// Just documenting that we are intentionally not testing a simple CRUD operation
            /// </summary>
            [Fact]
            public void NoNeedToTest()
            {
                true.Should().BeTrue();
            }
        }
    }
}
