using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Validation.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;
using System.Net.Http;
using Newtonsoft.Json;
using Moq.Contrib.HttpClient;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class SmartSpotPriceServiceTests
    {
        public class GetSmartSpotPrice : IClassFixture<TestFixture>
        {
            private SmartSpotPriceConfig _config;
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private Mock<HttpMessageHandler> _httpHandler;
            private ISmartSpotPriceService _svc;
            private List<LoadshopSmartSpotPriceRequest> _requests;
            private List<AWSSmartSpotPriceRequest> _awsRequests;
            private Mock<IUserContext> _userContext;
            private Mock<IRecaptchaService> _recaptchaService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISecurityService> _securityService;
            private readonly Mock<IShippingService> _shippingService;
            private readonly Mock<ILoadCarrierGroupService> _loadCarrierGroupService;

            private const decimal EXPECTED_SPOT_PRICE_0 = 1.99M;
            private const decimal EXPECTED_SPOT_PRICE_1 = 99.10M;
            private const decimal EXPECTED_GUARD_RATE_0 = 1.98M; // Default to lower than AWS rate
            private const decimal EXPECTED_GUARD_RATE_1 = 100M; // Default to higher than AWS rate
            private static readonly Guid LOAD_ID_0 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly Guid LOAD_ID_1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            private static readonly DateTime PICKUP_DATE = new DateTime(2020, 1, 1);
            private static readonly DateTime DELIVERY_DATE = new DateTime(2020, 1, 2);

            private string URL = "https://www.smartspotprice.com/doit";

            private readonly List<CarrierScacEntity> CARRIER_SCACS = new List<CarrierScacEntity>
            {
                new CarrierScacEntity
                {
                    Scac = "OPRT",
                    CarrierId = "1800PACKRAT"
                },
                new CarrierScacEntity
                {
                    Scac = "CARRIER2",
                    CarrierId = "CARRIER TWO"
                }
            };
            private readonly LoadEntity LOAD_0 = new LoadEntity
            {
                LoadId = LOAD_ID_0,
                LatestTransactionTypeId = "PendingNew",
                Miles = 15,
                Weight = 100,
                DirectMiles = 15,
                Stops = 2,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID_0,
                        LateDtTm = PICKUP_DATE,
                        StopTypeId = (int)StopTypeEnum.Pickup,
                        State = "WI",
                        PostalCode = "53123",
                        StopNbr = 1
                    },
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID_0,
                        LateDtTm = DELIVERY_DATE,
                        StopTypeId = (int)StopTypeEnum.Delivery,
                        State = "WI",
                        PostalCode = "53123",
                        StopNbr = 2
                    }
                },
                CarrierScacs = new List<LoadCarrierScacEntity>()
            };
            private readonly LoadEntity LOAD_1 = new LoadEntity
            {
                LoadId = LOAD_ID_1,
                LatestTransactionTypeId = "PendingNew",
                Miles = 200,
                Weight = 1000,
                DirectMiles = 200,
                Stops = 2,
                LoadStops = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID_1,
                        LateDtTm = PICKUP_DATE,
                        StopTypeId = (int)StopTypeEnum.Pickup,
                        State = "WI",
                        PostalCode = "53123",
                        StopNbr = 1
                    },
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID_1,
                        LateDtTm = DELIVERY_DATE,
                        StopTypeId = (int)StopTypeEnum.Delivery,
                        State = "WI",
                        PostalCode = "53123",
                        StopNbr = 2
                    }
                },
                CarrierScacs = new List<LoadCarrierScacEntity>()
            };

            public GetSmartSpotPrice(TestFixture fixture)
            {
                _config = new SmartSpotPriceConfig
                {
                    ApiUrl = URL,
                    AccessKeyId = "access-key-id",
                    SecretAccessKey = "secret-access-key",
                    Service = "service-name",
                    Region = "us-east-1"
                };
                _db = new MockDbBuilder()
                    .WithLoad(LOAD_0)
                    .WithLoad(LOAD_1)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                _db.SetupSequence(x => x.GetDATGuardRate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                    .Returns(EXPECTED_GUARD_RATE_0)
                    .Returns(EXPECTED_GUARD_RATE_1);

                _mapper = fixture.Mapper;
                var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new AWSSmartSpotPriceResponse { Results = new List<decimal> { EXPECTED_SPOT_PRICE_0, EXPECTED_SPOT_PRICE_1 } }))
                };

                _httpHandler = new Mock<HttpMessageHandler>();
                _httpHandler.SetupRequest(HttpMethod.Post, URL)
                        .ReturnsAsync(expectedResponse);               

                _userContext = new Mock<IUserContext>();
                _recaptchaService = new Mock<IRecaptchaService>();

                _mileageService = new Mock<IMileageService>();
                _securityService = new Mock<ISecurityService>();
                _shippingService = new Mock<IShippingService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();

                InitService();

                _requests = new List<LoadshopSmartSpotPriceRequest>
                {
                    new LoadshopSmartSpotPriceRequest
                    {
                        LoadId = LOAD_ID_0,
                        Weight = 1,
                        Commodity = "Beer",
                        EquipmentId = "Equipment",
                        CarrierIds = new List<string> { "1800PACKRAT", "CARRIER2" }
                    },
                    new LoadshopSmartSpotPriceRequest
                    {
                        LoadId = LOAD_ID_1,
                        Weight = 1000,
                        Commodity = "Beer",
                        EquipmentId = "Equipment",
                        CarrierIds = new List<string> { "1800PACKRAT" }
                    }
                };

                _awsRequests = new List<AWSSmartSpotPriceRequest>
                {
                    new AWSSmartSpotPriceRequest
                    {
                        LoadId = Guid.Empty,
                        TransactionCreate = DateTime.Now,
                        TransactionTypeId = "New",
                        LoadShopMiles = 1,
                        DirectMiles = 1,
                        Stops = 2,
                        Weight = 1,
                        EquipmentId = "Equipment",
                        PkupDate = DateTime.Now.AddDays(2),
                        OrigState = "WI",
                        OriginZip = "54130",
                        O3Zip = "541",
                        DestState = "IL",
                        DestZip = "60611",
                        D3Zip = "606",
                        NbrSCACsRequest = 1,
                        NbrCarriersRequest = 1,
                        NbrSCACsPosted = 0,
                        NbrContractSCACsPosted = 0,
                        NbrSCACsHidden = 0
                    }
                };
            }

            private void InitService()
            {

                var http = _httpHandler.CreateClient();
                _svc = new SmartSpotPriceService(_config, 
                    _db.Object, 
                    _mapper, 
                    http,
                    _userContext.Object,
                    _recaptchaService.Object,
                    _mileageService.Object,
                    _securityService.Object,
                    _loadCarrierGroupService.Object,
                    _shippingService.Object);
            }

            [Fact]
            public void MissingAPIUrlConfigSetting_ThrowsException()
            {
                _config.ApiUrl = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotPricesAsync(It.IsAny<List<LoadshopSmartSpotPriceRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing API URL Config Setting");
            }

            [Fact]
            public void MissingAccessKeyIdConfigSetting_ThrowsException()
            {
                _config.AccessKeyId = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotPricesAsync(It.IsAny<List<LoadshopSmartSpotPriceRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Access Key Id Config Setting");
            }

            [Fact]
            public void MissingSecretAccessKeyConfigSetting_ThrowsException()
            {
                _config.SecretAccessKey = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotPricesAsync(It.IsAny<List<LoadshopSmartSpotPriceRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Secret Access Key Config Setting");
            }

            [Fact]
            public void MissingRegionConfigSetting_ThrowsException()
            {
                _config.Region = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotPricesAsync(It.IsAny<List<LoadshopSmartSpotPriceRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Region Config Setting");
            }

            [Fact]
            public void MissingServiceConfigSetting_ThrowsException()
            {
                _config.Service = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotPricesAsync(It.IsAny<List<LoadshopSmartSpotPriceRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Service Config Setting");
            }

            [Fact]
            public void MissingRequest_ThrowsException()
            {
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(default(List<LoadshopSmartSpotPriceRequest>)))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Requests are required");
            }

            [Fact]
            public void MissingLoadId_ThrowsException()
            {
                var request = new List<LoadshopSmartSpotPriceRequest> {
                    new LoadshopSmartSpotPriceRequest()
                };
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*LoadId is required");
            }

            [Fact]
            public void MissingCommodity_ThrowsException()
            {
                var request = _requests;
                request[0].Commodity = null;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Commodity is required");
            }

            [Fact]
            public void MissingLoad_ThrowsException()
            {
                var load = LOAD_0;
                load.LoadId = Guid.Empty;
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Unable to find load with ID*");
            }

            [Fact]
            public void LoadMissingPickupStop_ThrowsException()
            {
                var load = LOAD_0;
                load.LoadStops.RemoveAt(0);
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Unable to find origin pickup stop*");
            }

            [Fact]
            public void LoadMissingDeliveryStop_ThrowsException()
            {
                var load = LOAD_0;
                load.LoadStops.RemoveAt(1);
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Unable to find destination delivery stop*");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            public void LoadMissingTransactionType_ThrowsException(string value)
            {
                var load = LOAD_0;
                load.LatestTransactionTypeId = value;
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*TransactionTypeId is required");
            }

            [Fact]
            public void LoadMissingPickupDate_ThrowsException()
            {
                var load = LOAD_0;
                load.LoadStops[0].LateDtTm = DateTime.MinValue;
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*PkupDate is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            public void LoadMissingOriginState_ThrowsException(string value)
            {
                var load = LOAD_0;
                load.LoadStops[0].State = value;
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*OrigState is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            [InlineData("12")] // Not long enough for 3 digit zip
            public void LoadMissingOriginPostalCode_ThrowsException(string value)
            {
                var load = LOAD_0;
                load.LoadStops[0].PostalCode = value;
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*O3Zip is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            public void LoadMissingDestinationState_ThrowsException(string value)
            {
                var load = LOAD_0;
                load.LoadStops[1].State = value;
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*DestState is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            [InlineData("12")] // Not long enough for 3 digit zip
            public void LoadMissingDestinationPostalCode_ThrowsException(string value)
            {
                var load = LOAD_0;
                load.LoadStops[1].PostalCode = value;
                _db = new MockDbBuilder()
                    .WithLoad(load)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
                InitService();

                var request = _requests;
                request.RemoveAt(1); // Limit to requesting a single load's price for validation tests
                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*D3Zip is required");
            }

            [Fact]
            public void SmartSpotPriceAPIReturnsError_ThrowsException()
            {
                var request = _requests;
                var expectedException = new Exception("API ERROR");
                var msg = "blah blah blah";
                var expectedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(msg)
                };
                _httpHandler = new Mock<HttpMessageHandler>();
                _httpHandler.SetupRequest(HttpMethod.Post, URL)
                        .ReturnsAsync(expectedResponse);

                InitService();

                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage($"Smart Spot Price Service Error: {msg}");
            }

            [Fact]
            public async Task ReturnsSpotPrice()
            {
                var request = _requests;
                var response = await _svc.GetSmartSpotPricesAsync(request);

                response.Should().NotBeNull();
                response.Should().HaveCount(2);
                response[0].LoadId.Should().Be(request[0].LoadId);
                response[0].Price.Should().Be(EXPECTED_GUARD_RATE_0);
                response[0].DATGuardRate.Should().Be(EXPECTED_GUARD_RATE_0);
                response[0].MachineLearningRate.Should().Be(EXPECTED_SPOT_PRICE_0);
                response[1].LoadId.Should().Be(request[1].LoadId);
                response[1].Price.Should().Be(EXPECTED_SPOT_PRICE_1);
                response[1].DATGuardRate.Should().Be(EXPECTED_GUARD_RATE_1);
                response[1].MachineLearningRate.Should().Be(EXPECTED_SPOT_PRICE_1);
            }

            [Fact]
            public void SmartSpotPriceAPIReturnsWrongNumberOfResults_ThrowsException()
            {
                var request = _requests;

                var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new AWSSmartSpotPriceResponse { Results = new List<decimal> { EXPECTED_SPOT_PRICE_0 } }))
                };

                _httpHandler = new Mock<HttpMessageHandler>();
                _httpHandler.SetupRequest(HttpMethod.Post, URL)
                        .ReturnsAsync(expectedResponse);

                InitService();

                _svc.Awaiting(async x => await x.GetSmartSpotPricesAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage($"*{expectedResponse.Content}");
            }
        }

        public class GetSmartSpotQuote : IClassFixture<TestFixture>
        {
            private SmartSpotPriceConfig _config;
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private Mock<HttpMessageHandler> _httpHandler;
            private ISmartSpotPriceService _svc;
            private RecaptchaRequest<LoadshopSmartSpotQuoteRequest> _request;
            private Mock<IUserContext> _userContext;
            private Mock<IRecaptchaService> _recaptchaService;
            private Mock<IMileageService> _mileageService;
            private Mock<ISecurityService> _securityService;
            private const decimal EXPECTED_SPOT_PRICE_0 = 1.99M;
            private static readonly DateTime PICKUP_DATE = new DateTime(2020, 1, 1);
            private readonly Mock<IShippingService> _shippingService;
            private readonly Mock<ILoadCarrierGroupService> _loadCarrierGroupService;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string USER_NAME = "TestUser";
            public string URL = "https://www.smartspotprice.com/doit";

            private static List<CarrierData> CARRIERS => new List<CarrierData>
            {
                new CarrierData { CarrierScacs = new List<string> { "1", "2" } },
                new CarrierData { CarrierScacs = new List<string> { "2", "3" } },
                new CarrierData { CarrierScacs = new List<string> { "8", "9" } }
            };

            protected void AssertHasError(BaseServiceResponse response, string urn, string message)
            {
                response.ModelState.Should().Contain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
            }

            protected void AssertNoError(BaseServiceResponse response, string urn, string message)
            {
                response.ModelState.Should().NotContain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
            }

            public GetSmartSpotQuote(TestFixture fixture)
            {
                _config = new SmartSpotPriceConfig
                {
                    ApiUrl = URL,
                    AccessKeyId = "access-key-id",
                    SecretAccessKey = "secret-access-key",
                    Service = "service-name",
                    Region = "us-east-1"
                };
                _db = new MockDbBuilder()
                    .Build();
                _mapper = fixture.Mapper;
                _httpHandler = new Mock<HttpMessageHandler>();

                var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new AWSSmartSpotPriceResponse { Results = new List<decimal> { EXPECTED_SPOT_PRICE_0 } }))
                };

                _httpHandler.SetupRequest(HttpMethod.Post, URL)
                        .ReturnsAsync(expectedResponse);

                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(_ => _.UserId).Returns(USER_ID);
                _userContext.SetupGet(_ => _.UserName).Returns(USER_NAME);

                _recaptchaService = new Mock<IRecaptchaService>();

                _mileageService = new Mock<IMileageService>();
                _mileageService.Setup(_ => _.GetDirectMiles(It.IsAny<MileageRequestData>())).Returns(100);

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(_ => _.GetContractedCarriersByPrimaryCustomerIdAsync()).ReturnsAsync(CARRIERS.AsReadOnly());
                _shippingService = new Mock<IShippingService>();
                _loadCarrierGroupService = new Mock<ILoadCarrierGroupService>();

                InitService();

                _request = new RecaptchaRequest<LoadshopSmartSpotQuoteRequest>
                {
                    Token = "RECAPTCHA_TOKEN",
                    Data = new LoadshopSmartSpotQuoteRequest
                    {
                        OriginCity = "Mosinee",
                        OriginState = "WI",
                        OriginPostalCode = "54455",
                        OriginCountry = "USA",
                        DestinationCity = "Stevens Point",
                        DestinationState = "WI",
                        DestinationPostalCode = "54481",
                        DestinationCountry = "USA",
                        EquipmentId = "53TF102",
                        Weight = 1000,
                        PickupDate = new DateTime(2020, 02, 01)
                    }
                };
            }

            private void InitService()
            {
                var http = _httpHandler.CreateClient();
                _svc = new SmartSpotPriceService(_config, 
                    _db.Object,
                    _mapper, 
                    http,
                    _userContext.Object, 
                    _recaptchaService.Object,
                    _mileageService.Object,
                    _securityService.Object,
                    _loadCarrierGroupService.Object,
                    _shippingService.Object);
            }

            [Fact]
            public void MissingAPIUrlConfigSetting_ThrowsException()
            {
                _config.ApiUrl = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(It.IsAny<RecaptchaRequest<LoadshopSmartSpotQuoteRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing API URL Config Setting");
            }

            [Fact]
            public void MissingAccessKeyIdConfigSetting_ThrowsException()
            {
                _config.AccessKeyId = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(It.IsAny<RecaptchaRequest<LoadshopSmartSpotQuoteRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Access Key Id Config Setting");
            }

            [Fact]
            public void MissingSecretAccessKeyConfigSetting_ThrowsException()
            {
                _config.SecretAccessKey = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(It.IsAny<RecaptchaRequest<LoadshopSmartSpotQuoteRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Secret Access Key Config Setting");
            }

            [Fact]
            public void MissingRegionConfigSetting_ThrowsException()
            {
                _config.Region = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(It.IsAny<RecaptchaRequest<LoadshopSmartSpotQuoteRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Region Config Setting");
            }

            [Fact]
            public void MissingServiceConfigSetting_ThrowsException()
            {
                _config.Service = null;
                InitService();

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(It.IsAny<RecaptchaRequest<LoadshopSmartSpotQuoteRequest>>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Missing Service Config Setting");
            }

            [Fact]
            public void MissingRequest_ThrowsException()
            {
                _svc.Awaiting(async x => await x.GetSmartSpotQuoteAsync(null))
                    .Should()
                    .Throw<Exception>();
            }

            [Fact]
            public void MissingRequestData_ThrowsException()
            {
                _request.Data = null;
                _svc.Awaiting(async x => await x.GetSmartSpotQuoteAsync(_request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Request is required");
            }

            [Fact]
            public async Task MissingPickupDate_ThrowsException()
            {
                InitService();

                var request = _request;
                request.Data.PickupDate = null;
                var response = await _svc.GetSmartSpotQuoteAsync(request);
                AssertHasError(response, "urn:root:PickupDate", "Date is required");
            }

            [Fact]
            public async Task MinPickupDate_ThrowsException()
            {
                InitService();

                var request = _request;
                request.Data.PickupDate = DateTimeOffset.MinValue;
                var response = await _svc.GetSmartSpotQuoteAsync(request);
                AssertHasError(response, "urn:root:PickupDate", "Date is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            public async Task MissingOriginState_ThrowsException(string value)
            {
                InitService();

                var request = _request;
                request.Data.OriginState = value;
                var response = await _svc.GetSmartSpotQuoteAsync(request);
                AssertHasError(response, "urn:root:OriginState", "Origin State is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            public async Task MissingOriginPostalCode_ThrowsException(string value)
            {
                InitService();

                var request = _request;
                request.Data.OriginPostalCode = value;
                var response = await _svc.GetSmartSpotQuoteAsync(request);
                AssertHasError(response, "urn:root:OriginPostalCode", "Origin Postal Code is required");
            }

            [Fact]
            public void ShortOriginPostalCode_ThrowsException()
            {
                InitService();

                var request = _request;
                request.Data.OriginPostalCode = "12";

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*O3Zip is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            public async Task MissingDestinationState_ThrowsException(string value)
            {
                InitService();

                var request = _request;
                request.Data.DestinationState = value;
                var response = await _svc.GetSmartSpotQuoteAsync(request);
                AssertHasError(response, "urn:root:DestinationState", "Destination State is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData("  ")]
            public async Task MissingDestinationPostalCode_ThrowsException(string value)
            {
                InitService();

                var request = _request;
                request.Data.DestinationPostalCode = value;
                var response = await _svc.GetSmartSpotQuoteAsync(request);
                AssertHasError(response, "urn:root:DestinationPostalCode", "Destination Postal Code is required");
            }

            [Fact]
            public void ShortDestinationPostalCode_ThrowsException()
            {
                InitService();

                var request = _request;
                request.Data.DestinationPostalCode = "12";

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*D3Zip is required");
            }

            [Theory]
            [InlineData(null)]
            [InlineData(-1)]
            [InlineData(0)]
            public async Task MissingWeight_ThrowsException(int? value)
            {
                InitService();

                var request = _request;
                request.Data.Weight = value;
                var response = await _svc.GetSmartSpotQuoteAsync(request);
                AssertHasError(response, "urn:root:Weight", "Weight is required");
            }

            [Fact]
            public void SmartSpotPriceAPIReturnsError_ThrowsException()
            {
                var request = _request;
                var msg = "API ERROR";
                var expectedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(msg)
                };

                _httpHandler = new Mock<HttpMessageHandler>();
                _httpHandler.SetupRequest(HttpMethod.Post, URL)
                        .ReturnsAsync(expectedResponse);

                InitService();

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage($"Smart Spot Price Service Error: {msg}");
            }

            [Fact]
            public async Task ReturnsSpotPrice()
            {
                var request = _request;
                var response = await _svc.GetSmartSpotQuoteAsync(request);

                response.Data.Should().Be(EXPECTED_SPOT_PRICE_0);
            }

            [Fact]
            public void SmartSpotPriceAPIReturnsWrongNumberOfResults_ThrowsException()
            {
                var request = _request;

                var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new AWSSmartSpotPriceResponse { Results = new List<decimal> {  } }))
                };

                _httpHandler = new Mock<HttpMessageHandler>();
                _httpHandler.SetupRequest(HttpMethod.Post, URL)
                        .ReturnsAsync(expectedResponse);

                InitService();

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage($"*{expectedResponse.Content}");
            }

            [Fact]
            public void RecaptchaServiceVerifyThrowsException()
            {
                var exception = new Exception();
                _recaptchaService.Setup(_ => _.ValidateToken(It.IsAny<RecaptchaRequest<LoadshopSmartSpotQuoteRequest>>())).ThrowsAsync(exception);

                _svc.Awaiting(x => x.GetSmartSpotQuoteAsync(_request))
                    .Should()
                    .Throw<Exception>()
                    .And
                    .Should()
                    .Be(exception);
            }
        }
    }
}
