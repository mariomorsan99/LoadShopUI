using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Loadshop.DomainServices.Loadshop.Services;
using Moq;
using Microsoft.Extensions.Logging;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class CarrierAdminServiceUnitTest : CrudServiceUnitTest<CarrierProfileData, ICarrierAdminService>
    {
        protected Mock<LoadshopDataContext> _db;
        private readonly Mock<ILogger<CarrierAdminService>> _logger;

        private static Guid ADMIN_USER_ID = new Guid("11111111-1111-1111-1111-111111111111");

        private List<CarrierEntity> CARRIER_ENTITIES;
        private CarrierProfileData CARRIER_PROFILE_DATA;

        public CarrierAdminServiceUnitTest(TestFixture fixture) : base(fixture)
        {
            _logger = new Mock<ILogger<CarrierAdminService>>();

            InitSeedData();
            _db = new MockDbBuilder()
                .WithCarriers(CARRIER_ENTITIES)
                .Build();

            _securityService.Setup(_ => _.UserHasAction("loadshop.ui.system.carrier.addedit")).Returns(true);

            CrudService = new CarrierAdminService(_db.Object, _mapper, _logger.Object, _userContext.Object, _securityService.Object);
        }

        [Fact]
        public override async Task GetCollectionTest()
        {
            await GetCollectionTestHelper<CarrierEntity>();
        }

        [Fact]
        public override async Task GetByKeyTest()
        {
            var result = CARRIER_PROFILE_DATA;
            await GetByKeyTestHelper(result, result.CarrierId);
        }

        /// <summary>
        /// Create is not setup in Admin service because the mapping ignores Carrier Properties that are poulated from TOPS
        /// This can be enabled if the Carrier's are ever directly created in Loadshop
        /// </summary>
        /// <returns></returns>
        //[Fact]
        public override async Task CreateTest()
        {
            await CreateTestHelper(CARRIER_PROFILE_DATA, CARRIER_PROFILE_DATA);
        }

        [Fact]
        public override async Task UpdateTest()
        {
            await UpdateTestHelper(CARRIER_PROFILE_DATA, CARRIER_PROFILE_DATA, CARRIER_PROFILE_DATA.CarrierId);
        }

        [Fact]
        public override async Task DeleteTest()
        {
            await DeleteHelper("SCAC Carrier");
        }

        [Fact]
        public async Task TestCarrierUserValidation()
        {
            var updateData = CARRIER_PROFILE_DATA;
            updateData.CarrierSuccessSpecialistId = null;
            updateData.CarrierSuccessTeamLeadId = null;

            var result = await CrudService.Update(updateData, true, updateData.CarrierId);

            var errors = new List<string>()
            {
                "Carrier Success Specialist is required",
                "Carrier Team Lead is required"
            };

            result.IsValid.Should().BeFalse();
            result.ModelState.Should().HaveCount(2);
            result.ModelState.Select(error => error.Value.Errors.First().ErrorMessage).Should().BeEquivalentTo(errors);
        }

        private void InitSeedData()
        {
            CARRIER_ENTITIES = new List<CarrierEntity>()
            {
                new CarrierEntity()
                {
                    CarrierId = "SCAC Carrier",
                    CarrierName = "SCAC Carrier",
                    CarrierSuccessSpecialistId = ADMIN_USER_ID,
                    CarrierSuccessTeamLeadId = ADMIN_USER_ID,
                    City = "Green Bay",
                    State = "WI",
                    Country = "USA",
                    Address = "123 SCAC Street",
                    HasCanAuth = true,
                    HasMexAuth = true,
                    IsLoadshopActive = true,
                    OperatingAuthNbr = "123456789",
                    USDOTNbr = "123456789",
                    Comments = "Oggaa Booga Comment",
                    CarrierScacs = new List<CarrierScacEntity>()
                    {
                        new CarrierScacEntity()
                        {
                            Scac = "SCAC",
                            ScacName = "SCAC - SCAC Carrier",
                            CarrierId = "SCAC Carrier",
                            IsActive = true,
                            IsBookingEligible = true,
                            IsDedicated = false,
                            DataSource = "TOPS",
                            EffectiveDate = DateTime.Today.AddDays(-1),
                            ExpirationDate = DateTime.Today.AddDays(100),
                            //Needed for Active CarrierScac Filter
                            Carrier = new CarrierEntity()
                            {
                                IsLoadshopActive = true
                            }
                        }
                    }
                }
            };
            CARRIER_PROFILE_DATA = new CarrierProfileData
            {
                CarrierId = "SCAC Carrier",
                CarrierName = "SCAC Carrier",
                CarrierSuccessSpecialistId = ADMIN_USER_ID,
                CarrierSuccessTeamLeadId = ADMIN_USER_ID,
                City = "Green Bay",
                State = "WI",
                Country = "USA",
                Address = "123 SCAC Street",
                HasCanAuth = true,
                HasMexAuth = true,
                IsLoadshopActive = true,
                OperatingAuthNbr = "123456789",
                USDOTNbr = "123456789",
                Comments = "Oggaa Booga Comment",
                CarrierScacs = new List<string>() { "SCAC" },
                Scacs = new List<CarrierScacData>()
                    {
                        new CarrierScacData()
                        {
                            Scac = "SCAC",
                            ScacName = "SCAC - SCAC Carrier",
                            CarrierId = "SCAC Carrier",
                            IsActive = true,
                            IsBookingEligible = true,
                            IsDedicated = false,
                            DataSource = "TOPS",
                            EffectiveDate = DateTime.Today.AddDays(-1),
                            ExpirationDate = DateTime.Today.AddDays(100),
                        }
                    }
            };
        }
    }
}

