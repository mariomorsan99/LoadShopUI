using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Loadshop.Data;
using Loadshop.DomainServices.Common.Services;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class LoadServiceUnitTestV2
    {

        public class UpdateScacsTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<IConfigurationRoot> _config;
            private readonly Mock<INotificationService> _notificationService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISpecialInstructionsService> _specialInstructionsService;
            private readonly Mock<ISmartSpotPriceService> _smartSpotService;
            private readonly Mock<ILoadshopDocumentService> _documentService;
            private readonly Mock<ILoadValidationService> _loadValidationService;
            private readonly Mock<IDateTimeProvider> _dateTimeProvider;
            private readonly Mock<ILoadshopFeeService> _loadshopFeeService;
            private readonly Mock<IMileageService> _mileageService;
            private readonly Mock<ILoadQueryRepository> _loadQueryRepository;
            private ServiceUtilities _serviceUtilities;

            internal readonly Guid LoadIdWithoutPendingRates = Guid.NewGuid();
            internal readonly Guid LoadIdWithPendingRates = Guid.NewGuid();
            internal readonly Guid LoadIdWithPendingFuel = Guid.NewGuid();

            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private List<LoadStopEntity> LOAD_STOPS;
            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_IDENT_ID = Guid.Parse("11111111-1111-1111-2222-111111111111");
            internal readonly decimal FuelRate = 123.45m;
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC = "SCAC";

            private ILoadService _svc;

            public UpdateScacsTests(TestFixture fixture)
            {
                _db = new Mock<LoadshopDataContext>();
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _notificationService = new Mock<INotificationService>();
                _securityService = new Mock<ISecurityService>();

                _commonService = new Mock<ICommonService>();
                _config = new Mock<IConfigurationRoot>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _documentService = new Mock<ILoadshopDocumentService>();
                _loadValidationService = new Mock<ILoadValidationService>();
                _dateTimeProvider = new Mock<IDateTimeProvider>();
                _loadshopFeeService = new Mock<ILoadshopFeeService>();
                _mileageService = new Mock<IMileageService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                _serviceUtilities = new ServiceUtilities(_dateTimeProvider.Object);


                // set up security service
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>
                {
                    new CustomerData
                    {
                        CustomerId = CUSTOMER_ID
                    }
                });
                _securityService.Setup(x => x.UserHasAction(new string[] { It.IsAny<string>() })).Returns(true);

                InitSeedData();
                InitDb();
                BuildService();
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
                CUSTOMER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID,
                    IdentUserId = CUSTOMER_IDENT_ID
                };
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoads(BuildLoads().ToArray())
                    .WithLoadStops(LOAD_STOPS)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .WithCustomer(CUSTOMER)
                    .Build();
            }
            private void BuildService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                 _db.Object,
                 _mapper,
                 _notificationService.Object,
                 _userContext.Object,
                 _commonService.Object,
                 _config.Object,
                 _loadValidationService.Object,
                 _securityService.Object,
                 _specialInstructionsService.Object,
                 _mileageService.Object,
                 _smartSpotService.Object,
                 _documentService.Object,
                 _dateTimeProvider.Object,
                 _serviceUtilities,
                 _loadshopFeeService.Object,
                 _loadQueryRepository.Object);
            }

            private IList<LoadCarrierScacEntity> BuildLoadCarrierScacs()
            {
                return new List<LoadCarrierScacEntity>
            {
                new LoadCarrierScacEntity()
                {
                    LoadId =  Guid.NewGuid(),
                    Scac = "ABCD",
                    ContractRate = null
                },
                new LoadCarrierScacEntity()
                {
                    LoadId =  Guid.NewGuid(),
                    Scac = "BCDE",
                    ContractRate = 1.23m
                },
                new LoadCarrierScacEntity()
                {
                    LoadId =  Guid.NewGuid(),
                    Scac = "CDEF",
                    ContractRate = 2.34m
                }
            };
            }

            private IList<LoadEntity> BuildLoads()
            {
                return new List<LoadEntity>
            {
                new LoadEntity()
                {
                    ReferenceLoadId = "load without transactions",
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    }
                },

                new LoadEntity()
                {
                    LoadId = LoadIdWithoutPendingRates,
                    ReferenceLoadId = "load missing PendingRates transaction",
                    LatestTransactionTypeId = TransactionTypes.New,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    }
                },

                new LoadEntity()
                {
                    LoadId = LoadIdWithPendingRates,
                    ReferenceLoadId = "load with PendingRates transaction",
                    LatestTransactionTypeId = TransactionTypes.PendingRates,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    CarrierScacs = BuildLoadCarrierScacs().ToList()
                },

                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "load with fuel rate",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            EarlyDtTm = DateTime.Now.AddDays(1)
                        }
                    }
                },

                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "load in the past",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            EarlyDtTm = new DateTime(2019, 1, 1)
                        }
                    }
                },

                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "load in the past 2",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 3,
                            LateDtTm = new DateTime(2019, 1, 3)
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 2,
                            LateDtTm = new DateTime(2019, 1, 2)
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            LateDtTm = new DateTime(2019, 1, 1)
                        }
                    }
                },

                new LoadEntity()
                {
                    LoadId = LoadIdWithPendingFuel,
                    ReferenceLoadId = "load with fuel rate and PendingFuel",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            EarlyDtTm = DateTime.Now.AddDays(1)
                        }
                    }
                },
                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "update load - keep in marketplace",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 2,
                            LateDtTm = DateTime.Now.AddDays(1)
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            LateDtTm = DateTime.Now.AddDays(-1)
                        }
                    },
                    Contacts = new List<LoadContactEntity>(),
                    LoadServiceTypes = new List<LoadServiceTypeEntity>(),
                    LatestTransactionTypeId = "Posted" // simulate load being in marketplace
                },
            };
            }

            [Fact]
            public void UpdateScacs_CustomerNotFound()
            {
                LoadUpdateScacData loadScacs = null;
                _svc.Invoking(y => y.UpdateScacs(loadScacs, Guid.NewGuid(), "unit_test"))
                    .Should().Throw<ValidationException>()
                    .WithMessage("Customer not found");
            }

            [Fact]
            public void UpdateScacs_Null()
            {
                LoadUpdateScacData loadScacs = null;
                _svc.Invoking(y => y.UpdateScacs(loadScacs, CUSTOMER_IDENT_ID, "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Load not found");
            }

            [Fact]
            public void UpdateScacs_LoadNotFound()
            {
                LoadUpdateScacData loadScacs = new LoadUpdateScacData()
                {
                    ReferenceLoadId = "invalid"
                };
                _svc.Invoking(y => y.UpdateScacs(loadScacs, CUSTOMER_IDENT_ID, "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Load not found");
            }

            [Fact]
            public void UpdateScacs_LoadWithoutTransactions()
            {
                LoadUpdateScacData loadScacs = new LoadUpdateScacData()
                {
                    ReferenceLoadId = "load without transactions"
                };
                _svc.Invoking(y => y.UpdateScacs(loadScacs, CUSTOMER_IDENT_ID, "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Load SCACs cannot be updated because load has not been created");
            }

            [Fact]
            public void UpdateScacs_LoadWithIncorrectStatus()
            {
                LoadUpdateScacData loadScacs = new LoadUpdateScacData()
                {
                    ReferenceLoadId = "load missing PendingRates transaction"
                };
                _svc.Invoking(y => y.UpdateScacs(loadScacs, CUSTOMER_IDENT_ID, "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Load SCACs cannot be updated because its current status is: New");
            }

            [Fact]
            public void UpdateScacs()
            {
                LoadUpdateScacData loadScacs = new LoadUpdateScacData()
                {
                    ReferenceLoadId = "load with PendingRates transaction",
                    LoadBoardId = 12345,
                    CarrierScacs = BuildLoadCarrierScacs().Select(x => new LoadCarrierScacData()
                    {
                        Scac = x.Scac,
                        ContractRate = x.ContractRate
                    }).ToList()
                };

                loadScacs.CarrierScacs[0].ContractRate = 5.67m;
                loadScacs.CarrierScacs[1].ContractRate = null;
                loadScacs.CarrierScacs.RemoveAt(2);

                _svc.UpdateScacs(loadScacs, CUSTOMER_IDENT_ID, "unit_test");
            }
        }

        public class UpdateFuelTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<IConfigurationRoot> _config;
            private readonly Mock<INotificationService> _notificationService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISpecialInstructionsService> _specialInstructionsService;
            private readonly Mock<ISmartSpotPriceService> _smartSpotService;
            private readonly Mock<ILoadshopDocumentService> _documentService;
            private readonly Mock<ILoadValidationService> _loadValidationService;
            private readonly Mock<IDateTimeProvider> _dateTimeProvider;
            private readonly Mock<ILoadshopFeeService> _loadshopFeeService;
            private readonly Mock<IShippingService> _shippingService;
            private readonly Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private readonly Mock<IMileageService> _mileageService;
            private readonly Mock<ILoadQueryRepository> _loadQueryRepository;
            private ServiceUtilities _serviceUtilities;

            internal readonly Guid LoadIdWithoutPendingRates = Guid.NewGuid();
            internal readonly Guid LoadIdWithPendingRates = Guid.NewGuid();
            internal readonly Guid LoadIdWithPendingFuel = Guid.NewGuid();

            private LoadEntity LOAD;
            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private List<LoadStopEntity> LOAD_STOPS;
            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LATEST_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_IDENT_ID = Guid.Parse("11111111-1111-2222-1111-111111111111");
            private static readonly string EQUIPMENT_ID = "EquipmentId";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC = "SCAC";
            internal readonly decimal FuelRate = 123.45m;
            private static readonly string LOADSHOP_SEPARATORS = ";,|,~,:";
            private static readonly string LOADSHOP_URL = "https://loadshop.com";

            private ILoadService _svc;

            public UpdateFuelTests(TestFixture fixture)
            {
                _db = new Mock<LoadshopDataContext>();
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _notificationService = new Mock<INotificationService>();
                _securityService = new Mock<ISecurityService>();

                _commonService = new Mock<ICommonService>();
                _config = new Mock<IConfigurationRoot>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _documentService = new Mock<ILoadshopDocumentService>();
                _loadValidationService = new Mock<ILoadValidationService>();
                _dateTimeProvider = new Mock<IDateTimeProvider>();
                _loadshopFeeService = new Mock<ILoadshopFeeService>();
                _shippingService = new Mock<IShippingService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _mileageService = new Mock<IMileageService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                _serviceUtilities = new ServiceUtilities(_dateTimeProvider.Object);

                _config = new Mock<IConfigurationRoot>();
                var _configSection = new Mock<IConfigurationSection>();
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == "LoadshopShopItUrlLoadIdSeparators"))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns(LOADSHOP_SEPARATORS);

                var _urlConfigSection = new Mock<IConfigurationSection>();
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == "LoadshopShopItHomeTabURL"))).Returns(_urlConfigSection.Object);
                _urlConfigSection.Setup(a => a.Value).Returns(LOADSHOP_URL);


                // set up security service
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>
                {
                    new CustomerData
                    {
                        CustomerId = CUSTOMER_ID
                    }
                });
                _securityService.Setup(x => x.UserHasAction(new string[] { It.IsAny<string>() })).Returns(true);

                InitSeedData();
                InitDb();
                BuildService();
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
                    CustomerId = CUSTOMER_ID,
                    IdentUserId = CUSTOMER_IDENT_ID
                };
            }

            private IList<LoadEntity> BuildLoads()
            {
                return new List<LoadEntity>
            {
                new LoadEntity()
                {
                    ReferenceLoadId = "load without transactions",
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    }
                },

                new LoadEntity()
                {
                    LoadId = LoadIdWithoutPendingRates,
                    ReferenceLoadId = "load missing PendingRates transaction",
                    LatestTransactionTypeId = TransactionTypes.New,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    }
                },

                new LoadEntity()
                {
                    LoadId = LoadIdWithPendingRates,
                    ReferenceLoadId = "load with PendingRates transaction",
                    LatestTransactionTypeId = TransactionTypes.PendingRates,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    CarrierScacs = BuildLoadCarrierScacs().ToList()
                },

                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "load with fuel rate",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            EarlyDtTm = DateTime.Now.AddDays(1)
                        }
                    }
                },

                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "load in the past",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            EarlyDtTm = new DateTime(2019, 1, 1)
                        }
                    }
                },

                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "load in the past 2",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 3,
                            LateDtTm = new DateTime(2019, 1, 3)
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 2,
                            LateDtTm = new DateTime(2019, 1, 2)
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            LateDtTm = new DateTime(2019, 1, 1)
                        }
                    }
                },

                new LoadEntity()
                {
                    LoadId = LoadIdWithPendingFuel,
                    ReferenceLoadId = "load with fuel rate and PendingFuel",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            EarlyDtTm = DateTime.Now.AddDays(1)
                        }
                    }
                },
                new LoadEntity()
                {
                    LoadId = Guid.NewGuid(),
                    ReferenceLoadId = "update load - keep in marketplace",
                    FuelRate = FuelRate,
                    Customer = new CustomerEntity()
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_IDENT_ID
                    },
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 2,
                            LateDtTm = DateTime.Now.AddDays(1)
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 1,
                            LateDtTm = DateTime.Now.AddDays(-1)
                        }
                    },
                    Contacts = new List<LoadContactEntity>(),
                    LoadServiceTypes = new List<LoadServiceTypeEntity>(),
                    LatestTransactionTypeId = "Posted" // simulate load being in marketplace
                },
            };
            }
            private IList<LoadCarrierScacEntity> BuildLoadCarrierScacs()
            {
                return new List<LoadCarrierScacEntity>
            {
                new LoadCarrierScacEntity()
                {
                    LoadId =  Guid.NewGuid(),
                    Scac = "ABCD",
                    ContractRate = null
                },
                new LoadCarrierScacEntity()
                {
                    LoadId =  Guid.NewGuid(),
                    Scac = "BCDE",
                    ContractRate = 1.23m
                },
                new LoadCarrierScacEntity()
                {
                    LoadId =  Guid.NewGuid(),
                    Scac = "CDEF",
                    ContractRate = 2.34m
                }
            };
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithLoads(BuildLoads().ToArray())
                    .WithLoadStops(LOAD_STOPS)
                    .WithLoadCarrierScac(LOAD_CARRIER_SCAC)
                    .WithUser(VALID_USER)
                    .WithCustomer(CUSTOMER)
                    .Build();
            }
            private void BuildService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                 _db.Object,
                 _mapper,
                 _notificationService.Object,
                 _userContext.Object,
                 _commonService.Object,
                 _config.Object,
                 _loadValidationService.Object,
                 _securityService.Object,
                 _specialInstructionsService.Object,
                 _mileageService.Object,
                 _smartSpotService.Object,
                 _documentService.Object,
                new DateTimeProvider(),
                 _serviceUtilities,
                 _loadshopFeeService.Object,
                 _loadQueryRepository.Object);
            }

            [Fact]
            public void UpdateFuel_CustomerNotFound()
            {
                LoadUpdateFuelData loadFuel = null;
                _svc.Invoking(y => y.UpdateFuel(loadFuel, Guid.NewGuid(), "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Customer not found");
            }

            [Fact]
            public void UpdateFuel_Null()
            {
                LoadUpdateFuelData loadFuel = null;
                _svc.Invoking(y => y.UpdateFuel(loadFuel, CUSTOMER_IDENT_ID, "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Load not found");
            }

            [Fact]
            public void UpdateFuel_LoadNotFound()
            {
                LoadUpdateFuelData loadFuel = new LoadUpdateFuelData()
                {
                    ReferenceLoadId = "invalid"
                };
                _svc.Invoking(y => y.UpdateFuel(loadFuel, CUSTOMER_IDENT_ID, "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Load not found");
            }

            [InlineData("load in the past")]
            [InlineData("load in the past 2")]
            [Theory]
            public void UpdateFuel_LoadIsInThePast(string referenceLoadId)
            {
                LoadUpdateFuelData loadFuel = new LoadUpdateFuelData()
                {
                    ReferenceLoadId = referenceLoadId,
                    FuelRate = FuelRate
                };
                _svc.Invoking(y => y.UpdateFuel(loadFuel, CUSTOMER_IDENT_ID, "unit_test"))
                     .Should().Throw<ValidationException>()
                     .WithMessage("Fuel cannot be updated on loads from the past");
            }

            [Fact]
            public void UpdateFuel_FuelIsTheSame()
            {
                LoadUpdateFuelData loadFuel = new LoadUpdateFuelData()
                {
                    ReferenceLoadId = "load with fuel rate",
                    FuelRate = FuelRate
                };

                _svc.UpdateFuel(loadFuel, CUSTOMER_IDENT_ID, "unit_test").Should().BeFalse();
            }

            [Fact]
            public void UpdateFuel_PendingFuelTransaction()
            {
                LoadUpdateFuelData loadFuel = new LoadUpdateFuelData()
                {
                    ReferenceLoadId = "load with fuel rate and PendingFuel",
                    FuelRate = FuelRate * 2
                };

                _svc.UpdateFuel(loadFuel, CUSTOMER_IDENT_ID, "unit_test").Should().BeTrue();
            }

            [Fact]
            public void UpdateFuel()
            {
                LoadUpdateFuelData loadFuel = new LoadUpdateFuelData()
                {
                    ReferenceLoadId = "load with fuel rate",
                    FuelRate = FuelRate * 2
                };

                _svc.UpdateFuel(loadFuel, CUSTOMER_IDENT_ID, "unit_test").Should().BeTrue();
            }

            [Fact]

            public void GenerateReturnUrl_Null()
            {
                // act
                _svc.Invoking(x => x.GenerateReturnURL(new LoadDetailData[1] { null })).Should().Throw<Exception>().Where(x => x.Message.StartsWith("Reference Load Id is required."));
                _svc.Invoking(x => x.GenerateReturnURL(new LoadDetailData[1] { new LoadDetailData() { ReferenceLoadId = null } })).Should().Throw<Exception>().Where(x => x.Message.StartsWith("Reference Load Id is required."));
            }

            [Fact]

            public void GenerateReturnUrl()
            {
                var loads1 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test-Load1" } };
                var loads2 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test-Load1" }, new LoadDetailData { ReferenceLoadId = "Test-Load2" }, new LoadDetailData { ReferenceLoadId = "Test-Load3" } }; // separator should be ;
                var loads3 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test;Load1" }, new LoadDetailData { ReferenceLoadId = "Test;Load2" }, new LoadDetailData { ReferenceLoadId = "Test;Load3" } }; // separator should be |
                var loads4 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test:Load1" }, new LoadDetailData { ReferenceLoadId = "Test:Load2" }, new LoadDetailData { ReferenceLoadId = "Test:Load3" } }; // separator should be ;
                var loads5 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test|Load1" }, new LoadDetailData { ReferenceLoadId = "Test|Load2" }, new LoadDetailData { ReferenceLoadId = "Test|Load3" } };// separator should be ;
                var loads6 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test~Load1" }, new LoadDetailData { ReferenceLoadId = "Test~Load2" }, new LoadDetailData { ReferenceLoadId = "Test~Load3" } };// separator should be ;
                var loads7 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test;Load1" }, new LoadDetailData { ReferenceLoadId = "Test|Load2" }, new LoadDetailData { ReferenceLoadId = "Test~Load3" }, new LoadDetailData { ReferenceLoadId = "Test~Load4" } }; // separator should be :
                var loads8 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test;Load1" }, new LoadDetailData { ReferenceLoadId = "Test|Load2" }, new LoadDetailData { ReferenceLoadId = "Test;Load3" }, new LoadDetailData { ReferenceLoadId = "Test;Load4" } }; // separator should be ~
                var loads9 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test;Load1" }, new LoadDetailData { ReferenceLoadId = "Test:Load2" }, new LoadDetailData { ReferenceLoadId = "Test~Load3" }, new LoadDetailData { ReferenceLoadId = "Test~Load4" } }; // separator should be |
                var loads10 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test:Load1" }, new LoadDetailData { ReferenceLoadId = "Test|Load2" }, new LoadDetailData { ReferenceLoadId = "Test~Load3" }, new LoadDetailData { ReferenceLoadId = "Test~Load4" } }; // separator should be ;
                var loads11 = new LoadDetailData[] { new LoadDetailData { ReferenceLoadId = "Test;Load1" }, new LoadDetailData { ReferenceLoadId = "Test|Load2" }, new LoadDetailData { ReferenceLoadId = "Test~Load3" }, new LoadDetailData { ReferenceLoadId = "Test:Load4" } }; // should throw error

                var results = _svc.GenerateReturnURL(loads1);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=;");

                results = _svc.GenerateReturnURL(loads2);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=;");

                results = _svc.GenerateReturnURL(loads3);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=|");

                results = _svc.GenerateReturnURL(loads4);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=;");

                results = _svc.GenerateReturnURL(loads5);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=;");

                results = _svc.GenerateReturnURL(loads6);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=;");

                results = _svc.GenerateReturnURL(loads7);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=:");

                results = _svc.GenerateReturnURL(loads8);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=~");

                results = _svc.GenerateReturnURL(loads9);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=|");

                results = _svc.GenerateReturnURL(loads10);
                System.Net.WebUtility.UrlDecode(results).Should().EndWith("&sep=;");

                _svc.Invoking(x => x.GenerateReturnURL(loads11)).Should().Throw<Exception>().Where(x => x.Message.StartsWith("The sent Reference LoadIds contain:"));
            }
        }

        public class CreateLoadTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<IConfigurationRoot> _config;
            private readonly Mock<INotificationService> _notificationService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISpecialInstructionsService> _specialInstructionsService;
            private readonly Mock<ISmartSpotPriceService> _smartSpotService;
            private readonly Mock<ILoadshopDocumentService> _documentService;
            private readonly Mock<ILoadValidationService> _loadValidationService;
            private readonly Mock<IDateTimeProvider> _dateTimeProvider;
            private readonly Mock<ILoadshopFeeService> _loadshopFeeService;
            private readonly Mock<IShippingService> _shippingService;
            private readonly Mock<ILoadQueryRepository> _loadQueryRepository;
            private readonly Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private readonly Mock<IMileageService> _mileageService;
            private ServiceUtilities _serviceUtilities;


            private LoadEntity LOAD;
            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private List<LoadStopEntity> LOAD_STOPS;
            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LATEST_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string EQUIPMENT_ID = "EquipmentId";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC = "SCAC";

            private ILoadService _svc;

            public CreateLoadTests(TestFixture fixture)
            {
                _db = new Mock<LoadshopDataContext>();
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _notificationService = new Mock<INotificationService>();
                _securityService = new Mock<ISecurityService>();

                _commonService = new Mock<ICommonService>();
                _config = new Mock<IConfigurationRoot>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _documentService = new Mock<ILoadshopDocumentService>();
                _loadValidationService = new Mock<ILoadValidationService>();
                _dateTimeProvider = new Mock<IDateTimeProvider>();
                _loadshopFeeService = new Mock<ILoadshopFeeService>();
                _shippingService = new Mock<IShippingService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _mileageService = new Mock<IMileageService>();

                _serviceUtilities = new ServiceUtilities(_dateTimeProvider.Object);


                // set up security service
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>
                {
                    new CustomerData
                    {
                        CustomerId = CUSTOMER_ID
                    }
                });
                _securityService.Setup(x => x.UserHasAction(new string[] { It.IsAny<string>() })).Returns(true);

                InitSeedData();
                InitDb();
                BuildService();
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
                    CustomerId = CUSTOMER_ID,
                    IdentUserId = CUSTOMER_ID
                };
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
            private void BuildService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                 _db.Object,
                 _mapper,
                 _notificationService.Object,
                 _userContext.Object,
                 _commonService.Object,
                 _config.Object,
                 _loadValidationService.Object,
                 _securityService.Object,
                 _specialInstructionsService.Object,
                 _mileageService.Object,
                 _smartSpotService.Object,
                 _documentService.Object,
                 _dateTimeProvider.Object,
                 _serviceUtilities,
                 _loadshopFeeService.Object,
                 _loadQueryRepository.Object);
            }

            private LoadDetailData GenerateFakeLoad()
            {
                return new LoadDetailData()
                {
                    ReferenceLoadId = "GPCPG-MF01254081",
                    ReferenceLoadDisplay = "MF01254081",
                    Stops = 0,
                    Miles = 929,
                    LineHaulRate = 2134.9878m,
                    FuelRate = 244.398m,
                    Weight = 30000,
                    Cube = 3000,
                    Commodity = "Paper",
                    EquipmentType = "53TF102",
                    IsHazMat = false,
                    IsAccepted = false,
                    Comments = "",
                    Contacts = new List<LoadContactData>()
                {
                  new LoadContactData()
                  {
                        Display = "MAT GUTSCHOW",
                        Email = "dummy@dummy.com",
                        Phone = "920-438-2779"
                  }
                },
                    LoadStops = new List<LoadStopData>(){
                    new LoadStopData(){
                    StopNbr = 1,
                    Address1 = "4302 PHOENIX AVENUE",
                    Address2 = "",
                    City = "FORT SMITH",
                    State = "AR",
                    Country = "USA",
                    PostalCode = "72903",
                    Latitude = 35.338333m,
                    Longitude = -94.385833m,
                    LateDtTm = DateTime.Now.AddDays(10),
                    ApptType = "BY",
                    Contacts = new List<LoadStopContactData>()
                },
               new LoadStopData() {
                    StopNbr = 2,
                    Address1 = "9767 PRITCHARD ROAD",
                    Address2 = "",
                    City = "JACKSONVILLE",
                    State = "FL",
                    Country = "USA",
                    PostalCode = "32219",
                    Latitude = 30.373056m,
                    Longitude = -81.813611m,
                    LateDtTm = DateTime.Now.AddDays(20),
                    ApptType = "CS",
                    Contacts = new List<LoadStopContactData>()
                }
                },
                    LoadTransaction = new LoadTransactionData()
                    {
                        TransactionType = TransactionTypeData.PendingAdd
                    },
                    CarrierScacs = new List<LoadCarrierScacData>(),
                    MobileExternallyEntered = false,
                    LineItems = new List<LoadLineItemData>(),
                    ServiceTypes = new List<ServiceTypeData>()
                };
            }

            [Fact]
            public async Task CreateLoad_Should_Create()
            {
                var loadData = GenerateFakeLoad();

                _smartSpotService.Setup(x => x.GetSmartSpotPricesAsync(It.IsAny<List<LoadshopSmartSpotPriceRequest>>()))
                    .ReturnsAsync((List<LoadshopSmartSpotPriceRequest> input) => new List<SmartSpotPrice>()
                    {
                    new SmartSpotPrice()
                    {
                        LoadId = input.First().LoadId,
                        DATGuardRate = 1m,
                        MachineLearningRate = 2m,
                        Price =3m,
                    }
                    });

                _shippingService.Setup(x => x.GetLoadCarrierScacs(It.IsAny<Guid>())).Returns(new List<LoadCarrierScacData>());
                _loadCarrierGroupService.Setup(x => x.GetLoadCarrierGroupsForLoad(It.IsAny<Guid>())).Returns(new List<ShippingLoadCarrierGroupData>());

                BuildService();

                var opts = new CreateLoadOptionsDto()
                {
                    AddSmartSpot = true
                };
                // act
                var result = await _svc.CreateLoad(loadData, CUSTOMER_ID, "test", opts);

                // assert
                result.Should().NotBeNull();
                result.Data.LineHaulRate.Should().Be(2134.9878m);
                _db.Verify(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
                _smartSpotService.Verify(x => x.GetSmartSpotPriceAsync(It.IsAny<LoadEntity>()), Times.Exactly(1));
            }

            [Fact]
            public void CreateLoad_Should_FailWhenNotEnabledForCustomer()
            {
                // arrange
                var loadData = GenerateFakeLoad();

                _shippingService.Setup(x => x.GetLoadCarrierScacs(It.IsAny<Guid>())).Returns(new List<LoadCarrierScacData>());
                _loadCarrierGroupService.Setup(x => x.GetLoadCarrierGroupsForLoad(It.IsAny<Guid>())).Returns(new List<ShippingLoadCarrierGroupData>());

                _db = new MockDbBuilder()
                    .WithCustomer(new CustomerEntity
                    {
                        CustomerId = CUSTOMER_ID,
                        IdentUserId = CUSTOMER_ID,
                        AllowManualLoadCreation = false
                    })
                    .Build();

                BuildService();

                var opts = new CreateLoadOptionsDto()
                {
                    AddSmartSpot = true,
                    ManuallyCreated = true
                };

                // act
                Func<Task> action = async () => await _svc.CreateLoad(loadData, CUSTOMER_ID, "test", opts);
                action.Should().Throw<ValidationException>().Where(x => x.Message.StartsWith("Customer not configured for Order Entry"));
            }
        }
        public class UpdateLoadTests : IClassFixture<TestFixture>
        {
            private LoadshopDataContext _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<IConfigurationRoot> _config;
            private readonly Mock<INotificationService> _notificationService;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly Mock<ISpecialInstructionsService> _specialInstructionsService;
            private readonly Mock<ISmartSpotPriceService> _smartSpotService;
            private readonly Mock<ILoadshopDocumentService> _documentService;
            private readonly Mock<ILoadValidationService> _loadValidationService;
            private readonly Mock<IDateTimeProvider> _dateTimeProvider;
            private readonly Mock<ILoadshopFeeService> _loadshopFeeService;
            private readonly Mock<IShippingService> _shippingService;
            private readonly Mock<ILoadCarrierGroupService> _loadCarrierGroupService;
            private readonly Mock<IMileageService> _mileageService;
            private readonly Mock<ILoadQueryRepository> _loadQueryRepository;
            private ServiceUtilities _serviceUtilities;


            private LoadEntity LOAD;
            private CustomerEntity CUSTOMER;
            private LoadCarrierScacEntity LOAD_CARRIER_SCAC;
            private UserEntity VALID_USER;
            private List<LoadStopEntity> LOAD_STOPS;
            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LATEST_LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_IDENTITY_ID = Guid.Parse("11111111-1111-2222-1111-111111111111");
            private static readonly string EQUIPMENT_ID = "EquipmentId";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string SCAC = "SCAC";
            internal readonly decimal FuelRate = 123.45m;
            internal readonly Guid LoadIdWithoutPendingRates = Guid.NewGuid();
            internal readonly Guid LoadIdWithPendingRates = Guid.NewGuid();
            internal readonly Guid LoadIdWithPendingFuel = Guid.NewGuid();

            private ILoadService _svc;

            public UpdateLoadTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _notificationService = new Mock<INotificationService>();
                _securityService = new Mock<ISecurityService>();

                _commonService = new Mock<ICommonService>();
                _config = new Mock<IConfigurationRoot>();
                _specialInstructionsService = new Mock<ISpecialInstructionsService>();
                _smartSpotService = new Mock<ISmartSpotPriceService>();
                _documentService = new Mock<ILoadshopDocumentService>();
                _loadValidationService = new Mock<ILoadValidationService>();
                _dateTimeProvider = new Mock<IDateTimeProvider>();
                _loadshopFeeService = new Mock<ILoadshopFeeService>();
                _shippingService = new Mock<IShippingService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();
                _mileageService = new Mock<IMileageService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                _serviceUtilities = new ServiceUtilities(_dateTimeProvider.Object);

                var options = new DbContextOptionsBuilder<LoadshopDataContext>()
                    .UseInMemoryDatabase(databaseName:"updateLoad_tests")
                    .Options;

                _db = new LoadshopDataContext(options);

                // set up security service
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>
                {
                    new CustomerData
                    {
                        CustomerId = CUSTOMER_ID
                    }
                });
                _securityService.Setup(x => x.UserHasAction(new string[] { It.IsAny<string>() })).Returns(true);

                InitSeedData();
                BuildService();
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
                _db.LoadCarrierScacs.Add(LOAD_CARRIER_SCAC);
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
                _db.LoadStops.AddRange(LOAD_STOPS);
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadId = LOAD_ID.ToString(),
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
                _db.Loads.Add(LOAD);
                CUSTOMER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID,
                    IdentUserId = CUSTOMER_IDENTITY_ID
                };

                _db.Customers.Add(CUSTOMER);
                _db.SaveChanges();
            }
            private void BuildService()
            {
                _svc = new LoadService(new Mock<ILogger<LoadService>>().Object,
                 _db,
                 _mapper,
                 _notificationService.Object,
                 _userContext.Object,
                 _commonService.Object,
                 _config.Object,
                 _loadValidationService.Object,
                 _securityService.Object,
                 _specialInstructionsService.Object,
                 _mileageService.Object,
                 _smartSpotService.Object,
                 _documentService.Object,
                 _dateTimeProvider.Object,
                 _serviceUtilities,
                 _loadshopFeeService.Object,
                 _loadQueryRepository.Object);
            }

            private LoadDetailData GenerateFakeLoad()
            {
                return new LoadDetailData()
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadId = LOAD_ID.ToString(),
                    ReferenceLoadDisplay = "MF01254081",
                    Stops = 0,
                    Miles = 929,
                    LineHaulRate = 2134.98m,
                    FuelRate = 244.398m,
                    Weight = 30000,
                    Cube = 3000,
                    Commodity = "Paper",
                    EquipmentType = "53TF102",
                    IsHazMat = false,
                    IsAccepted = false,
                    Comments = "",
                    Contacts = new List<LoadContactData>()
                {
                  new LoadContactData()
                  {
                        Display = "MAT GUTSCHOW",
                        Email = "dummy@dummy.com",
                        Phone = "920-438-2779"
                  }
                },
                    LoadStops = new List<LoadStopData>(){
                    new LoadStopData(){
                    StopNbr = 1,
                    Address1 = "4302 PHOENIX AVENUE",
                    Address2 = "",
                    City = "FORT SMITH",
                    State = "AR",
                    Country = "USA",
                    PostalCode = "72903",
                    Latitude = 35.338333m,
                    Longitude = -94.385833m,
                    LateDtTm = DateTime.Now.AddDays(10),
                    ApptType = "BY",
                    Contacts = new List<LoadStopContactData>()
                },
               new LoadStopData() {
                    StopNbr = 2,
                    Address1 = "9767 PRITCHARD ROAD",
                    Address2 = "",
                    City = "JACKSONVILLE",
                    State = "FL",
                    Country = "USA",
                    PostalCode = "32219",
                    Latitude = 30.373056m,
                    Longitude = -81.813611m,
                    LateDtTm = DateTime.Now.AddDays(20),
                    ApptType = "CS",
                    Contacts = new List<LoadStopContactData>()
                }
                },
                    LoadTransaction = new LoadTransactionData()
                    {
                        TransactionType = TransactionTypeData.PendingAdd
                    },
                    CarrierScacs = new List<LoadCarrierScacData>(),
                    MobileExternallyEntered = false,
                    LineItems = new List<LoadLineItemData>(),
                    ServiceTypes = new List<ServiceTypeData>()
                };
            }

            /// <summary>
            /// Note these tests are brittle as they are mocking internal API's for EF.  If the unit tests move to an inmemory db instead of mock, then we can remove these.
            /// </summary>
            /// <returns></returns>
            [Fact]
            public async Task UpdateLoadService_ShouldKeepLoadInMarketplace_WhenNoChanges()
            {
                // arrange
                var loadData = GenerateFakeLoad();
                
                var opts = new UpdateLoadOptions()
                {
                    MapObject = true,
                };
                
                // simulate the transaction type we put on incoming requests
                loadData.LoadTransaction = new LoadTransactionData()
                {
                    TransactionType = TransactionTypeData.PendingAdd
                };

                // act
                var update = await _svc.UpdateLoad(loadData, CUSTOMER_IDENTITY_ID, "test", opts);

                // assert
                update.Should().NotBeNull();
                update.IsSuccess.Should().BeTrue();
                // 2 transaction should have been added, one for the update, one for keeping the load in the marketplace
                //_db.Verify(x => x.LoadTransactions.Add(It.IsAny<LoadTransactionEntity>()), Times.Exactly(2));
                //_db.Verify(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
                _smartSpotService.Verify(x => x.GetSmartSpotPricesAsync(It.IsAny<List<LoadshopSmartSpotPriceRequest>>()), Times.Never);

                var dbLoad = _db.Loads.FirstOrDefault(x => x.LoadId == LOAD_ID);

                dbLoad.LineHaulRate.Should().Be(loadData.LineHaulRate);
            }
        }
    }
}
