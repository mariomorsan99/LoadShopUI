using FluentAssertions;
using Xunit;
using Loadshop.DomainServices.Validation.Services;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using System.Threading;
using System.Net;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Microsoft.Extensions.Configuration;
using Loadshop.DomainServices.Loadshop.DataProvider;
using System;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Validation.Data.Address;

namespace Loadshop.Tests.DomainServices.Validation
{
    public class AddressValidationServiceTests
    {
        internal static Mock<HttpMessageHandler> _mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        internal static void MockHttpResponse(
                string streetNumber = "", string route = "", string city = "",
                string county = "", string stateName = "", string stateAbbrev = "",
                string countryName = "", string countryCode = "", string postalCode = "")
        {
            var json = $@"
            {{
                ""results"": [
                {{
                    ""address_components"": [
                    {{
                        ""long_name"": ""{streetNumber}"",
                        ""short_name"": ""{streetNumber}"",
                        ""types"": [ ""street_number"" ]
                    }},
                    {{
                        ""long_name"": ""{route}"",
                        ""short_name"": ""{route}"",
                        ""types"": [ ""route"" ]
                    }},
                    {{
                        ""long_name"": ""{city}"",
                        ""short_name"": ""{city}"",
                        ""types"": [ ""locality"", ""political"" ]
                    }},
                    {{
                        ""long_name"": ""{county}"",
                        ""short_name"": ""{county}"",
                        ""types"": [ ""administrative_area_level_2"", ""political"" ]
                    }},
                    {{
                      ""long_name"": ""{stateName}"",
                      ""short_name"": ""{stateAbbrev}"",
                      ""types"": [ ""administrative_area_level_1"", ""political"" ]
                    }},
                    {{
                      ""long_name"": ""{countryName}"",
                      ""short_name"": ""{countryCode}"",
                      ""types"": [ ""country"", ""political"" ]
                    }},
                    {{
                      ""long_name"": ""{postalCode}"",
                      ""short_name"": ""{postalCode}"",
                      ""types"": [ ""postal_code"" ]
                    }}
                    ],
                    ""formatted_address"": ""Staten Island, NY, USA"",
                    ""geometry"": {{
                        ""bounds"": {{
                            ""northeast"": {{
                                ""lat"": 40.6518121,
                                ""lng"": -74.0345471
                            }},
                            ""southwest"": {{
                                ""lat"": 40.4773991,
                                ""lng"": -74.25908989999999
                            }}
                        }},
                        ""location"": {{
                            ""lat"": 40.5795317,
                            ""lng"": -74.1502007
                        }},
                        ""location_type"": ""APPROXIMATE"",
                        ""viewport"": {{
                            ""northeast"": {{
                                ""lat"": 40.6518121,
                                ""lng"": -74.0345471
                            }},
                            ""southwest"": {{
                                ""lat"": 40.4773991,
                                ""lng"": -74.25908989999999
                            }}
                        }}
                    }},
                    ""place_id"": ""ChIJ59T0ee9FwokReLy6NIUfJ1A"",
                    ""types"": [ ""political"", ""sublocality"", ""sublocality_level_1"" ]
                }}],
                ""status"": ""OK""
            }}";

            MockHttpResponse(json, HttpStatusCode.OK);
        }

        internal static void MockHttpResponse(string response, HttpStatusCode statusCode)
        {
            _mockHttpHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage() { StatusCode = statusCode, Content = new StringContent(response), })
               .Verifiable();
        }

        public class IsAddressValidTest
        {
            private Mock<LoadshopDataContext> _db;
            private readonly HttpClient HttpClient;
            private readonly Mock<IConfigurationRoot> _mockConfig;

            private IAddressValidationService _svc;

            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private CustomerEntity CUSTOMER;

            public IsAddressValidTest()
            {
                MockHttpResponse("", HttpStatusCode.OK);
                HttpClient = new HttpClient(_mockHttpHandler.Object);

                _mockConfig = new Mock<IConfigurationRoot>();
                _mockConfig.SetupGet(x => x["AddressValidationEnabled"]).Returns("true");
                _mockConfig.SetupGet(x => x["GoogleGeocodingApiUrl"]).Returns("https://testing.com/googlegeocodingapiurl/");
                _mockConfig.SetupGet(x => x["GoogleApiKey"]).Returns("UNIT_TEST_API_KEY");

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
                    .WithCustomer(CUSTOMER)
                    .Build();
            }

            private void InitService()
            {
                _svc = new AddressValidationService(_db.Object, _mockConfig.Object, HttpClient);
            }

            [Fact]
            public void NullValues_False()
            {
                var stop = new LoadStopData
                {
                    Address1 = null,
                    City = null,
                    State = null,
                    Country = null,
                    PostalCode = null
                };
                _svc.IsAddressValid(CUSTOMER_ID, stop).Should().BeFalse();
            }

