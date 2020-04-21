using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class CarrierServiceTests
    {
        public class IsActiveCarrierTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IDateTimeProvider> _dateTime;

            private ICarrierService _svc;

            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);

            public IsActiveCarrierTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder()
                    .WithCarriers(new List<CarrierEntity>
                    {
                        new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            IsLoadshopActive = true
                        }
                    })
                    .Build();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);

                InitService();
            }

            private void InitService()
            {
                _svc = new CarrierService(_db.Object, _mapper, _dateTime.Object);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("invalid")]
            public void InvalidCarrierId_ReturnsFalse(string carrierId)
            {
                var actual = _svc.IsActiveCarrier(carrierId);
                actual.Should().BeFalse();
            }

            [Fact]
            public void ThrowsExceptionWhenDBThrowsException()
            {
                var expected = new Exception("DB Exception");
                _db.Setup(x => x.Carriers).Throws(expected);
                InitService();

                Func<bool> action = () => _svc.IsActiveCarrier(CARRIER_ID);
                action.Should().Throw<Exception>(expected.Message);
            }

            [Fact]
            public void CarrierIsActive_ReturnsTrue()
            {
                var actual = _svc.IsActiveCarrier(CARRIER_ID);
                actual.Should().BeTrue();
            }

            [Fact]
            public void CarrierIsInactive_ReturnsFalse()
            {
                _db = new MockDbBuilder()
                    .WithCarriers(new List<CarrierEntity>
                    {
                        new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            IsLoadshopActive = false
                        }
                    })
                    .Build();
                InitService();

                var actual = _svc.IsActiveCarrier(CARRIER_ID);
                actual.Should().BeFalse();
            }
        }

        public class IsPlanningEligibleTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IDateTimeProvider> _dateTime;

            private ICarrierService _svc;

            private static readonly string SCAC = "SCAC";
            private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);

            public IsPlanningEligibleTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder()
                    .WithCarrierScacs(new List<CarrierScacEntity>
                    {
                        new CarrierScacEntity
                        {
                            Scac = SCAC,
                            IsBookingEligible = true
                        }
                    })
                    .Build();

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Now).Returns(NOW);

                InitService();
            }

            private void InitService()
            {
                _svc = new CarrierService(_db.Object, _mapper, _dateTime.Object);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("invalid")]
            public void InvalidScac_ReturnsFalse(string scac)
            {
                var actual = _svc.IsPlanningEligible(scac);
                actual.Should().BeFalse();
            }

            [Fact]
            public void ThrowsExceptionWhenDBThrowsException()
            {
                var expected = new Exception("DB Exception");
                _db.Setup(x => x.CarrierScacs).Throws(expected);
                InitService();

                Func<bool> action = () => _svc.IsPlanningEligible(SCAC);
                action.Should().Throw<Exception>(expected.Message);
            }

            [Fact]
            public void ScacIsBookingEligible_ReturnsTrue()
            {
                var actual = _svc.IsPlanningEligible(SCAC);
                actual.Should().BeTrue();
            }

            [Fact]
            public void ScacIsNotBookingEligible_ReturnsFalse()
            {
                _db = new MockDbBuilder()
                    .WithCarrierScacs(new List<CarrierScacEntity>
                    {
                        new CarrierScacEntity
                        {
                            Scac = SCAC,
                            IsBookingEligible = false
                        }
                    })
                    .Build();
                InitService();

                var actual = _svc.IsPlanningEligible(SCAC);
                actual.Should().BeFalse();
            }
        }

        public class GetAllCarrierScacsAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IDateTimeProvider> _dateTime;

            private ICarrierService _svc;

            private static readonly string CARRIER_ID_AAA = "CarrierAAA";
            private static readonly string CARRIER_ID_BBB = "CarrierBBB";
            private static readonly string CARRIER_ID_CCC = "CarrierCCC";
            private static readonly DateTime NOW = new DateTime(2020, 3, 1, 0, 0, 0);
            private List<CarrierScacEntity> CARRIER_SCACS;

            public GetAllCarrierScacsAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.Setup(x => x.Today).Returns(NOW);

                InitSeedData();
                InitDb();
                InitService();
            }

            private void InitService()
            {
                _svc = new CarrierService(_db.Object, _mapper, _dateTime.Object);
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithCarrierScacs(CARRIER_SCACS)
                    .Build();
            }

            [Fact]
            public void ThrowsExceptionWhenDBThrowsException()
            {
                var expected = new Exception("DB Exception");
                _db.Setup(x => x.CarrierScacs).Throws(expected);
                InitService();

                Func<Task> action = async () => await _svc.GetAllCarrierScacsAsync();
                action.Should().Throw<Exception>(expected.Message);
            }

            [Fact]
            public async Task SortsByCarrierName()
            {
                var actual = await _svc.GetAllCarrierScacsAsync();
                actual.Should().NotBeNullOrEmpty();
                actual.Should().HaveCount(3);
                actual.First().Carrier.CarrierName.Should().Be("AAA");
                actual.Last().Carrier.CarrierName.Should().Be("CCC");
            }

            [Fact]
            public async Task NullEffectiveAndExpirationDates_ReturnsScac()
            {
                var expected = CARRIER_SCACS[0];
                expected.EffectiveDate = null;
                expected.ExpirationDate = null;
                InitDb();
                InitService();

                var actual = await _svc.GetAllCarrierScacsAsync();
                actual.ElementAt(0).Carrier.CarrierName.Should().Be(expected.Carrier.CarrierName);
                actual.ElementAt(0).CarrierScacs.First().Scac.Should().Be(expected.Scac);
            }

            [Fact]
            public async Task ExpiredScac_DoesNotReturnScac()
            {
                var expected = CARRIER_SCACS[0];
                expected.ExpirationDate = NOW.AddDays(-1);
                InitDb();
                InitService();

                var actual = await _svc.GetAllCarrierScacsAsync();
                foreach(var item in actual)
                {
                    foreach(var carrierScac in item.CarrierScacs)
                    {
                        carrierScac.Scac.Should().NotBe(expected.Scac);
                    }
                }
            }

            [Fact]
            public async Task NotYetEffective_DoesNotReturnScac()
            {
                var expected = CARRIER_SCACS[0];
                expected.EffectiveDate = NOW.AddDays(5);
                InitDb();
                InitService();

                var actual = await _svc.GetAllCarrierScacsAsync();
                foreach (var item in actual)
                {
                    foreach (var carrierScac in item.CarrierScacs)
                    {
                        carrierScac.Scac.Should().NotBe(expected.Scac);
                    }
                }
            }

            [Fact]
            public async Task CarrierNotLoadshopActive_DoesNotReturnScac()
            {
                var expected = CARRIER_SCACS[0];
                expected.Carrier.IsLoadshopActive = false;
                InitDb();
                InitService();

                var actual = await _svc.GetAllCarrierScacsAsync();
                foreach (var item in actual)
                {
                    foreach (var carrierScac in item.CarrierScacs)
                    {
                        carrierScac.Scac.Should().NotBe(expected.Scac);
                    }
                }
            }

            [Fact]
            public async Task ScacIsInactive_DoesNotReturnScac()
            {
                var expected = CARRIER_SCACS[0];
                expected.IsActive = false;
                InitDb();
                InitService();

                var actual = await _svc.GetAllCarrierScacsAsync();
                foreach (var item in actual)
                {
                    foreach (var carrierScac in item.CarrierScacs)
                    {
                        carrierScac.Scac.Should().NotBe(expected.Scac);
                    }
                }
            }

            [Fact]
            public async Task ScacIsNotBookingEligible_DoesNotReturnScac()
            {
                var expected = CARRIER_SCACS[0];
                expected.IsBookingEligible = false;
                InitDb();
                InitService();

                var actual = await _svc.GetAllCarrierScacsAsync();
                foreach (var item in actual)
                {
                    foreach (var carrierScac in item.CarrierScacs)
                    {
                        carrierScac.Scac.Should().NotBe(expected.Scac);
                    }
                }
            }

            private void InitSeedData()
            {
                CARRIER_SCACS = new List<CarrierScacEntity>
                {
                    new CarrierScacEntity
                    {
                        Scac = "AAA",
                        EffectiveDate = NOW.AddDays(-5),
                        ExpirationDate = NOW.AddDays(5),
                        IsActive = true,
                        IsBookingEligible = true,
                        CarrierId = CARRIER_ID_AAA,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID_AAA,
                            CarrierName = "AAA",
                            IsLoadshopActive = true
                        }
                    },
                    new CarrierScacEntity
                    {
                        Scac = "CCC",
                        EffectiveDate = NOW.AddDays(-5),
                        ExpirationDate = NOW.AddDays(5),
                        IsActive = true,
                        IsBookingEligible = true,
                        CarrierId = CARRIER_ID_CCC,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID_CCC,
                            CarrierName = "CCC",
                            IsLoadshopActive = true
                        }
                    },
                    new CarrierScacEntity
                    {
                        Scac = "BBB",
                        EffectiveDate = NOW.AddDays(-5),
                        ExpirationDate = NOW.AddDays(5),
                        IsActive = true,
                        IsBookingEligible = true,
                        CarrierId = CARRIER_ID_BBB,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID_BBB,
                            CarrierName = "BBB",
                            IsLoadshopActive = true
                        }
                    }
                };
            }
        }
    }
}
