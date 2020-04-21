using FluentAssertions;
using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Loadshop.Services.Repositories;
using Loadshop.DomainServices.Utilities;
using Loadshop.Tests.DomainServices;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Loadshop.Testing.DomainServices.Loadshop
{
    public class LoadQueryRepositoryTests : IClassFixture<TestFixture>
    {
        private Mock<LoadshopDataContext> _db;
        private readonly Mock<ISecurityService> _securityService;
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;

        private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid LOAD_CARRIER_SCAC_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        internal readonly decimal FuelRate = 123.45m;
        private static readonly string CARRIER_ID = "CarrierId";
        private static readonly string SCAC = "SCAC";

        private ILoadQueryRepository _repo;

        public LoadQueryRepositoryTests()
        {
            _securityService = new Mock<ISecurityService>();
            _dateTimeProvider = new Mock<IDateTimeProvider>();

            InitDb();
            InitLoadQueryRepo();
        }

        [Fact]
        public void HighContractRate_Admin_ReturnsLoad()
        {
            var lineHaulRate = 100M;
            var contractRate = lineHaulRate + 1;

            var loadCarrierScacs = BuildLoadCarrierScacs();
            loadCarrierScacs.ForEach(x => x.ContractRate = contractRate);

            var loads = BuildLoads();
            loads.ForEach(x =>
            {
                x.LineHaulRate = lineHaulRate;
                x.CarrierScacs = loadCarrierScacs;
            });

            InitDb(loads: loads, loadCarrierScacs: loadCarrierScacs);
            InitLoadQueryRepo();

            var result = _repo.GetLoadsForCarrierMarketplaceAsAdmin(new[] { TransactionTypes.New });
            result.Should().HaveCount(1);
        }

        [Fact]
        public void HighContractRate_NonAdmin_DoesNotReturnLoad()
        {
            var lineHaulRate = 100M;
            var contractRate = lineHaulRate + 1;

            var loadCarrierScacs = BuildLoadCarrierScacs();
            loadCarrierScacs.ForEach(x => x.ContractRate = contractRate);

            var loads = BuildLoads();
            loads.ForEach(x =>
            {
                x.LineHaulRate = lineHaulRate;
                x.CarrierScacs = loadCarrierScacs;
            });

            InitDb(loads: loads, loadCarrierScacs: loadCarrierScacs);
            InitLoadQueryRepo();

            var result = _repo.GetLoadsForCarrierMarketplace(new[] { TransactionTypes.New }, SCAC);
            result.Should().BeEmpty();
        }

        [Fact]
        public void HighContractRateOnDedicatedScac_NonAdmin_ReturnsLoad()
        {
            var lineHaulRate = 100M;
            var contractRate = lineHaulRate + 1;

            var loadCarrierScacs = BuildLoadCarrierScacs();
            loadCarrierScacs.ForEach(x => x.ContractRate = contractRate);

            var carrierScacs = BuildCarrierScacs();
            carrierScacs.ForEach(x => x.IsDedicated = true);

            var loads = BuildLoads();
            loads.ForEach(x =>
            {
                x.LineHaulRate = lineHaulRate;
                x.CarrierScacs = loadCarrierScacs;
            });

            InitDb(loads: loads, loadCarrierScacs: loadCarrierScacs, carrierScacs: carrierScacs);
            InitLoadQueryRepo();

            var result = _repo.GetLoadsForCarrierMarketplace(new[] { TransactionTypes.New }, SCAC);
            result.Should().HaveCount(1);
        }


        [Theory]
        [InlineData(TransactionTypes.New)]
        [InlineData(TransactionTypes.Updated)]
        public void GetMarketplaceLoads(string transactionTypeId)
        {
            InitDb();
            InitLoadQueryRepo();

            var result = _repo.GetLoadsForCarrierMarketplace(new[] { transactionTypeId }, SCAC);
            result.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(TransactionTypes.Accepted)]
        [InlineData(TransactionTypes.Pending)]
        [InlineData(TransactionTypes.SentToShipperTender)]
        [InlineData(TransactionTypes.PreTender)]
        [InlineData(TransactionTypes.Delivered)]
        public void GetLoadsWithLoadClaims(string transactionTypeId)
        {
            InitDb();
            InitLoadQueryRepo();

            var result = _repo.GetLoadsForCarrierWithLoadClaim(new[] { transactionTypeId }, new[] { SCAC });
            result.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(TransactionTypes.New)]
        [InlineData(TransactionTypes.Accepted)]
        [InlineData(TransactionTypes.Pending)]
        [InlineData(TransactionTypes.SentToShipperTender)]
        [InlineData(TransactionTypes.PreTender)]
        [InlineData(TransactionTypes.Delivered)]
        public void GetLoadDetails(string transactionTypeId)
        {
            var loads = BuildLoads();
            var loadTestId = Guid.NewGuid();
            loads.First().LoadId = loadTestId;
            loads.First().LatestTransactionTypeId = transactionTypeId;
            var loadTransactions = BuildLoadTransactions();
            loadTransactions.First().LoadId = loadTestId;


            InitDb(loads: loads, loadTransactions: loadTransactions);
            InitLoadQueryRepo();

            var opts = new GetLoadDetailOptions()
            {
                LoadId = loadTestId,
                TransactionTypes = new List<string>() { transactionTypeId }
            };

            var result = _repo.GetLoadDetailViews(opts);
            result.Should().HaveCount(1);
            result.First().LoadId.Should().Be(loadTestId);
        }

        [Fact]
        public async Task GetLoadDetailsUnprocessed()
        {
            var loadTransactions = BuildLoadTransactions();
            loadTransactions.First().ProcessedDtTm = DateTime.Now;

            InitDb(loadTransactions: loadTransactions);
            InitLoadQueryRepo();

            var result = await _repo.GetLoadDetailViewUnprocessedAsync();
            result.Should().HaveCount(0);
        }

        [Fact]
        public void NoLoadCarrierScacs_Admin_ReturnsLoad()
        {
            InitDb(loadCarrierScacs: new List<LoadCarrierScacEntity>());
            InitLoadQueryRepo();

            var result = _repo.GetLoadsForCarrierMarketplaceAsAdmin(new[] { TransactionTypes.New, TransactionTypes.Updated });
            result.Should().HaveCount(2);
        }

        private void InitDb(
            List<LoadEntity> loads = null,
            List<LoadStopEntity> loadStops = null,
            List<LoadCarrierScacEntity> loadCarrierScacs = null,
            List<UserEntity> users = null,
            List<CustomerEntity> customers = null,
            List<LoadTransactionEntity> loadTransactions = null,
            List<LoadClaimEntity> loadClaims = null,
            List<CarrierScacEntity> carrierScacs = null,
            List<CarrierEntity> carriers = null,
            List<CustomerCarrierScacContractEntity> customerCarrierScacContracts = null)
        {
            _db = new MockDbBuilder()
                .WithLoads(loads ?? BuildLoads())
                .WithLoadStops(loadStops ?? BuildLoadStops())
                .WithLoadCarrierScacs(loadCarrierScacs ?? BuildLoadCarrierScacs())
                .WithUsers(users ?? BuildUser())
                .WithCustomers(customers ?? BuildCustomers())
                .WithLoadTransactions(loadTransactions ?? BuildLoadTransactions())
                .WithLoadClaims(loadClaims ?? BuildLoadClaims())
                .WithCarrierScacs(carrierScacs ?? BuildCarrierScacs())
                .WithCarriers(carriers ?? BuildCarriers())
                .WithCustomerCarrierScacContracts(customerCarrierScacContracts ?? BuildCustomerCarrierScacContracts())
                .Build();
        }

        private void InitLoadQueryRepo()
        {
            _repo = new LoadQueryRepository(_db.Object, _securityService.Object, _dateTimeProvider.Object);
        }

        private List<LoadEntity> BuildLoads()
        {
            return new List<LoadEntity>()
            {
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.Accepted,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.Delivered,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.Error,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.New,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.Pending,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.PendingAdd,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.PendingFuel,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.PendingRates,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.PendingRemove,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.PendingRemoveScac,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.PendingUpdate,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.Posted,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.PreTender,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.Removed,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.SentToShipperTender,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                },
                new LoadEntity()
                {
                    LoadId = LOAD_ID,
                    LatestTransactionTypeId = TransactionTypes.Updated,
                    Stops = 2,
                    CustomerId = CUSTOMER_ID,
                    LineHaulRate = 1,
                    CarrierScacs = BuildLoadCarrierScacs().ToList(),
                    Equipment = new EquipmentEntity(){ EquipmentDesc = "BIG TRUCK" }
                }
            };
        }

        private List<LoadStopEntity> BuildLoadStops()
        {
            return new List<LoadStopEntity>()
            {
                new LoadStopEntity()
                {
                    LoadId = LOAD_ID,
                    StopNbr = 1
                },
                new LoadStopEntity()
                {
                    LoadId = LOAD_ID,
                    StopNbr = 2
                }
            };
        }

        private List<LoadCarrierScacEntity> BuildLoadCarrierScacs()
        {
            return new List<LoadCarrierScacEntity>()
            {
                new LoadCarrierScacEntity()
                {
                    LoadCarrierScacId = LOAD_CARRIER_SCAC_ID,
                    LoadId = LOAD_ID,
                    Scac = SCAC
                }
            };
        }

        private List<UserEntity> BuildUser()
        {
            return new List<UserEntity>()
            {
                new UserEntity()
                {
                    PrimaryScac = SCAC,
                    IdentUserId = USER_ID
                }
            };
        }

        private List<CustomerEntity> BuildCustomers()
        {
            return new List<CustomerEntity>()
            {
                new CustomerEntity()
                {
                    CustomerId = CUSTOMER_ID
                }
            };
        }

        private List<LoadTransactionEntity> BuildLoadTransactions()
        {
            return new List<LoadTransactionEntity>()
            {
                new LoadTransactionEntity()
                {
                    LoadId = LOAD_ID,
                    LoadTransactionId = LOAD_TRANSACTION_ID
                }
            };
        }

        private List<LoadClaimEntity> BuildLoadClaims()
        {
            return new List<LoadClaimEntity>()
            {
                new LoadClaimEntity()
                {
                    LoadTransactionId = LOAD_TRANSACTION_ID,
                    Scac = SCAC
                }
            };
        }

        private List<CarrierScacEntity> BuildCarrierScacs()
        {
            return new List<CarrierScacEntity>()
            {
                new CarrierScacEntity()
                {
                    Scac = SCAC,
                    CarrierId = CARRIER_ID
                }
            };
        }

        private List<CarrierEntity> BuildCarriers()
        {
            return new List<CarrierEntity>()
            {
                new CarrierEntity()
                {
                    CarrierId = CARRIER_ID,
                    CarrierName = "Carrier"
                }
            };
        }

        private List<CustomerCarrierScacContractEntity> BuildCustomerCarrierScacContracts()
        {
            return new List<CustomerCarrierScacContractEntity>()
            {

            };
        }
    }
}
