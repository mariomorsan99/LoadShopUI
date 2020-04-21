using System;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services;
using Xunit;
using FluentAssertions;
using Moq;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System.Collections.Generic;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class LocationServiceTests
    {
        public class GetLocationTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private LocationService _service;

            private static Guid CUSTOMER_ID = Guid.NewGuid();
            private static long LOCATION_ID_1 = 5;
            private static long LOCATION_ID_2 = 10;
            private static string LOCATION_NAME_1 = "Location Name 1";
            private static string LOCATION_NAME_2 = "Location Name 2";

            private LocationEntity LOCATION_1 = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID_1,
                LocationName = LOCATION_NAME_1
            };
            private LocationEntity LOCATION_2 = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID_2,
                LocationName = LOCATION_NAME_2
            };

            public GetLocationTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder()
                    .WithLocations(new List<LocationEntity>()
                    {
                        LOCATION_1, LOCATION_2
                    })
                    .Build();
            }

            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [Theory]
            public void GetLocation_NullOrEmpty(string locationName)
            {
                _service = new LocationService(_db.Object, _mapper);

                var result = _service.GetLocation(CUSTOMER_ID, locationName);
                result.Should().BeNull();
            }

            [Fact]
            public void GetLocation_LocationNotFoundForCustomer()
            {
                _service = new LocationService(_db.Object, _mapper);

                var result = _service.GetLocation(Guid.NewGuid(), LOCATION_NAME_1);
                result.Should().BeNull();
            }

            [Fact]
            public void GetLocation_LocationNotFound()
            {
                _service = new LocationService(_db.Object, _mapper);

                var result = _service.GetLocation(CUSTOMER_ID, "not found");
                result.Should().BeNull();
            }

            [Fact]
            public void GetLocation()
            {
                _service = new LocationService(_db.Object, _mapper);

                var result = _service.GetLocation(CUSTOMER_ID, LOCATION_NAME_1);
                result.Should().NotBeNull();
                result.LocationId.Should().Be(LOCATION_ID_1);

                result = _service.GetLocation(CUSTOMER_ID, LOCATION_NAME_1.ToUpper());
                result.Should().NotBeNull();
                result.LocationId.Should().Be(LOCATION_ID_1);

                result = _service.GetLocation(CUSTOMER_ID, LOCATION_NAME_1.ToLower());
                result.Should().NotBeNull();
                result.LocationId.Should().Be(LOCATION_ID_1);

                result = _service.GetLocation(CUSTOMER_ID, LOCATION_NAME_2);
                result.Should().NotBeNull();
                result.LocationId.Should().Be(LOCATION_ID_2);
            }
        }

        public class GetLocationsTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private LocationService _service;

            private static Guid CUSTOMER_ID = Guid.NewGuid();
            private static long LOCATION_ID_1 = 5;
            private static long LOCATION_ID_2 = 10;

            private LocationEntity LOCATION_1 = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID_1
            };
            private LocationEntity LOCATION_2 = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID_2
            };

            public GetLocationsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder()
                    .WithLocations(new List<LocationEntity>()
                    {
                        LOCATION_1, LOCATION_2
                    })
                    .Build();
            }

            [Fact]
            public void GetLocations_NoLoadsForCustomer()
            {
                _service = new LocationService(_db.Object, _mapper);

                var results = _service.GetLocations(Guid.NewGuid());
                results.Should().NotBeNull();
                results.Should().BeEmpty();
            }

            [Fact]
            public void GetLocations()
            {
                _service = new LocationService(_db.Object, _mapper);

                var results = _service.GetLocations(CUSTOMER_ID);
                results.Should().NotBeNullOrEmpty();
                results.Count.Should().Be(2);

                foreach (var item in results)
                {
                    item.Should().NotBeNull();
                    item.CustomerId.Should().Be(CUSTOMER_ID);
                    item.LocationId.Should().BeOneOf(LOCATION_ID_1, LOCATION_ID_2);
                }
            }
        }

        public class SearchLocationsTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private LocationService _service;

            private static Guid CUSTOMER_ID = Guid.NewGuid();
            private static long LOCATION_ID_1 = 5;
            private static long LOCATION_ID_2 = 10;
            private static long LOCATION_ID_3 = 10;
            private static string LOCATION_NAME_1 = "Location Name 1";
            private static string LOCATION_NAME_2 = "Location Name 2";
            private static string LOCATION_NAME_3 = "another";

            private LocationEntity LOCATION_1 = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID_1,
                LocationName = LOCATION_NAME_1
            };
            private LocationEntity LOCATION_2 = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID_2,
                LocationName = LOCATION_NAME_2
            };
            private LocationEntity LOCATION_3 = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID_3,
                LocationName = LOCATION_NAME_3
            };

            public SearchLocationsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder()
                    .WithLocations(new List<LocationEntity>()
                    {
                        LOCATION_1, LOCATION_2
                    })
                    .Build();
            }

            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [Theory]
            public void SearchLocations_NullOrEmpty(string searchTerm)
            {
                _service = new LocationService(_db.Object, _mapper);

                var result = _service.SearchLocations(CUSTOMER_ID, searchTerm);
                result.Should().NotBeNull();
                result.Should().BeEmpty();
            }

            [Fact]
            public void SearchLocations_NoLoadsForCustomer()
            {
                _service = new LocationService(_db.Object, _mapper);

                var results = _service.SearchLocations(Guid.NewGuid(), "location");
                results.Should().NotBeNull();
                results.Should().BeEmpty();
            }

            [Fact]
            public void SearchLocations_SearchTermNotFound()
            {
                _service = new LocationService(_db.Object, _mapper);

                var results = _service.SearchLocations(CUSTOMER_ID, "location name not found");
                results.Should().NotBeNull();
                results.Should().BeEmpty();
            }

            [InlineData("Location")]
            [InlineData("LOCATION")]
            [InlineData("location")]
            [InlineData(" name ")]
            [InlineData(" NAME ")]
            [Theory]
            public void SearchLocations(string locationName)
            {
                _service = new LocationService(_db.Object, _mapper);

                var results = _service.SearchLocations(CUSTOMER_ID, locationName);
                results.Should().NotBeNullOrEmpty();
                results.Count.Should().Be(2);

                foreach (var item in results)
                {
                    item.Should().NotBeNull();
                    item.CustomerId.Should().Be(CUSTOMER_ID);
                    item.LocationId.Should().BeOneOf(LOCATION_ID_1, LOCATION_ID_2);
                }
            }

            [InlineData("Location Name 1")]
            [InlineData("LOCATION NAME 1")]
            [InlineData("location name 1")]
            [InlineData("name 1")]
            [InlineData("NAME 1")]
            [Theory]
            public void SearchLocations_OneMatch(string locationName)
            {
                _service = new LocationService(_db.Object, _mapper);

                var results = _service.SearchLocations(CUSTOMER_ID, locationName);
                results.Should().NotBeNullOrEmpty();
                results.Count.Should().Be(1);

                results[0].Should().NotBeNull();
                results[0].LocationId.Should().Be(LOCATION_ID_1);
            }
        }

        public class AddOrUpdateLocationsTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private LocationService _service;
            private readonly string _username = "Loadshop.Testing";

            private static Guid CUSTOMER_ID = Guid.NewGuid();
            private static long LOCATION_ID = 5;
            private static string LOCATION_NAME = "Location Name 1";

            private LocationEntity LOCATION = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID,
                LocationName = LOCATION_NAME
            };

            public AddOrUpdateLocationsTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder()
                    .WithLocation(LOCATION)
                    .Build();
            }

            [Fact]
            public void AddOrUpdateLocations_Null()
            {
                _service = new LocationService(_db.Object, _mapper);

                _service.AddOrUpdateLocations(CUSTOMER_ID, null, _username).Should().BeNull();
                _db.Verify(x => x.SaveChanges(_username), Times.Never);
            }

            [Fact]
            public void AddOrUpdateLocations_Empty()
            {
                _service = new LocationService(_db.Object, _mapper);

                _service.AddOrUpdateLocations(CUSTOMER_ID, new List<LocationData>(), _username).Should().BeEmpty();
                _db.Verify(x => x.SaveChanges(_username), Times.Never);
            }

            [Fact]
            public void AddOrUpdateLocations_LocationNotFound()
            {
                _service = new LocationService(_db.Object, _mapper);

                _service.Invoking(x => x.AddOrUpdateLocations(CUSTOMER_ID, new List<LocationData>()
                        {
                            new LocationData()
                            {
                                CustomerId = CUSTOMER_ID,
                                LocationId = 1
                            }
                        }, _username)).Should()
                    .Throw<Exception>()
                    .WithMessage("Location not found");
            }

            [Fact]
            public void AddOrUpdateLocations_Create()
            {
                var locations = new List<LocationData>()
                {
                    new LocationData()
                    {
                        CustomerId = CUSTOMER_ID,
                        LocationName = "new location"
                    }
                };

                _service = new LocationService(_db.Object, _mapper);

                _service.AddOrUpdateLocations(CUSTOMER_ID, locations, _username).Should().BeSameAs(locations);
                _db.Verify(x => x.SaveChanges(_username), Times.Once);
            }

            [Fact]
            public void AddOrUpdateLocations_Update()
            {
                var locations = new List<LocationData>()
                {
                    new LocationData()
                    {
                        CustomerId = CUSTOMER_ID,
                        LocationId = LOCATION_ID,
                        LocationName = "updated location"
                    }
                };

                _service = new LocationService(_db.Object, _mapper);

                _service.AddOrUpdateLocations(CUSTOMER_ID, locations, _username).Should().BeSameAs(locations);
                _db.Verify(x => x.SaveChanges(_username), Times.Once);
            }

            [Fact]
            public void AddOrUpdateLocations_UpdateFromLocationName()
            {
                var locations = new List<LocationData>()
                {
                    new LocationData()
                    {
                        CustomerId = CUSTOMER_ID,
                        LocationName = LOCATION_NAME,
                        City = "city"
                    }
                };

                _service = new LocationService(_db.Object, _mapper);

                _service.AddOrUpdateLocations(CUSTOMER_ID, locations, _username).Should().BeSameAs(locations);
                _db.Verify(x => x.SaveChanges(_username), Times.Once);
            }

            [Fact]
            public void AddOrUpdateLocations_MultipleLocations()
            {
                var locations = new List<LocationData>()
                {
                    new LocationData()
                    {
                        CustomerId = CUSTOMER_ID,
                        LocationName = "new location"
                    },
                    new LocationData()
                    {
                        CustomerId = CUSTOMER_ID,
                        LocationId = LOCATION_ID,
                        LocationName = "updated location"
                    }
                };

                _service = new LocationService(_db.Object, _mapper);

                _service.AddOrUpdateLocations(CUSTOMER_ID, locations, _username).Should().BeSameAs(locations);
                _db.Verify(x => x.SaveChanges(_username), Times.Once);
            }
        }

        public class DeleteLocationTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private LocationService _service;

            private static Guid CUSTOMER_ID = Guid.NewGuid();
            private static long LOCATION_ID = 5;

            private LocationEntity LOCATION = new LocationEntity
            {
                CustomerId = CUSTOMER_ID,
                LocationId = LOCATION_ID
            };

            public DeleteLocationTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder()
                    .WithLocation(LOCATION)
                    .Build();
            }

            [Fact]
            public void DeleteLocation_LocationNotFound()
            {
                _service = new LocationService(_db.Object, _mapper);

                _service.Invoking(x => x.DeleteLocation(CUSTOMER_ID, 0)).Should()
                    .Throw<Exception>()
                    .WithMessage("Location not found");
            }

            [Fact]
            public void DeleteLocation_LocationNotFoundForCustomer()
            {
                _service = new LocationService(_db.Object, _mapper);

                _service.Invoking(x => x.DeleteLocation(Guid.NewGuid(), LOCATION_ID)).Should()
                    .Throw<Exception>()
                    .WithMessage("Location not found");
            }

            [Fact]
            public void DeleteLocation()
            {
                _service = new LocationService(_db.Object, _mapper);

                _service.DeleteLocation(CUSTOMER_ID, LOCATION_ID);
                _db.Verify(x => x.SaveChanges(), Times.Once);
            }
        }
    }
}
