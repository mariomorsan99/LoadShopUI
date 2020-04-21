using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI;
using Loadshop.DomainServices.Security;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class LoadStatusTransactionServiceTests
    {
        public abstract class BaseLoadStatusTransactionTests : IClassFixture<TestFixture>
        {
            protected readonly IMapper _mapper;
            protected readonly Mock<IUserContext> _userContext;
            protected MockDbBuilder _dbBuilder;
            protected Mock<LoadshopDataContext> _db;
            protected Mock<ISecurityService> _securityService;
            protected Mock<ICarrierWebAPIService> _carrierWebApiService;
            protected LoadStatusTransactionService _svc;

            protected static Guid VALID_CARRIER_USER_ID = Guid.Parse("99999999-9999-9999-9999-999999999999");
            protected static Guid SHIPPER_USER_ID = Guid.Parse("99999999-9999-9999-9999-999999999998");
            protected static Guid INVALID_USER_ID = Guid.Parse("88888888-8888-8888-8888-888888888888");
            protected static Guid VALID_LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            protected static Guid INVALID_LOAD_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");
            protected static Guid UNBOOKED_LOAD_ID = Guid.Parse("33333333-3333-3333-3333-333333333333");
            protected static Guid VALID_CUSTOMER_ID = Guid.Parse("12345678-1234-1234-1234-123456789012");
            protected static Guid INVALID_CUSTOMER_ID = Guid.Parse("12345678-1234-1234-1234-000000000000");
            protected static string VALID_REF_LOAD_ID = "TEST-12345678";

            protected static UserEntity VALID_CARRIER_USER => new UserEntity
            {
                IdentUserId = VALID_CARRIER_USER_ID,
                PrimaryScac = "TEST"
            };

            protected static UserEntity SHIPPER_USER => new UserEntity
            {
                IdentUserId = SHIPPER_USER_ID,
                PrimaryScac = null
            };

            protected static UserEntity INVALID_CARRIER_USER => new UserEntity
            {
                IdentUserId = INVALID_USER_ID,
                PrimaryScac = "INVALID"
            };

            protected static LoadEntity VALID_LOAD => new LoadEntity
            {
                LoadId = VALID_LOAD_ID,
                CustomerId = VALID_CUSTOMER_ID,
                ReferenceLoadId = VALID_REF_LOAD_ID,
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = "Accepted",
                    CreateDtTm = new DateTime(2020, 02, 11, 12, 0, 0),
                    Claim = new LoadClaimEntity
                    {
                        Scac = "TEST"
                    }
                },
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        StopNbr = 1,
                        //Oregon
                        Latitude = 46.161605m,
                        Longitude = -123.414906m
                    },
                    new LoadStopEntity
                    {
                        StopNbr = 2,
                        //Oregon
                        Latitude = 46.161605m,
                        Longitude = -123.414906m
                    },
                }
            };

            protected static LoadEntity UNBOOKED_LOAD => new LoadEntity
            {
                LoadId = UNBOOKED_LOAD_ID,
                CustomerId = VALID_CUSTOMER_ID,
                LatestLoadTransaction = new LoadTransactionEntity
                {
                    TransactionTypeId = "Pending",
                    CreateDtTm = new DateTime(2020, 02, 11, 11, 0, 0)
                }
            };

            protected List<LoadEntity> LOADS = new List<LoadEntity> { VALID_LOAD, UNBOOKED_LOAD };

            protected List<LoadStatusTransactionEntity> LOAD_STATUS_TRANSACTIONS = new List<LoadStatusTransactionEntity>
            {
                new LoadStatusTransactionEntity
                {
                    LoadId = VALID_LOAD_ID,
                    TransactionDtTm = new DateTime(2020, 2, 11, 9, 0, 0),
                    MessageId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 9, 30, 0))
                },
                new LoadStatusTransactionEntity
                {
                    LoadId = VALID_LOAD_ID,
                    TransactionDtTm = new DateTime(2020, 2, 11, 10, 0, 0),
                    MessageId = Guid.Parse("44444444-4444-4444-4444-444444444445"),
                    MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 30, 0))
                },
                new LoadStatusTransactionEntity
                {
                    LoadId = VALID_LOAD_ID,
                    TransactionDtTm = new DateTime(2020, 2, 11, 8, 0, 0),
                    MessageId = Guid.Parse("44444444-4444-4444-4444-444444444446"),
                    MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 8, 30, 0))
                },
                new LoadStatusTransactionEntity
                {
                    LoadId = INVALID_LOAD_ID,
                    TransactionDtTm = new DateTime(2020, 2, 11, 11, 0, 0),//Later that the latest on the valid load
                    MessageId = Guid.Parse("44444444-4444-4444-4444-444444444447"),
                    MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 11, 30, 0))
                }
            };

            protected BaseLoadStatusTransactionTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(LOADS)
                    .WithUsers(new List<UserEntity> { VALID_CARRIER_USER, INVALID_CARRIER_USER, SHIPPER_USER })
                    .WithLoadStatusTransactions(LOAD_STATUS_TRANSACTIONS)
                    .Build();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(_ => _.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(true);
                _securityService.Setup(_ => _.GetAuthorizedCustomersforUserAsync()).ReturnsAsync((new List<CustomerData>()
                {
                    new CustomerData
                    {
                        CustomerId = VALID_CUSTOMER_ID
                    }
                }).AsReadOnly());

                _carrierWebApiService = new Mock<ICarrierWebAPIService>();

                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(_ => _.UserId).Returns(VALID_CARRIER_USER_ID);
            }

            protected LoadStatusTransactionService CreateService()
            {
                return new LoadStatusTransactionService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _carrierWebApiService.Object);
            }

            protected void AssertHasError(BaseServiceResponse response, string urn, string message)
            {
                response.ModelState.Should().Contain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
            }

            protected void AssertNoError(BaseServiceResponse response, string urn, string message)
            {
                response.ModelState.Should().NotContain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
            }
        }

        public class GetLatestStatusTests : BaseLoadStatusTransactionTests
        {
            public GetLatestStatusTests(TestFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task LoadNotFound()
            {
                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetLatestStatus(INVALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task LoadNotAcceptedOrPending()
            {
                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetLatestStatus(UNBOOKED_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not accepted");
            }

            [Fact]
            public async Task UserDoesNotHaveAction()
            {
                _securityService.Setup(_ => _.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(false);

                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetLatestStatus(VALID_LOAD_ID);
                await action.Should().ThrowAsync<Exception>();
                _securityService.Verify(_ => _.UserHasActionAsync(It.Is<string[]>(actions =>
                    actions != null && actions.Length == 2
                        && actions[0] == SecurityActions.Loadshop_Ui_My_Loads_Status_View
                        && actions[1] == SecurityActions.Loadshop_Ui_Shopit_Load_View_Booked_Detail
                )));
            }

            [Fact]
            public async Task UserHasWrongPrimaryScac()
            {
                _userContext.SetupGet(_ => _.UserId).Returns(INVALID_USER_ID);

                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetLatestStatus(VALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task ShipperUserDoesntHaveCustomer()
            {
                _userContext.SetupGet(_ => _.UserId).Returns(SHIPPER_USER_ID);
                _securityService.Setup(_ => _.GetAuthorizedCustomersforUserAsync()).ReturnsAsync((new List<CustomerData>()).AsReadOnly());

                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetLatestStatus(VALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task NoLoadStatusTransactions()
            {
                _db = new MockDbBuilder()
                    .WithLoads(LOADS)
                    .WithUsers(new List<UserEntity> { VALID_CARRIER_USER })
                    .WithLoadStatusTransactions(new List<LoadStatusTransactionEntity>())
                    .Build();

                _svc = CreateService();

                var result = await _svc.GetLatestStatus(VALID_LOAD_ID);
                result.Should().BeNull();
            }

            [Fact]
            public async Task CarrierSuccess()
            {
                _svc = CreateService();

                var result = await _svc.GetLatestStatus(VALID_LOAD_ID);
                result.Should().NotBeNull();
                result.LoadId.Should().Be(VALID_LOAD_ID);
                result.MessageId.Should().Be(Guid.Parse("44444444-4444-4444-4444-444444444445"));
                result.MessageTime.Should().Be(new DateTimeOffset(new DateTime(2020, 2, 11, 10, 30, 0)));
                result.TransactionDtTm.Should().Be(new DateTime(2020, 2, 11, 10, 0, 0));
            }

            [Fact]
            public async Task ShipperSuccess()
            {
                _userContext.SetupGet(_ => _.UserId).Returns(SHIPPER_USER_ID);
                _svc = CreateService();

                var result = await _svc.GetLatestStatus(VALID_LOAD_ID);
                result.Should().NotBeNull();
                result.LoadId.Should().Be(VALID_LOAD_ID);
                result.MessageId.Should().Be(Guid.Parse("44444444-4444-4444-4444-444444444445"));
                result.MessageTime.Should().Be(new DateTimeOffset(new DateTime(2020, 2, 11, 10, 30, 0)));
                result.TransactionDtTm.Should().Be(new DateTime(2020, 2, 11, 10, 0, 0));
            }
        }

        public class AddInTransitStatusTests : BaseLoadStatusTransactionTests
        {
            public AddInTransitStatusTests(TestFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task LoadNotFound()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = INVALID_LOAD_ID
                };
                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddInTransitStatus(inTransitStatus);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task LoadNotAcceptedOrPending()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = UNBOOKED_LOAD_ID
                };
                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddInTransitStatus(inTransitStatus);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not accepted");
            }

            [Fact]
            public async Task UserDoesNotHaveAction()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID
                };
                _securityService.Setup(_ => _.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(false);

                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddInTransitStatus(inTransitStatus);
                await action.Should().ThrowAsync<Exception>();
                _securityService.Verify(_ => _.UserHasActionAsync(It.Is<string[]>(actions =>
                    actions != null && actions.Length == 1
                        && actions[0] == SecurityActions.Loadshop_Ui_My_Loads_Status_Update
                )));
            }

            [Fact]
            public async Task UserHasWrongPrimaryScac()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID
                };
                _userContext.SetupGet(_ => _.UserId).Returns(INVALID_USER_ID);

                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddInTransitStatus(inTransitStatus);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task LocationTimeLatitudeAndLongitudeIsNull()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = null,
                    Latitude = null,
                    Longitude = null
                };

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:LocationTime", "Status Date/Time is required");
                AssertHasError(response, $"urn:root:Latitude", "Location is required");
            }

            [Fact]
            public async Task LocationIsNull()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = null,
                    Latitude = 0,
                    Longitude = 0
                };

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:LocationTime", "Status Date/Time is required");
            }

            [Fact]
            public async Task LocationLatitudeIsNull()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                    Latitude = null,
                    Longitude = 0
                };

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:Latitude", "Location is required");
            }

            [Fact]
            public async Task LocationLongitudeIsNull()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                    Latitude = 0,
                    Longitude = null
                };

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:Longitude", "Location is required");
            }

            [Fact]
            public async Task SubmitsCorrectInTransitTime()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                    //Oregon
                    Latitude = 46.161605m,
                    Longitude = -123.414906m
                };
                _carrierWebApiService.Setup(_ => _.Send<InTransitLoadData>(It.IsAny<LoadStatusEvent<InTransitLoadData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeTrue();

                _carrierWebApiService.Verify(_ => _.Send<InTransitLoadData>(It.Is<LoadStatusEvent<InTransitLoadData>>(
                    e => e.MessageType == "LoadLocation"
                        && e.ApiVersion == "1.1"
                        && e.Payload.Loads[0].LoadNumber == VALID_REF_LOAD_ID
                        && e.Payload.Loads[0].Latitude == 46.161605m
                        && e.Payload.Loads[0].Longitude == -123.414906m
                        && e.Payload.Loads[0].LocationTime == "2020-02-11T10:00:00"
                        && e.Payload.Loads[0].IsLocalTime == true
                    )));
            }

            [Fact]
            public async Task SubmitsBillingLoadId()
            {
                var load = VALID_LOAD;
                load.PlatformPlusLoadId = "PLATFORM_PLUS_ID";
                load.LatestLoadTransaction.Claim.BillingLoadId = "BILLING_LOAD_ID";
                var loads = new List<LoadEntity> { load };
                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(loads)
                    .WithUsers(new List<UserEntity> { VALID_CARRIER_USER })
                    .Build();

                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                    //Oregon
                    Latitude = 46.161605m,
                    Longitude = -123.414906m
                };
                _carrierWebApiService.Setup(_ => _.Send<InTransitLoadData>(It.IsAny<LoadStatusEvent<InTransitLoadData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeTrue();

                _carrierWebApiService.Verify(_ => _.Send<InTransitLoadData>(It.Is<LoadStatusEvent<InTransitLoadData>>(
                    e => e.MessageType == "LoadLocation"
                        && e.ApiVersion == "1.1"
                        && e.Payload.Loads[0].LoadNumber == "BILLING_LOAD_ID"
                        && e.Payload.Loads[0].Latitude == 46.161605m
                        && e.Payload.Loads[0].Longitude == -123.414906m
                        && e.Payload.Loads[0].LocationTime == "2020-02-11T10:00:00"
                        && e.Payload.Loads[0].IsLocalTime == true
                    )));
            }

            [Fact]
            public async Task SubmitsPlatformPlusLoadId()
            {
                var load = VALID_LOAD;
                load.PlatformPlusLoadId = "PLATFORM_PLUS_ID";
                var loads = new List<LoadEntity> { load };
                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(loads)
                    .WithUsers(new List<UserEntity> { VALID_CARRIER_USER })
                    .Build();

                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                    //Oregon
                    Latitude = 46.161605m,
                    Longitude = -123.414906m
                };
                _carrierWebApiService.Setup(_ => _.Send<InTransitLoadData>(It.IsAny<LoadStatusEvent<InTransitLoadData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeTrue();

                _carrierWebApiService.Verify(_ => _.Send<InTransitLoadData>(It.Is<LoadStatusEvent<InTransitLoadData>>(
                    e => e.MessageType == "LoadLocation"
                        && e.ApiVersion == "1.1"
                        && e.Payload.Loads[0].LoadNumber == "PLATFORM_PLUS_ID"
                        && e.Payload.Loads[0].Latitude == 46.161605m
                        && e.Payload.Loads[0].Longitude == -123.414906m
                        && e.Payload.Loads[0].LocationTime == "2020-02-11T10:00:00"
                        && e.Payload.Loads[0].IsLocalTime == true
                    )));
            }

            [Fact]
            public async Task PreventsFutureTimeForTimeZone()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = new DateTimeOffset(DateTime.Now),//Assuming this is central time
                    //Oregon
                    Latitude = 46.161605m,
                    Longitude = -123.414906m
                };
                _carrierWebApiService.Setup(_ => _.Send<InTransitLoadData>(It.IsAny<LoadStatusEvent<InTransitLoadData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:LocationTime", "Status Date/Time must be in the past");
            }

            [Fact]
            public async Task AddLoadStatusTransaction()
            {
                var inTransitStatus = new LoadStatusInTransitData
                {
                    LoadId = VALID_LOAD_ID,
                    LocationTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                    //Oregon
                    Latitude = 46.161605m,
                    Longitude = -123.414906m
                };
                _carrierWebApiService.Setup(_ => _.Send<InTransitLoadData>(It.IsAny<LoadStatusEvent<InTransitLoadData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddInTransitStatus(inTransitStatus);
                response.IsSuccess.Should().BeTrue();

                _dbBuilder.MockLoadStatusTransactions.Verify(_ => _.Add(It.Is<LoadStatusTransactionEntity>(x =>
                    x.MessageId == Guid.Parse("44444444-4444-4444-4444-555555555555")
                    && x.MessageTime == new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    && x.LoadId == VALID_LOAD_ID
                    //can't verity the TransactionDtTm since it's using DateTime.Now
                )));
            }
        }

        public class AddStopStatusesTests : BaseLoadStatusTransactionTests
        {

            public AddStopStatusesTests(TestFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task LoadNotFound()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = INVALID_LOAD_ID
                };
                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddStopStatuses(stopStatuses);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task LoadNotAcceptedOrPending()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = UNBOOKED_LOAD_ID
                };
                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddStopStatuses(stopStatuses);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not accepted");
            }

            [Fact]
            public async Task UserDoesNotHaveAction()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID
                };
                _securityService.Setup(_ => _.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(false);

                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddStopStatuses(stopStatuses);
                await action.Should().ThrowAsync<Exception>();
                _securityService.Verify(_ => _.UserHasActionAsync(It.Is<string[]>(actions =>
                    actions != null && actions.Length == 1
                        && actions[0] == SecurityActions.Loadshop_Ui_My_Loads_Status_Update
                )));
            }

            [Fact]
            public async Task UserHasWrongPrimaryScac()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID
                };
                _userContext.SetupGet(_ => _.UserId).Returns(INVALID_USER_ID);

                _svc = CreateService();

                Func<Task> action = async () => await _svc.AddStopStatuses(stopStatuses);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task LocationTimeIsNull()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID,
                    Events = new List<LoadStatusStopEventData>
                    {
                        new LoadStatusStopEventData
                        {
                            StopNumber = 1,
                            EventTime = null,
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };

                _svc = CreateService();

                var response = await _svc.AddStopStatuses(stopStatuses);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:Events:0:EventTime", "Status Date/Time is required");
            }

            [Fact]
            public async Task StopDoesntExist()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID,
                    Events = new List<LoadStatusStopEventData>
                    {
                        new LoadStatusStopEventData
                        {
                            StopNumber = 3,
                            EventTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };

                _svc = CreateService();

                var response = await _svc.AddStopStatuses(stopStatuses);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:Events:0", "Stop number does not exist");
            }

            [Fact]
            public async Task SubmitsCorrectInTransitTime()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID,
                    Events = new List<LoadStatusStopEventData>
                    {
                        new LoadStatusStopEventData
                        {
                            StopNumber = 1,
                            EventTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };
                _carrierWebApiService.Setup(_ => _.Send<StopEventData>(It.IsAny<LoadStatusEvent<StopEventData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddStopStatuses(stopStatuses);
                response.IsSuccess.Should().BeTrue();

                _carrierWebApiService.Verify(_ => _.Send<StopEventData>(It.Is<LoadStatusEvent<StopEventData>>(
                    e => e.MessageType == "LoadStopEvent"
                        && e.ApiVersion == "1.1"
                        && e.Payload.LoadNumber == VALID_REF_LOAD_ID
                        && e.Payload.StopNbr == 1
                        && e.Payload.StatusDateTime == "2020-02-11T10:00:00"
                        && e.Payload.IsLocalTime == true
                        && e.Payload.Scac == "TEST"
                    )));
            }

            [Fact]
            public async Task SubmitsPlatformPlusLoadId()
            {
                var load = VALID_LOAD;
                load.PlatformPlusLoadId = "PLATFORM_PLUS_ID";
                var loads = new List<LoadEntity> { load };
                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(loads)
                    .WithUsers(new List<UserEntity> { VALID_CARRIER_USER })
                    .Build();

                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID,
                    Events = new List<LoadStatusStopEventData>
                    {
                        new LoadStatusStopEventData
                        {
                            StopNumber = 1,
                            EventTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };
                _carrierWebApiService.Setup(_ => _.Send<StopEventData>(It.IsAny<LoadStatusEvent<StopEventData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddStopStatuses(stopStatuses);
                response.IsSuccess.Should().BeTrue();

                _carrierWebApiService.Verify(_ => _.Send<StopEventData>(It.Is<LoadStatusEvent<StopEventData>>(
                    e => e.MessageType == "LoadStopEvent"
                        && e.ApiVersion == "1.1"
                        && e.Payload.LoadNumber == "PLATFORM_PLUS_ID"
                        && e.Payload.StopNbr == 1
                        && e.Payload.StatusDateTime == "2020-02-11T10:00:00"
                        && e.Payload.IsLocalTime == true
                        && e.Payload.Scac == "TEST"
                    )));
            }

            [Fact]
            public async Task SubmitsBillingLoadId()
            {
                var load = VALID_LOAD;
                load.PlatformPlusLoadId = "PLATFORM_PLUS_ID";
                load.LatestLoadTransaction.Claim.BillingLoadId = "BILLING_LOAD_ID";
                var loads = new List<LoadEntity> { load };
                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(loads)
                    .WithUsers(new List<UserEntity> { VALID_CARRIER_USER })
                    .Build();

                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID,
                    Events = new List<LoadStatusStopEventData>
                    {
                        new LoadStatusStopEventData
                        {
                            StopNumber = 1,
                            EventTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };
                _carrierWebApiService.Setup(_ => _.Send<StopEventData>(It.IsAny<LoadStatusEvent<StopEventData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddStopStatuses(stopStatuses);
                response.IsSuccess.Should().BeTrue();

                _carrierWebApiService.Verify(_ => _.Send<StopEventData>(It.Is<LoadStatusEvent<StopEventData>>(
                    e => e.MessageType == "LoadStopEvent"
                        && e.ApiVersion == "1.1"
                        && e.Payload.LoadNumber == "BILLING_LOAD_ID"
                        && e.Payload.StopNbr == 1
                        && e.Payload.StatusDateTime == "2020-02-11T10:00:00"
                        && e.Payload.IsLocalTime == true
                        && e.Payload.Scac == "TEST"
                    )));
            }

            [Fact]
            public async Task PreventsFutureTimeForTimeZone()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID,
                    Events = new List<LoadStatusStopEventData>
                    {
                        new LoadStatusStopEventData
                        {
                            StopNumber = 1,
                            EventTime = new DateTimeOffset(DateTime.Now),//Assuming this is central time and Stop one is Pacific Time (Oregon)
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };
                _carrierWebApiService.Setup(_ => _.Send<StopEventData>(It.IsAny<LoadStatusEvent<StopEventData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddStopStatuses(stopStatuses);
                response.IsSuccess.Should().BeFalse();
                AssertHasError(response, $"urn:root:Events:0:EventTime", "Status Date/Time must be in the past");
            }

            [Fact]
            public async Task AddLoadStatusTransaction()
            {
                var stopStatuses = new LoadStatusStopData
                {
                    LoadId = VALID_LOAD_ID,
                    Events = new List<LoadStatusStopEventData>
                    {
                        new LoadStatusStopEventData
                        {
                            StopNumber = 1,
                            EventTime = new DateTimeOffset(new DateTime(2020, 2, 11, 10, 0, 0)),
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };
                _carrierWebApiService.Setup(_ => _.Send<StopEventData>(It.IsAny<LoadStatusEvent<StopEventData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });

                _svc = CreateService();

                var response = await _svc.AddStopStatuses(stopStatuses);
                response.IsSuccess.Should().BeTrue();

                _dbBuilder.MockLoadStatusTransactions.Verify(_ => _.Add(It.Is<LoadStatusTransactionEntity>(x =>
                    x.MessageId == Guid.Parse("44444444-4444-4444-4444-555555555555")
                    && x.MessageTime == new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    && x.LoadId == VALID_LOAD_ID
                    //can't verity the TransactionDtTm since it's using DateTime.Now
                )));
            }
        }

        public class AddInTransitStopStatusTests : BaseLoadStatusTransactionTests
        {

            public AddInTransitStopStatusTests(TestFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task Null()
            {
                _svc = CreateService();

                (await _svc.AddInTransitStopStatus(null, null)).Should().BeNull();
                (await _svc.AddInTransitStopStatus(new LoadStatusStopData(), null)).Should().BeNull();
                (await _svc.AddInTransitStopStatus(null, new LoadEntity())).Should().BeNull();
                (await _svc.AddInTransitStopStatus(new LoadStatusStopData(), new LoadEntity())).Should().BeNull();
            }

            [Fact]
            public async Task MostRecentStopIsNotDeparture()
            {
                var stopData = new LoadStatusStopData()
                {
                    Events = new List<LoadStatusStopEventData>()
                    {
                        new LoadStatusStopEventData()
                        {
                            StopNumber = 1,
                            EventType = StopEventTypeEnum.Departure
                        },
                        new LoadStatusStopEventData()
                        {
                            StopNumber = 2,
                            EventType = StopEventTypeEnum.Arrival
                        }
                    }
                };

                var load = new LoadEntity()
                {
                    LoadId = VALID_LOAD_ID,
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 2,
                            Latitude = 10,
                            Longitude = 20
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 3,
                            Latitude = 20,
                            Longitude = 30
                        }
                    }
                };

                _svc = CreateService();

                (await _svc.AddInTransitStopStatus(stopData, load)).Should().BeNull();
            }

            [Fact]
            public async Task Success()
            {
                var stopData = new LoadStatusStopData()
                {
                    Events = new List<LoadStatusStopEventData>()
                    {
                        new LoadStatusStopEventData()
                        {
                            StopNumber = 1,
                            EventType = StopEventTypeEnum.Arrival
                        },
                        new LoadStatusStopEventData()
                        {
                            StopNumber = 2,
                            EventType = StopEventTypeEnum.Departure,
                            EventTime = DateTime.Now
                        }
                    }
                };

                var load = new LoadEntity()
                {
                    LoadId = VALID_LOAD_ID,
                    LoadStops = new List<LoadStopEntity>()
                    {
                        new LoadStopEntity()
                        {
                            StopNbr = 2,
                            Latitude = 10,
                            Longitude = 20
                        },
                        new LoadStopEntity()
                        {
                            StopNbr = 3,
                            Latitude = 20,
                            Longitude = 30
                        }
                    }
                };

                _carrierWebApiService.Setup(_ => _.Send<InTransitLoadData>(It.IsAny<LoadStatusEvent<InTransitLoadData>>()))
                    .ReturnsAsync(new LoadStatusEvent<CarrierApiResponseMessages>
                    {
                        MessageId = Guid.Parse("44444444-4444-4444-4444-555555555555"),
                        MessageTime = new DateTimeOffset(new DateTime(2020, 2, 11, 12, 13, 14))
                    });
                _svc = CreateService();

                var response = await _svc.AddInTransitStopStatus(stopData, load);
                response.Should().NotBeNull();
                response.LoadId.Should().Be(load.LoadId);
                response.LocationTime.Should().BeSameDateAs(stopData.Events[1].EventTime.Value);
                response.Latitude.Should().Be(load.LoadStops[0].Latitude);
                response.Longitude.Should().Be(load.LoadStops[0].Longitude);
            }
        }
    }
}
