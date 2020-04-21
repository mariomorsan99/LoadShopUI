using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Loadshop.DomainServices.Common.Services.Crud;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class UserCommunicationServiceUnitTest : CrudServiceUnitTest<UserCommunicationDetailData, IUserCommunicationService>
    {
        protected Mock<LoadshopDataContext> _db;
        private readonly Mock<ILogger<UserCommunicationService>> _logger;

        private static Guid USER_COMM_ID = new Guid("11111111-1111-1111-1111-111111111111");
        private static Guid USER_ID = new Guid("22222222-2222-2222-2222-222222222222");
        private static Guid USER_IDENT_ID = new Guid("33333333-3333-3333-3333-333333333333");

        private static List<UserCommunicationEntity> USER_COMMUNICATIONS => new List<UserCommunicationEntity>
        {
            new UserCommunicationEntity()
            {
                UserCommunicationId = USER_COMM_ID,
                Title = "UC Test 1",
                Message = "This is a test!",
                AcknowledgementRequired = false,
                AllUsers = true,
                IsDeleted = false,
                EffectiveDate = DateTime.Today.AddDays(-30),
                ExpirationDate = null,
                OwnerId = USER_ID,
                UserCommunicationCarriers = new List<UserCommunicationCarrierEntity>(),
                UserCommunicationShippers = new List<UserCommunicationShipperEntity>(),
                UserCommunicationUsers = new List<UserCommunicationUserEntity>(),
                UserCommunicationSecurityAccessRoles = new List<UserCommunicationSecurityAccessRoleEntity>(),
                UserCommunicationAcknowledgements = new List<UserCommunicationAcknowledgementEntity>()// USER_COMM_ACKNOWLEDGEMENTS
            }
        };

        private static List<UserCommunicationAcknowledgementEntity> USER_COMM_ACKNOWLEDGEMENTS => new List<UserCommunicationAcknowledgementEntity>()
        {
            new UserCommunicationAcknowledgementEntity()
            {
                UserCommunicationAcknowledgementId = Guid.NewGuid(),
                UserCommunicationId = USER_COMM_ID,
                Acknowledged = true,
                UserId = USER_ID,
                AcknowledgedDate = DateTime.Now
            }
        };

        private static List<UserEntity> USERS => new List<UserEntity>()
        {
            new UserEntity()
            {
                IdentUserId = USER_IDENT_ID,
                UserId = USER_ID,
                FirstName = "Carrier",
                LastName = "User",
                PrimaryScac = "KBXL",
                IsNotificationsEnabled = true,
                UserShippers = new List<UserShipperEntity>(),
                UserCarrierScacs = new List<UserCarrierScacEntity>(),
                SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>()
                {
                    new SecurityUserAccessRoleEntity()
                    {
                        UserId = USER_ID,
//                        AccessRoleId = GetSecurityAccessRole(SecurityRoleType.SysAdmin).AccessRoleId,
//                        SecurityAccessRole = GetSecurityAccessRole(SecurityRoleType.SysAdmin)
                    }
                }
            }
        };

        private static UserCommunicationDetailData DATA => new UserCommunicationDetailData()
        {
            UserCommunicationId = USER_COMM_ID,
            Title = "UC Test 1",
            Message = "This is a test!",
            AcknowledgementRequired = false,
            AllUsers = true,
            EffectiveDate = DateTime.Today.AddDays(-30),
            ExpirationDate = null,
            OwnerId = USER_ID,
            UserCommunicationCarriers = new List<UserCommunicationCarrierData>(),
            UserCommunicationShippers = new List<UserCommunicationShipperData>(),
            UserCommunicationUsers = new List<UserCommunicationUserData>(),
            UserCommunicationSecurityAccessRoles = new List<UserCommunicationSecurityAccessRoleData>()
        };

        public UserCommunicationServiceUnitTest(TestFixture fixture) : base(fixture)
        {
            _logger = new Mock<ILogger<UserCommunicationService>>();
            _db = new MockDbBuilder()
                .WithUsers(USERS)
                .WithUserCommunications(USER_COMMUNICATIONS)
                .WithUserCommunicationAcknowledgements(USER_COMM_ACKNOWLEDGEMENTS)
                .Build();

            _userContext.SetupGet(_ => _.UserId).Returns(USER_IDENT_ID);

            CrudService = new UserCommunicationService(_db.Object, _mapper, _logger.Object, _userContext.Object);
        }

        [Fact]
        public override async Task GetCollectionTest()
        {
            await GetCollectionTestHelper<UserCommunicationData>();
        }

        [Fact]
        public override async Task GetByKeyTest()
        {
            await GetByKeyTestHelper(DATA, USER_COMM_ID);
        }

        /// <summary>
        /// Create is not setup in Admin service because the mapping ignores Carrier Properties that are poulated from TOPS
        /// This can be enabled if the Carrier's are ever directly created in Loadshop
        /// </summary>
        /// <returns></returns>
        //[Fact]
        public override async Task CreateTest()
        {
            await CreateTestHelper(DATA, DATA);
        }

        [Fact]
        public override async Task UpdateTest()
        {
            await UpdateTestHelper(DATA, DATA, USER_COMM_ID);
        }

        [Fact]
        public override async Task DeleteTest()
        {
            await DeleteHelper(USER_COMM_ID);
        }

        [Fact]
        public async Task TestCarrierUserValidation()
        {
            var updateData = DATA;
            updateData.AllUsers = false;

            var result = await CrudService.Update(updateData, true, USER_COMM_ID);

            var errors = new List<string>()
            {
                "User Communication does not target any users. Please select at least one target for the communication."
            };


            result.IsValid.Should().BeFalse();
            result.ModelState.Should().HaveCount(1);
            result.ModelState.Select(error => error.Value.Errors.First().ErrorMessage).Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task TestGetUserCommuncationsForDisplay()
        {
            var result = await CrudService.GetUserCommunicationsForDisplay(USER_IDENT_ID);

            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task TestAcknowledgeUserCommuncation()
        {
            var result = await CrudService.Acknowledge(USER_IDENT_ID, new AcknowledgeUserCommunication() { UserCommunicationId = USER_COMM_ID });

            result.Status.Should().Be(CrudResultStatus.Successful);
        }
    }
}