            [Fact]
            public void EmptyStrings_False()
            {
                var stop = new LoadStopData
                {
                    Address1 = string.Empty,
                    City = string.Empty,
                    State = string.Empty,
                    Country = string.Empty,
                    PostalCode = string.Empty
                };
                _svc.IsAddressValid(CUSTOMER_ID, stop).Should().BeFalse();
            }

            [Theory]
            [InlineData(null, null, "St Paul", "Minnesota", "MN", "United States", "US", "55108")]
            [InlineData("2230", "Energy Park Dr", "St Paul", "Minnesota", "MN", "United States", "US", "55108")]
            public void ValidAddresses_True(string streetNumber, string route, string city, string stateName, string stateAbbrev, string countryName, string countryCode, string postalCode)
            {
                MockHttpResponse(streetNumber, route, city, "", stateName, stateAbbrev, countryName, countryCode, postalCode);
                var stop = new LoadStopData
                {
                    Address1 = $"{streetNumber} {route}".Trim(),
                    City = city,
                    State = stateAbbrev,
                    Country = countryCode,
                    PostalCode = postalCode
                };
                _svc.IsAddressValid(CUSTOMER_ID, stop).Should().BeTrue();
            }

            [Fact]
            public void NoCustomer_True()
            {
                CUSTOMER.IdentUserId = default;
                InitDb();
                InitService();

                var stop = new LoadStopData(); // Stop doesn't have to be valid; will default to true
                _svc.IsAddressValid(CUSTOMER_ID, stop).Should().BeTrue();
            }

            [Fact]
            public void CustomerSetToNotValidateAddresses_True()
            {
                CUSTOMER.ValidateAddresses = false;
                InitDb();
                InitService();

                var stop = new LoadStopData(); // Stop doesn't have to be valid; will default to true
                _svc.IsAddressValid(CUSTOMER_ID, stop).Should().BeTrue();
            }

            private void InitSeedData()
            {
                CUSTOMER = new CustomerEntity
                {
                    IdentUserId = CUSTOMER_ID,
                    ValidateAddresses = true
                };
            }
        }

        public class GetValidAddressTest
        {
            private Mock<LoadshopDataContext> _db;
            private readonly HttpClient HttpClient;
            private readonly Mock<IConfigurationRoot> _mockConfig;

            private IAddressValidationService _svc;

            public GetValidAddressTest()
            {
                MockHttpResponse("", HttpStatusCode.OK);
                HttpClient = new HttpClient(_mockHttpHandler.Object);

                _mockConfig = new Mock<IConfigurationRoot>();
                _mockConfig.SetupGet(x => x["AddressValidationEnabled"]).Returns("true");
                _mockConfig.SetupGet(x => x["GoogleGeocodingApiUrl"]).Returns("https://testing.com/googlegeocodingapiurl/");
                _mockConfig.SetupGet(x => x["GoogleApiKey"]).Returns("UNIT_TEST_API_KEY");

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
                    .Build();
            }

            private void InitService()
            {
                _svc = new AddressValidationService(_db.Object, _mockConfig.Object, HttpClient);
            }

            [Fact]
            public void NullValues_NullValues()
            {
                var expected = new GeocodeAddress(item: null);
                var stop = new LoadStopData
                {
                    Address1 = null,
                    City = null,
                    State = null,
                    Country = null,
                    PostalCode = null
                };
                var actual = _svc.GetValidAddress(stop);
                actual.Should().BeEquivalentTo(expected);
            }

            [Fact]
            public void EmptyStrings_NullValues()
            {
                var expected = new GeocodeAddress(item: null);
                var stop = new LoadStopData
                {
                    Address1 = string.Empty,
                    City = string.Empty,
                    State = string.Empty,
                    Country = string.Empty,
                    PostalCode = string.Empty
                };
                var actual = _svc.GetValidAddress(stop);
                actual.Should().BeEquivalentTo(expected);
            }

            [Theory]
            [InlineData(null, null, "St Paul", "Minnesota", "MN", "United States", "US", "55108")]
            [InlineData("2230", "Energy Park Dr", "St Paul", "Minnesota", "MN", "United States", "US", "55108")]
            public void ValidAddress_MatchingGeocodeAddress(string streetNumber, string route, string city, string stateName, string stateAbbrev, string countryName, string countryCode, string postalCode)
            {
                MockHttpResponse(streetNumber, route, city, "", stateName, stateAbbrev, countryName, countryCode, postalCode);
                var stop = new LoadStopData
                {
                    Address1 = $"{streetNumber} {route}".Trim(),
                    City = city,
                    State = stateAbbrev,
                    Country = countryCode,
                    PostalCode = postalCode
                };
                var expected = new GeocodeAddress(stop)
                {
                    StreetNumber = streetNumber ?? string.Empty,
                    Route = route ?? string.Empty
                };

                var actual = _svc.GetValidAddress(stop);
                actual.Should().BeEquivalentTo(expected);
            }
        }
    }
}
