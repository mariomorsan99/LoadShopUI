using FluentAssertions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Loadshop.Tests.DomainServices.Utility
{
    public class ServiceUtilitiesTests
    {
        public class GetContractRateTests
        {
            private ServiceUtilities _svc;
            private Mock<LoadshopDataContext> _db;
            private Mock<IDateTimeProvider> _dateTime;

            private static readonly Guid LOAD_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string SCAC = "SCAC";

            private readonly List<UserEntity> USERS = new List<UserEntity>
            {
                new UserEntity
                {
                    UserId = USER_ID,
                    IdentUserId = USER_ID,
                    PrimaryScac = SCAC
                }
            };
            private readonly List<LoadCarrierScacEntity> LOAD_CARRIER_SCACS = new List<LoadCarrierScacEntity>
            {
                new LoadCarrierScacEntity
                {
                    LoadId = LOAD_ID,
                    Scac = SCAC,
                    ContractRate = 99m,
                    CarrierScac = new CarrierScacEntity
                    {
                        Scac = SCAC
                    }
                }
            };

            public GetContractRateTests()
            {
                _dateTime = new Mock<IDateTimeProvider>();

                _svc = new ServiceUtilities(_dateTime.Object);
                _db = new MockDbBuilder()
                    .WithUsers(USERS)
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .Build();
            }

            [Fact]
            public void DbThrowsError_Throws()
            {
                var expected = new Exception("DB Error");
                _db.Setup(x => x.Users).Throws(expected);
                Action action = () => _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                action.Should().Throw<Exception>(expected.Message);
            }

            [Fact]
            public void UserNotFound_ReturnsNull()
            {
                _db = new MockDbBuilder()
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .Build();

                var contractRate = _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                contractRate.Should().BeNull();
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("invalid")]
            public void InvalidPrimaryScac_ReturnsNull(string primaryScac)
            {
                USERS.First().PrimaryScac = primaryScac;
                _db = new MockDbBuilder()
                    .WithUsers(USERS)
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .Build();

                var contractRate = _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                contractRate.Should().BeNull();
            }

            [Fact]
            public void InvalidLoadId_ReturnsNull()
            {
                LOAD_CARRIER_SCACS.First().LoadId = Guid.Empty;
                _db = new MockDbBuilder()
                    .WithUsers(USERS)
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .Build();

                var contractRate = _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                contractRate.Should().BeNull();
            }

            [Fact]
            public void PrimaryScacNotFound_ReturnsNull()
            {
                LOAD_CARRIER_SCACS.First().Scac = "invalid";
                _db = new MockDbBuilder()
                    .WithUsers(USERS)
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .Build();

                var contractRate = _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                contractRate.Should().BeNull();
            }

            [Fact]
            public void NullContractRate_ReturnsNull()
            {
                LOAD_CARRIER_SCACS.First().ContractRate = null;
                _db = new MockDbBuilder()
                    .WithUsers(USERS)
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .Build();

                var contractRate = _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                contractRate.Should().BeNull();
            }

            [Fact]
            public void ReturnsValidContractRate()
            {
                var contractRate = _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                contractRate.Should().Be(LOAD_CARRIER_SCACS.First().ContractRate);
            }

            [Fact]
            public void IsDedicated_ReturnsNull()
            {
                var carrierScac = LOAD_CARRIER_SCACS.First().CarrierScac;
                carrierScac.IsDedicated = true;
                _db = new MockDbBuilder()
                    .WithUsers(USERS)
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .Build();

                var contractRate = _svc.GetContractRate(_db.Object, LOAD_ID, USER_ID);
                contractRate.Should().Be(null);
            }

        }

        public class HasLoadChangedTests
        {
            private ServiceUtilities _svc;
            private Mock<IDateTimeProvider> _dateTime;

            public HasLoadChangedTests()
            {
                _dateTime = new Mock<IDateTimeProvider>();
                _svc = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void IdenticalLoadHistories_NotChanged()
            {
                var history = new LoadHistoryEntity();
                var actual = _svc.HasLoadChanged(history, history);
                actual.Should().BeFalse();
            }

            [Fact]
            public void NullOrigPropertyValue_HasChanged()
            {
                var orig = new LoadHistoryEntity
                {
                    Comments = null
                };
                var latest = new LoadHistoryEntity
                {
                    Comments = "comments"
                };
                var actual = _svc.HasLoadChanged(orig, latest);
                actual.Should().BeTrue();
            }

            [Fact]
            public void NullLatestPropertyValue_HasChanged()
            {
                var orig = new LoadHistoryEntity
                {
                    Comments = "comments"
                };
                var latest = new LoadHistoryEntity
                {
                    Comments = null
                };
                var actual = _svc.HasLoadChanged(orig, latest);
                actual.Should().BeTrue();
            }

            [Fact]
            public void DifferentPropertyValues_HasChanged()
            {
                var orig = new LoadHistoryEntity
                {
                    LineHaulRate = 1m
                };
                var latest = new LoadHistoryEntity
                {
                    LineHaulRate = 2m
                };
                var actual = _svc.HasLoadChanged(orig, latest);
                actual.Should().BeTrue();
            }
        }

        public class MapShipperSearchTypeToTransactionListTests
        {
            public MapShipperSearchTypeToTransactionListTests() { }

            /// <summary>
            /// Just documenting that we are intentionally not testing returning static values
            /// </summary>
            [Fact]
            public void NoNeedToTest()
            {
                true.Should().BeTrue();
            }
        }

        public class DistanceFromTodayTests
        {
            public DistanceFromTodayTests() { }

            /// <summary>
            /// Just documenting that we are intentionally not testing Math.Abs, subtraction operators and the DateTimeOffset class
            /// </summary>
            [Fact]
            public void NoNeedToTest()
            {
                true.Should().BeTrue();
            }
        }
    }
}
