using FluentAssertions;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Data.Google;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TMS.Infrastructure.Common.Configuration;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class MileageServiceTests
    {
        public class GetDirectMilesTests
        {
            internal Mock<ILogger<MileageService>> _logger;
            private readonly HttpClient _httpClient;
            private readonly Mock<HttpMessageHandler> _mockHttpHandler;
            private Mock<IConfigurationRoot> _config;
            private MileageService _svc;

            public GetDirectMilesTests()
            {
                _logger = new Mock<ILogger<MileageService>>();
                _mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                _httpClient = new HttpClient(_mockHttpHandler.Object);
            }

            private MileageService CreateService()
            {
                _config = new Mock<IConfigurationRoot>();
                _config.SetupGet(x => x["PcMilerAddress"]).Returns("http://testing.test/");

                return new MileageService(_logger.Object, _config.Object, _httpClient);
            }

            [Fact]
            public void GetDirectMilesTests_Null()
            {
                _svc = CreateService();
                _svc.GetDirectMiles(null).Should().Be(0);
            }
        }

        public class GetDirectRouteMilesTests
        {
            internal Mock<ILogger<MileageService>> _logger;
            private readonly HttpClient _httpClient;
            private readonly Mock<HttpMessageHandler> _mockHttpHandler;
            private readonly Mock<IConfigurationRoot> _mockConfig;
            private MileageService _svc;

            public GetDirectRouteMilesTests()
            {
                _logger = new Mock<ILogger<MileageService>>();
                _mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                MockHttpResponse(JsonConvert.SerializeObject(new GoogleDirectionClass
                {
                }), HttpStatusCode.OK);
                _httpClient = new HttpClient(_mockHttpHandler.Object);

                _mockConfig = new Mock<IConfigurationRoot>();
                _mockConfig.SetupGet(x => x["PcMilerAddress"]).Returns("http://testing.test/");
                _mockConfig.SetupGet(x => x["GoogleDirectionsApiUrl"]).Returns("https://testing.com/directionsurl");
                _mockConfig.SetupGet(x => x["GoogleAPIKey"]).Returns("THE_API_KEY");

                _svc = new MileageService(_logger.Object, _mockConfig.Object, _httpClient);
            }

            private void MockHttpResponse(string response, HttpStatusCode statusCode)
            {
                _mockHttpHandler
                   .Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(response), })
                   .Verifiable();
            }

            [Fact]
            public async Task NullRequest()
            {
                (await _svc.GetDirectRouteMiles(null)).Should().Be(0);
            }

            [Fact]
            public async Task CallsApiWithExpectedValues()
            {
                await _svc.GetDirectRouteMiles(new MileageRequestData
                {
                    OriginCity = "O_City",
                    OriginState = "O_State",
                    OriginCountry = "O_Country",
                    OriginPostalCode = "O_PostalCode",
                    DestinationCity = "D_City",
                    DestinationState = "D_State",
                    DestinationCountry = "D_Country",
                    DestinationPostalCode = "D_PostalCode",
                    DefaultMiles = 0
                });

                var expectedUrl = "https://testing.com/directionsurl?" +
                    "origin=O_City,%20O_State%20O_PostalCode" +
                    "&destination=D_City,%20D_State%20D_PostalCode" +
                    "&key=THE_API_KEY";
                _mockHttpHandler.Protected().Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(_ => _.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
            }

            [Fact]
            public async Task EmptyResponse()
            {
                var response = await _svc.GetDirectRouteMiles(new MileageRequestData
                {
                    OriginCity = "O_City",
                    OriginState = "O_State",
                    OriginCountry = "O_Country",
                    OriginPostalCode = "O_PostalCode",
                    DestinationCity = "D_City",
                    DestinationState = "D_State",
                    DestinationCountry = "D_Country",
                    DestinationPostalCode = "D_PostalCode",
                    DefaultMiles = 0
                });

                response.Should().Be(0);
            }

            [Fact]
            public async Task ReturnsCorrectMileage()
            {
                MockHttpResponse(JsonConvert.SerializeObject(new GoogleDirectionClass
                {
                    routes = new List<Route>
                    {
                        new Route
                        {
                            legs = new List<Leg>
                            {
                                new Leg
                                {
                                    distance = new Distance
                                    {
                                        value = 336877//value in meters
                                    }
                                }
                            }
                        }
                    }
                }), HttpStatusCode.OK);
                var response = await _svc.GetDirectRouteMiles(new MileageRequestData
                {
                    OriginCity = "O_City",
                    OriginState = "O_State",
                    OriginCountry = "O_Country",
                    OriginPostalCode = "O_PostalCode",
                    DestinationCity = "D_City",
                    DestinationState = "D_State",
                    DestinationCountry = "D_Country",
                    DestinationPostalCode = "D_PostalCode",
                    DefaultMiles = 0
                });

                response.Should().Be(209);
            }

            [Fact]
            public void BadRequest()
            {
                MockHttpResponse("", HttpStatusCode.BadRequest);
                var request = new MileageRequestData
                {
                    OriginCity = "O_City",
                    OriginState = "O_State",
                    OriginCountry = "O_Country",
                    OriginPostalCode = "O_PostalCode",
                    DestinationCity = "D_City",
                    DestinationState = "D_State",
                    DestinationCountry = "D_Country",
                    DestinationPostalCode = "D_PostalCode",
                    DefaultMiles = 0
                };
                _svc.Awaiting(x => x.GetDirectRouteMiles(request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Bad Request*");
            }
        }

        public class GetRouteMilesTests
        {
            internal Mock<ILogger<MileageService>> _logger;
            private readonly HttpClient _httpClient;
            private readonly Mock<HttpMessageHandler> _mockHttpHandler;
            private readonly Mock<IConfigurationRoot> _mockConfig;
            private MileageService _svc;

            public GetRouteMilesTests()
            {
                _logger = new Mock<ILogger<MileageService>>();
                _mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                MockHttpResponse(JsonConvert.SerializeObject(new GoogleDirectionClass
                {
                }), HttpStatusCode.OK);
                _httpClient = new HttpClient(_mockHttpHandler.Object);

                _mockConfig = new Mock<IConfigurationRoot>();
                _mockConfig.SetupGet(x => x["PcMilerAddress"]).Returns("http://testing.test/");
                _mockConfig.SetupGet(x => x["GoogleDirectionsApiUrl"]).Returns("https://testing.com/directionsurl");
                _mockConfig.SetupGet(x => x["GoogleAPIKey"]).Returns("THE_API_KEY");

                _svc = new MileageService(_logger.Object, _mockConfig.Object, _httpClient);
            }

            private void MockHttpResponse(string response, HttpStatusCode statusCode)
            {
                _mockHttpHandler
                   .Protected()
                   .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(response), })
                   .Verifiable();
            }

            [Fact]
            public void NullRequest()
            {
                _svc.Awaiting(x => x.GetRouteMiles(null))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("Invalid number of stops. Must provide 2 or more stops.");
            }

            [Fact]
            public void NoStops()
            {
                _svc.Awaiting(x => x.GetRouteMiles(new List<LoadStopData>()))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("Invalid number of stops. Must provide 2 or more stops.");
            }
            [Fact]
            public void TooFewStops()
            {
                _svc.Awaiting(x => x.GetRouteMiles(new List<LoadStopData> { new LoadStopData() }))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("Invalid number of stops. Must provide 2 or more stops.");
            }

            [Fact]
            public async Task CallsApiWithExpectedValues()
            {
                var loadStopData = new List<LoadStopData>
                {
                    new LoadStopData { Address1 = "123 Some St", City = "Some City", State = "ZY", PostalCode = "12345" },
                    new LoadStopData { Address1 = "456 Another Ln", City = "Another City", State = "AB", PostalCode = "67890" }
                };

                await _svc.GetRouteMiles(loadStopData);

                var expectedUrl = "https://testing.com/directionsurl?" +
                    "origin=123%20Some%20St,%20Some%20City,%20ZY%2012345" +
                    "&destination=456%20Another%20Ln,%20Another%20City,%20AB%2067890" +
                    "&key=THE_API_KEY";
                _mockHttpHandler.Protected().Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(_ => _.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
            }

            [Fact]
            public async Task EmptyResponse()
            {
                var loadStopData = new List<LoadStopData>
                {
                    new LoadStopData { Address1 = "123 Some St", City = "Some City", State = "ZY", PostalCode = "12345" },
                    new LoadStopData { Address1 = "456 Another Ln", City = "Another City", State = "AB", PostalCode = "67890" }
                };

                var response = await _svc.GetRouteMiles(loadStopData);

                response.Should().Be(0);
            }

            [Fact]
            public async Task TwoStopReturnsCorrectMileage()
            {
                MockHttpResponse(JsonConvert.SerializeObject(new GoogleDirectionClass
                {
                    routes = new List<Route>
                    {
                        new Route
                        {
                            legs = new List<Leg>
                            {
                                new Leg
                                {
                                    distance = new Distance
                                    {
                                        value = 336877//value in meters
                                    }
                                }
                            }
                        }
                    }
                }), HttpStatusCode.OK);

                var loadStopData = new List<LoadStopData>
                {
                    new LoadStopData { Address1 = "123 Some St", City = "Some City", State = "ZY", PostalCode = "12345" },
                    new LoadStopData { Address1 = "456 Another Ln", City = "Another City", State = "AB", PostalCode = "67890" }
                };

                var response = await _svc.GetRouteMiles(loadStopData);

                response.Should().Be(209);
            }

            [Fact]
            public async Task MultistopCallsApiWithExpectedValues()
            {
                var loadStopData = new List<LoadStopData>
                {
                    new LoadStopData { Address1 = "123 Some St", City = "Some City", State = "ZY", PostalCode = "12345" },
                    new LoadStopData { Address1 = "987 Two St", City = "Two City", State = "TW", PostalCode = "22222" },
                    new LoadStopData { Address1 = "654 Three St", City = "Three City", State = "TH", PostalCode = "33333" },
                    new LoadStopData { Address1 = "456 Another Ln", City = "Another City", State = "AB", PostalCode = "67890" }
                };

                await _svc.GetRouteMiles(loadStopData);

                var expectedUrl = "https://testing.com/directionsurl?" +
                    "origin=123%20Some%20St,%20Some%20City,%20ZY%2012345" +
                    "&destination=456%20Another%20Ln,%20Another%20City,%20AB%2067890" +
                    "&waypoints=optimize:true|" +
                    "987%20Two%20St,%20Two%20City,%20TW%2022222|" +
                    "654%20Three%20St,%20Three%20City,%20TH%2033333" +
                    "&key=THE_API_KEY";
                _mockHttpHandler.Protected().Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(_ => _.RequestUri == new Uri(expectedUrl)),
                    ItExpr.IsAny<CancellationToken>());
            }

            [Fact]
            public async Task MultistopStopReturnsCorrectMileage()
            {
                MockHttpResponse(JsonConvert.SerializeObject(new GoogleDirectionClass
                {
                    routes = new List<Route>
                    {
                        new Route
                        {
                            legs = new List<Leg>
                            {
                                new Leg { distance = new Distance { value = 111111 /*value in meters*/ } },
                                new Leg { distance = new Distance { value = 222222 /*value in meters*/ } },
                                new Leg { distance = new Distance { value = 333333 /*value in meters*/ } },
                            }
                        }
                    }
                }), HttpStatusCode.OK);

                var loadStopData = new List<LoadStopData>
                {
                    new LoadStopData { Address1 = "123 Some St", City = "Some City", State = "ZY", PostalCode = "12345" },
                    new LoadStopData { Address1 = "987 Two St", City = "Two City", State = "TW", PostalCode = "22222" },
                    new LoadStopData { Address1 = "654 Three St", City = "Three City", State = "TH", PostalCode = "33333" },
                    new LoadStopData { Address1 = "456 Another Ln", City = "Another City", State = "AB", PostalCode = "67890" }
                };

                var response = await _svc.GetRouteMiles(loadStopData);

                response.Should().Be(414);
            }

            [Fact]
            public void BadRequest()
            {
                MockHttpResponse("", HttpStatusCode.BadRequest);
                var loadStopData = new List<LoadStopData>
                {
                    new LoadStopData { Address1 = "123 Some St", City = "Some City", State = "ZY", PostalCode = "12345" },
                    new LoadStopData { Address1 = "456 Another Ln", City = "Another City", State = "AB", PostalCode = "67890" }
                };
                _svc.Awaiting(x => x.GetRouteMiles(loadStopData))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("*Bad Request*");
            }
        }
    }
}
