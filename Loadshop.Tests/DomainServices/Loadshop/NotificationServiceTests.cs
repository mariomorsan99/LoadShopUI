using AutoMapper;
using FluentAssertions;
using Loadshop.Data;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Loadshop.Services.Repositories;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMS.Infrastructure.Common.Configuration;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class NotificationServiceTests
    {
        public class CreateNotificationDetailsAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private Mock<IConfigurationRoot> _config;
            private Mock<ISMSService> _smsService;
            private Mock<IEmailService> _emailService;
            private Mock<IRatingService> _ratingService;
            private Mock<IDateTimeProvider> _dateTime;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private ServiceUtilities _serviceUtilities;

            private NotificationService _svc;

            private List<LoadDetailViewEntity> LOAD_DETAIL_VIEWS = new List<LoadDetailViewEntity>();
            private List<LoadStopEntity> LOAD_STOPS = new List<LoadStopEntity>();
            private List<EquipmentEntity> EQUIPMENT = new List<EquipmentEntity>();
            private List<LoadCarrierScacEntity> LOAD_CARRIER_SCACS = new List<LoadCarrierScacEntity>();
            private List<UserEntity> USERS = new List<UserEntity>();
            private List<NotificationDataEntity> NOTIFICATION_DATA = new List<NotificationDataEntity>();
            private List<UserLaneEntity> USER_LANES = new List<UserLaneEntity>();
            private List<LoadTransactionEntity> LOAD_TRANSACTIONS = new List<LoadTransactionEntity>();
            private List<LoadClaimEntity> CLAIMS = new List<LoadClaimEntity>();
            private List<UserNotificationEntity> USER_NOTIFICATIONS = new List<UserNotificationEntity>();

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid LOAD_TRANSACTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private readonly DateTime NOW = new DateTime(2020, 3, 3, 12, 23, 0);
            private static readonly string SCAC1 = "ABCD";
            private static readonly string SCAC2 = "EFGH";

            public CreateNotificationDetailsAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _config = new Mock<IConfigurationRoot>();

                _smsService = new Mock<ISMSService>();
                _emailService = new Mock<IEmailService>();
                _ratingService = new Mock<IRatingService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                InitScenario_Default();
            }

            private void InitScenario_Default()
            {
                InitScenario_NewLoad_SingleScac();
                InitDb();
                InitService();
            }

            private void InitService()
            {
                _svc = new NotificationService(_db.Object, _mapper, _config.Object, _smsService.Object, _emailService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities, _loadQueryRepository.Object);
            }

            private void InitDb(MockDbBuilder builder = null)
            {
                if (builder == null)
                {
                    builder = new MockDbBuilder();
                }
                _db = builder
                    .WithLoadDetailViews(LOAD_DETAIL_VIEWS)
                    .WithLoadStops(LOAD_STOPS)
                    .WithEquipement(EQUIPMENT)
                    .WithLoadCarrierScacs(LOAD_CARRIER_SCACS)
                    .WithUsers(USERS)
                    .WithUserLanes(USER_LANES)
                    .WithLoadTransactions(LOAD_TRANSACTIONS)
                    .WithLoadClaims(CLAIMS)
                    .WithNotificationData(NOTIFICATION_DATA)
                    .WithUserNotifications(USER_NOTIFICATIONS)
                    .Build();

                _loadQueryRepository.Setup(lqr => lqr.GetLoadDetailViewUnprocessedAsync()).Returns(Task.FromResult(LOAD_DETAIL_VIEWS));
            }

            [Fact]
            public async Task DoesNotProcessIfNoLoadsFound()
            {
                LOAD_DETAIL_VIEWS = new List<LoadDetailViewEntity>();
                InitDb();
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                VerifySaveChanges(Times.Never);
            }

            [Fact]
            public async Task DoesNotProcessIfMissingStops()
            {
                LOAD_STOPS = new List<LoadStopEntity>();
                InitDb();
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                VerifySaveChanges(Times.Never);
            }

            [Fact]
            public void ThrowsExceptionWhenCantFindLoadTransaction()
            {
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>();
                InitDb();
                InitService();

                Func<Task> action = async () => await _svc.CreateNotificationDetailsAsync();
                action.Should().Throw<Exception>($"Unable to find LoadTransaction {LOAD_DETAIL_VIEWS.First().LoadTransactionId}");
            }

            [Fact]
            public async Task NewLoad_SingleScac_ContractRateGreaterThanLineHaul_NoNotificationCreated()
            {
                InitScenario_NewLoad_SingleScac();
                LOAD_CARRIER_SCACS.First().ContractRate = LOAD_DETAIL_VIEWS.First().LineHaulRate + 1;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_SingleScac_ContractRateEqualToLineHaul_NotificationCreated()
            {
                InitScenario_NewLoad_SingleScac();
                LOAD_CARRIER_SCACS.First().ContractRate = LOAD_DETAIL_VIEWS.First().LineHaulRate;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Once);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_SingleScac_ContractRateLessThanLineHaul_NotificationCreated()
            {
                InitScenario_NewLoad_SingleScac();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Once);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_SinlgleScac_UserHasContractRate_NotificationCreatedWithContractRate()
            {
                var origin = LOAD_STOPS.First();
                var dest = LOAD_STOPS.Last();
                var expected = new NotificationDataEntity
                {
                    LoadId = LOAD_ID,
                    UserId = USER_ID,
                    TransactionTypeId = TransactionTypes.New,
                    Origin = $"{origin.City}, {origin.State}",
                    Dest = $"{dest.City}, {dest.State}",
                    LineHaulRate = LOAD_CARRIER_SCACS.First().ContractRate.Value,
                    FuelRate = LOAD_DETAIL_VIEWS.First().FuelRate,
                    EquipmentDesc = EQUIPMENT.First().EquipmentDesc,
                    OriginDtTm = origin.LateDtTm,
                    DestDtTm = dest.LateDtTm,
                    Miles = LOAD_DETAIL_VIEWS.First().Miles,
                    Notification = new NotificationEntity()
                    {
                        MessageTypeId = MessageTypeConstants.Email_SingleCarrierScac
                    }
                };

                InitScenario_NewLoad_SingleScac();
                var builder = new MockDbBuilder();
                InitDb(builder);
                NotificationDataEntity added = null;
                builder.MockNotificationData.Setup(_ => _.Add(It.IsAny<NotificationDataEntity>())).Callback((NotificationDataEntity _) => { added = _; });

                InitService();

                await _svc.CreateNotificationDetailsAsync();
                added.Should().BeEquivalentTo(expected);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_SingleScac_UserDoesNotHaveContractRate_NotificationCreatedWithLineHaulRate()
            {
                var origin = LOAD_STOPS.First();
                var dest = LOAD_STOPS.Last();
                var expected = new NotificationDataEntity
                {
                    LoadId = LOAD_ID,
                    UserId = USER_ID,
                    TransactionTypeId = TransactionTypes.New,
                    Origin = $"{origin.City}, {origin.State}",
                    Dest = $"{dest.City}, {dest.State}",
                    LineHaulRate = LOAD_DETAIL_VIEWS.First().LineHaulRate,
                    FuelRate = LOAD_DETAIL_VIEWS.First().FuelRate,
                    EquipmentDesc = EQUIPMENT.First().EquipmentDesc,
                    OriginDtTm = origin.LateDtTm,
                    DestDtTm = dest.LateDtTm,
                    Miles = LOAD_DETAIL_VIEWS.First().Miles,
                    Notification = new NotificationEntity()
                    {
                        MessageTypeId = MessageTypeConstants.Email_SingleCarrierScac
                    }
                };

                InitScenario_NewLoad_SingleScac();
                LOAD_CARRIER_SCACS.First().ContractRate = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                NotificationDataEntity added = null;
                builder.MockNotificationData.Setup(_ => _.Add(It.IsAny<NotificationDataEntity>())).Callback((NotificationDataEntity _) => { added = _; });

                InitService();

                await _svc.CreateNotificationDetailsAsync();
                added.Should().BeEquivalentTo(expected);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_NotificationExists_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                NOTIFICATION_DATA = new List<NotificationDataEntity>
                {
                    new NotificationDataEntity
                    {
                        LoadId = LOAD_ID,
                        UserId = USER_ID
                    }
                };
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_UserHasNoPrimaryScac_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                USERS.First().PrimaryScac = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_UserPrimaryScacContractRateGreaterThanLineHaul_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                LOAD_CARRIER_SCACS.Where(x => x.Scac == USERS.First().PrimaryScac).First().ContractRate = LOAD_DETAIL_VIEWS.First().LineHaulRate + 1;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_UserHasScacNotFoundInLoadCarrierScacs_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                USERS.First().PrimaryScac = "ZZZZ";
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_UserLaneEquipmentDoesNotMatchLoadEquipment_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                LOAD_DETAIL_VIEWS.First().EquipmentId = "Eq1";
                USER_LANES.First().UserLaneEquipments.First().EquipmentId = "INVALID";
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_UserLaneOriginStateDoesNotMatchFirstLoadStopState_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                LOAD_STOPS.First().State = "WI";
                USER_LANES.First().OrigState = "NA";
                USER_LANES.First().OrigLat = null;
                USER_LANES.First().OrigLng = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_UserLaneDestStateDoesNotMatchLastLoadStopState_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                LOAD_STOPS.Last().State = "WI";
                USER_LANES.First().DestState = "NA";
                USER_LANES.First().DestLat = null;
                USER_LANES.First().DestLng = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_TooMuchOriginDeadHeadDistance_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                USER_LANES.First().OrigLat = 1000m;
                USER_LANES.First().OrigLng = 1000m;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_TooMuchOriginDeadHeadDistanceWithDefaultOrigDH_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                USER_LANES.First().OrigLat = 1000m;
                USER_LANES.First().OrigLng = 1000m;
                USER_LANES.First().OrigDH = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_TooMuchDestDeadHeadDistance_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                USER_LANES.First().DestLat = 1000m;
                USER_LANES.First().DestLng = 1000m;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_TooMuchDestDeadHeadDistanceWithDefaultDestDH_NoNotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                USER_LANES.First().DestLat = 1000m;
                USER_LANES.First().DestLng = 1000m;
                USER_LANES.First().DestDH = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_NotificationAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Once);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task NewLoad_MultipleScac_MultipleUserLaneNotifications_MultipleNotificationsAdded()
            {
                InitScenario_NewLoad_MultipleScacs();
                USER_LANES.First().UserLaneMessageTypes.Add(new UserLaneMessageTypeEntity { MessageTypeId = MessageTypeConstants.CellPhone });
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Exactly(2));
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task AcceptedLoad_NoClaimOnLoadTransaction_NoNotificationAdded()
            {
                InitScenario_AcceptedLoad();
                CLAIMS = new List<LoadClaimEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task AcceptedLoad_NoClaimUserId_NoNotificationAdded()
            {
                InitScenario_AcceptedLoad();
                CLAIMS.First().UserId = default(Guid);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task AcceptedLoad_NoUserNotifications_NoNotificationAdded()
            {
                InitScenario_AcceptedLoad();
                USER_NOTIFICATIONS = new List<UserNotificationEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task AcceptedLoad_NotificationsAdded()
            {
                InitScenario_AcceptedLoad();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Exactly(2));
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task PendingLoad_NoClaimOnLoadTransaction_NoNotificationAdded()
            {
                InitScenario_PendingLoad();
                CLAIMS = new List<LoadClaimEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task PendingLoad_NoClaimUserId_NoNotificationAdded()
            {
                InitScenario_PendingLoad();
                CLAIMS.First().UserId = default(Guid);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task PendingLoad_NoUserNotifications_NoNotificationAdded()
            {
                InitScenario_PendingLoad();
                USER_NOTIFICATIONS = new List<UserNotificationEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task PendingLoad_NotificationsAdded()
            {
                InitScenario_PendingLoad();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Exactly(2));
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task RemovedLoad_NoPendingLoadTransaction_NoNotificationAdded()
            {
                InitScenario_RemovedLoad();
                LOAD_TRANSACTIONS.RemoveAll(x => x.TransactionTypeId == TransactionTypes.Pending);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }
            [Fact]
            public async Task RemovedLoad_NoClaimOnLoadTransaction_NoNotificationAdded()
            {
                InitScenario_RemovedLoad();
                LOAD_TRANSACTIONS.Where(x => x.TransactionTypeId == TransactionTypes.Pending).First().Claim = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task RemovedLoad_NoClaimUserId_NoNotificationAdded()
            {
                InitScenario_RemovedLoad();
                CLAIMS.First().UserId = default(Guid);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task RemovedLoad_NoUserNotifications_NoNotificationAdded()
            {
                InitScenario_RemovedLoad();
                USER_NOTIFICATIONS = new List<UserNotificationEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task RemovedLoad_NotificationsAdded()
            {
                InitScenario_RemovedLoad();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Exactly(2));
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task UpdatedLoad_NoPendingLoadTransaction_NoNotificationAdded()
            {
                InitScenario_UpdatedLoad();
                LOAD_TRANSACTIONS.RemoveAll(x => x.TransactionTypeId == TransactionTypes.Pending);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }
            [Fact]
            public async Task UpdatedLoad_NoClaimOnLoadTransaction_NoNotificationAdded()
            {
                InitScenario_UpdatedLoad();
                LOAD_TRANSACTIONS.Where(x => x.TransactionTypeId == TransactionTypes.Pending).First().Claim = null;
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task UpdatedLoad_NoClaimUserId_NoNotificationAdded()
            {
                InitScenario_UpdatedLoad();
                CLAIMS.First().UserId = default(Guid);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task UpdatedLoad_NoUserNotifications_NoNotificationAdded()
            {
                InitScenario_UpdatedLoad();
                USER_NOTIFICATIONS = new List<UserNotificationEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task UpdatedLoad_NotificationsAdded()
            {
                InitScenario_UpdatedLoad();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationDetailsAsync();
                builder.MockNotificationData.Verify(x => x.Add(It.IsAny<NotificationDataEntity>()), Times.Exactly(2));
                VerifySaveChanges(Times.Once);
            }


            private void InitScenario_NewLoad_SingleScac()
            {
                InitCommonSeedData(TransactionTypes.New);
                LOAD_CARRIER_SCACS = new List<LoadCarrierScacEntity>
                {
                    new LoadCarrierScacEntity
                    {
                        LoadId = LOAD_ID,
                        Scac = SCAC1,
                        ContractRate = 99m,
                        CarrierScac = new CarrierScacEntity()
                        {
                            Scac = SCAC1
                        }
                    }
                };
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        PrimaryScac = SCAC1
                    }
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        LoadId = LOAD_ID,
                        TransactionTypeId = TransactionTypes.New
                    }
                };
            }

            private void InitScenario_NewLoad_MultipleScacs()
            {
                InitCommonSeedData(TransactionTypes.New);
                LOAD_CARRIER_SCACS = new List<LoadCarrierScacEntity>
                {
                    new LoadCarrierScacEntity
                    {
                        LoadId = LOAD_ID,
                        Scac = SCAC1,
                        ContractRate = 99m,
                        CarrierScac = new CarrierScacEntity()
                        {
                            Scac = SCAC1
                        }
                    },
                    new LoadCarrierScacEntity
                    {
                        LoadId = LOAD_ID,
                        Scac = SCAC2,
                        ContractRate = 99m,
                        CarrierScac = new CarrierScacEntity()
                        {
                            Scac = SCAC2
                        }
                    }
                };
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        PrimaryScac = SCAC1,
                        IsNotificationsEnabled = true
                    }
                };
                USER_LANES = new List<UserLaneEntity>
                {
                    new UserLaneEntity
                    {
                        OrigState = "WI",
                        OrigLat = 1.2m,
                        OrigLng = 1.2m,
                        OrigDH = 20,
                        DestState = "WI",
                        DestLat = 1.2m,
                        DestLng = 1.2m,
                        DestDH = 20,
                        UserLaneMessageTypes = new List<UserLaneMessageTypeEntity>
                        {
                            new UserLaneMessageTypeEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email
                            }
                        },
                        UserLaneEquipments = new List<UserLaneEquipmentEntity>
                        {
                            new UserLaneEquipmentEntity
                            {
                                EquipmentId = "Eq1"
                            }
                        },
                        User = USERS.First(),
                        UserId = USER_ID
                    }
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        LoadId = LOAD_ID,
                        TransactionTypeId = TransactionTypes.New
                    }
                };
            }

            private void InitScenario_AcceptedLoad()
            {
                InitCommonSeedData(TransactionTypes.Accepted);
                CLAIMS = new List<LoadClaimEntity>
                {
                    new LoadClaimEntity
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        UserId = USER_ID
                    }
                };
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "user@email.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
            }

            private void InitScenario_PendingLoad()
            {
                InitCommonSeedData(TransactionTypes.Pending);
                CLAIMS = new List<LoadClaimEntity>
                {
                    new LoadClaimEntity
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        UserId = USER_ID
                    }
                };
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "user@email.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
            }

            private void InitScenario_RemovedLoad()
            {
                InitCommonSeedData(TransactionTypes.Removed);
                CLAIMS = new List<LoadClaimEntity>
                {
                    new LoadClaimEntity
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        UserId = USER_ID
                    }
                };
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "user@email.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        TransactionTypeId = TransactionTypes.Removed,
                        CreateDtTm = NOW
                    },
                    new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        TransactionTypeId = TransactionTypes.Pending,
                        CreateDtTm = NOW.AddDays(-3),
                        Claim = CLAIMS.First()
                    }
                };
            }

            private void InitScenario_UpdatedLoad()
            {
                InitCommonSeedData(TransactionTypes.Updated);
                CLAIMS = new List<LoadClaimEntity>
                {
                    new LoadClaimEntity
                    {
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        UserId = USER_ID
                    }
                };
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "user@email.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
                LOAD_TRANSACTIONS = new List<LoadTransactionEntity>
                {
                    new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        TransactionTypeId = TransactionTypes.Updated,
                        CreateDtTm = NOW
                    },
                    new LoadTransactionEntity
                    {
                        LoadId = LOAD_ID,
                        TransactionTypeId = TransactionTypes.Pending,
                        CreateDtTm = NOW.AddDays(-3),
                        Claim = CLAIMS.First()
                    }
                };
            }

            private void InitCommonSeedData(string transactionTypeId)
            {
                LOAD_DETAIL_VIEWS = new List<LoadDetailViewEntity>
                {
                    new LoadDetailViewEntity
                    {
                        LoadId = LOAD_ID,
                        EquipmentId = "Eq1",
                        TransactionTypeId = transactionTypeId,
                        LoadTransactionId = LOAD_TRANSACTION_ID,
                        LineHaulRate = 100m,
                        FuelRate = 9.99m,
                        Miles = 10
                    }
                };
                LOAD_STOPS = new List<LoadStopEntity>
                {
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 1,
                        City = "Origin City",
                        State = "WI",
                        LateDtTm = NOW.AddDays(-2),
                        Latitude = 1.2m,
                        Longitude = 1.2m
                    },
                    new LoadStopEntity
                    {
                        LoadId = LOAD_ID,
                        StopNbr = 2,
                        City = "Dest City",
                        State = "WI",
                        LateDtTm = NOW.AddDays(-1),
                        Latitude = 1.2m,
                        Longitude = 1.2m
                    }
                };
                EQUIPMENT = new List<EquipmentEntity>
                {
                    new EquipmentEntity
                    {
                        EquipmentId = "Eq1",
                        EquipmentDesc = "Equipment 1"
                    }
                };
            }

            private void VerifySaveChanges(Func<Times> times)
            {
                _db.Verify(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), times);
            }
        }

        public class CreateNotificationsAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private Mock<IConfigurationRoot> _config;
            private Mock<ISMSService> _smsService;
            private Mock<IEmailService> _emailService;
            private Mock<IRatingService> _ratingService;
            private Mock<IDateTimeProvider> _dateTime;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private ServiceUtilities _serviceUtilities;

            private NotificationService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private readonly DateTime NOW = new DateTime(2020, 3, 3, 12, 23, 0);
            private static readonly string REASON = "Reason for removal";
            private static readonly string LOADSHOP_URL = "https://www.loadshop.com";

            private List<NotificationEntity> NOTIFICATIONS = new List<NotificationEntity>();
            private NotificationDataEntity NOTIFICATION_DATUM = new NotificationDataEntity();
            private UserEntity USER = new UserEntity();
            private List<UserNotificationEntity> USER_NOTIFICATIONS = new List<UserNotificationEntity>();
            private LoadEntity LOAD = new LoadEntity();
            private List<LoadContactEntity> LOAD_CONTACTS = new List<LoadContactEntity>();

            public CreateNotificationsAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _config = new Mock<IConfigurationRoot>();
                _config.SetupGet(x => x["LoadShopUrl"]).Returns(LOADSHOP_URL);

                _smsService = new Mock<ISMSService>();
                _emailService = new Mock<IEmailService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();

                _ratingService = new Mock<IRatingService>();
                _ratingService.Setup(x => x.GetRatingReason(It.IsAny<Guid>())).ReturnsAsync(REASON);

                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email);
                InitDb();
                InitService();
            }

            private void InitService()
            {
                _svc = new NotificationService(_db.Object, _mapper, _config.Object, _smsService.Object, _emailService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities, _loadQueryRepository.Object);
            }

            private void InitDb(MockDbBuilder builder = null)
            {
                if (builder == null)
                {
                    builder = new MockDbBuilder();
                }
                _db = builder
                    .WithNotifications(NOTIFICATIONS)
                    .WithUserNotifications(USER_NOTIFICATIONS)
                    .WithUser(USER)
                    .Build();
            }

            [Fact]
            public async Task DoesNotProcessIfNoNotificationsFound()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email);
                NOTIFICATIONS = new List<NotificationEntity>();
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                VerifySaveChanges(Times.Never);
            }

            [Fact]
            public void ThrowsExceptionWhenDBThrowsException()
            {
                var expected = new Exception("DB Exception");
                _db.Setup(x => x.Notifications).Throws(expected);
                InitService();

                Func<Task> action = async () => await _svc.CreateNotificationsAsync();
                action.Should().Throw<Exception>(expected.Message);
            }

            [Fact]
            public async Task GuardDeniesAccess_NoNotificationMessagesAdded()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email);
                // Wipe all security app actions associated with user's security access role
                USER.SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>
                    {
                        new SecurityUserAccessRoleEntity
                        {
                            SecurityAccessRole = new SecurityAccessRoleEntity
                            {
                                SecurityAccessRoleAppActions = new List<SecurityAccessRoleAppActionEntity>()
                            }
                        }
                    };
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task Single_New_Email_NotificationMessagesAdded()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Once);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task Single_New_Email_MessagesSent()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email);
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), false), Times.Once);
            }

            [Fact]
            public async Task Single_New_Email_SubjectMatches()
            {
                var expected = $"Loadshop Favorite Match - {NOTIFICATION_DATUM.Origin} ({NOTIFICATION_DATUM.OriginDtTm.ToString("MM/dd/yyyy")}) to {NOTIFICATION_DATUM.Dest} ({NOTIFICATION_DATUM.DestDtTm.ToString("MM/dd/yyyy HH:mm")}) - {NOTIFICATION_DATUM.EquipmentDesc}";
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                added.First().Subject.Should().Be(expected);
            }

            [Fact]
            public async Task Single_New_Email_BodyContainsAllRequiredFields()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                var body = added.First().Message;
                var n = NOTIFICATION_DATUM;
                body.Should().Contain(LOADSHOP_URL);
                body.Should().Contain(AuditTypeData.FavoritesMatchEmailView.ToString("G"));
                body.Should().Contain(n.LoadId.ToString("D"));
                body.Should().Contain(n.Load.ReferenceLoadDisplay);
                body.Should().Contain(n.Origin);
                body.Should().Contain(n.OriginDtTm.ToString("MM/dd/yyyy HH:mm"));
                body.Should().Contain(n.Dest);
                body.Should().Contain(n.DestDtTm.ToString("MM/dd/yyyy HH:mm"));
                body.Should().Contain(n.EquipmentDesc);
                body.Should().Contain(n.LineHaulRate.ToString());
                body.Should().Contain(n.FuelRate.ToString());
                var totalRate = n.LineHaulRate + n.FuelRate;
                body.Should().Contain(totalRate.ToString());
            }

            [Fact]
            public async Task Single_New_CellPhone_NotificationMessagesAdded()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.CellPhone);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Once);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task Single_New_CellPhone_SmsMessagesSent()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.CellPhone);
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                // ONE (1) CellPhone notification translates into TWO (2) SMS Messages
                _smsService.Verify(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            }

            [Fact]
            public async Task Single_New_CellPhone_SubjectMatches()
            {
                var expected = $"Load Matched - {NOTIFICATION_DATUM.Origin} to {NOTIFICATION_DATUM.Dest} - {NOTIFICATION_DATUM.EquipmentDesc}";
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.CellPhone);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                added.First().Subject.Should().Be(expected);
            }

            [Fact]
            public async Task Single_New_CellPhone_BodyContainsAllRequiredFields()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.CellPhone);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                var body = added.First().Message;
                var n = NOTIFICATION_DATUM;

                body.Should().Contain(n.Origin);
                body.Should().Contain(n.Dest);
                body.Should().Contain(n.OriginDtTm.ToString("MM/dd/yyyy HH:mm"));
                var totalRate = n.LineHaulRate + n.FuelRate;
                body.Should().Contain(totalRate.ToString());
                body.Should().Contain(LOADSHOP_URL);
                body.Should().Contain(AuditTypeData.FavoritesMatchEmailView.ToString("G"));
                body.Should().Contain(n.LoadId.ToString("D"));
            }

            [Fact]
            public async Task Single_New_EmailSingleCarrierScac_NotificationMessagesAdded()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email_SingleCarrierScac);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Once);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task Single_New_EmailSingleCarrierScac_MessagesSent()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email_SingleCarrierScac);
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), false), Times.Once);
            }

            [Fact]
            public async Task Single_New_EmailSingleCarrierScac_SubjectMatches()
            {
                var expected = $"Loadshop Ready To Book - {NOTIFICATION_DATUM.Origin} ({NOTIFICATION_DATUM.OriginDtTm.ToString("MM/dd/yyyy")}) to {NOTIFICATION_DATUM.Dest} ({NOTIFICATION_DATUM.DestDtTm.ToString("MM/dd/yyyy HH:mm")}) - {NOTIFICATION_DATUM.EquipmentDesc}";
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email_SingleCarrierScac);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                added.First().Subject.Should().Be(expected);
            }

            [Fact]
            public async Task Single_New_EmailSingleCarrierScac_BodyContainsAllRequiredFields()
            {
                InitScenarion_Single(TransactionTypes.New, MessageTypeConstants.Email_SingleCarrierScac);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                var body = added.First().Message;
                var n = NOTIFICATION_DATUM;
                body.Should().Contain(LOADSHOP_URL);
                body.Should().Contain(AuditTypeData.ReadyToBookEmailView.ToString("G"));
                body.Should().Contain(n.LoadId.ToString("D"));
                body.Should().Contain(n.Load.ReferenceLoadDisplay);
                body.Should().Contain(n.Origin);
                body.Should().Contain(n.OriginDtTm.ToString("MM/dd/yyyy HH:mm"));
                body.Should().Contain(n.Dest);
                body.Should().Contain(n.DestDtTm.ToString("MM/dd/yyyy HH:mm"));
                body.Should().Contain(n.EquipmentDesc);
                body.Should().Contain(n.LineHaulRate.ToString());
                body.Should().Contain(n.FuelRate.ToString());
                var totalRate = n.LineHaulRate + n.FuelRate;
                body.Should().Contain(totalRate.ToString());
            }

            [Fact]
            public async Task Single_Pending_Email_NoNotificationsBuilt()
            {
                InitScenarion_Single(TransactionTypes.Pending, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<NotificationMessageEntity[]>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Single_Pending_EmailSingleCarrierScac_NoNotificationsBuilt()
            {
                InitScenarion_Single(TransactionTypes.Pending, MessageTypeConstants.Email_SingleCarrierScac);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<NotificationMessageEntity[]>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Single_Pending_CellPhone_NoNotificationsBuilt()
            {
                InitScenarion_Single(TransactionTypes.Pending, MessageTypeConstants.CellPhone);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<NotificationMessageEntity[]>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Single_Accepted_Email_NoNotificationsBuilt()
            {
                InitScenarion_Single(TransactionTypes.Accepted, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<NotificationMessageEntity[]>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Single_Accepted_EmailSingleCarrierScac_NoNotificationsBuilt()
            {
                InitScenarion_Single(TransactionTypes.Accepted, MessageTypeConstants.Email_SingleCarrierScac);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<NotificationMessageEntity[]>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Single_Accepted_CellPhone_NoNotificationsBuilt()
            {
                InitScenarion_Single(TransactionTypes.Accepted, MessageTypeConstants.CellPhone);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<NotificationMessageEntity[]>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Single_Removed_Email_NotificationMessagesAdded()
            {
                InitScenarion_Single(TransactionTypes.Removed, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Once);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task Single_Removed_Email_MessagesSent()
            {
                InitScenarion_Single(TransactionTypes.Removed, MessageTypeConstants.Email);
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), false), Times.Once);
            }

            [Fact]
            public async Task Single_Removed_Email_SubjectMatches()
            {
                var expected = $"Booking Cancelled – load # {NOTIFICATION_DATUM.Load.ReferenceLoadDisplay} {NOTIFICATION_DATUM.Origin} to {NOTIFICATION_DATUM.Dest} - {NOTIFICATION_DATUM.EquipmentDesc}";
                InitScenarion_Single(TransactionTypes.Removed, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                added.First().Subject.Should().Be(expected);
            }

            [Fact]
            public async Task Single_Removed_Email_BodyContainsAllRequiredFields()
            {
                InitScenarion_Single(TransactionTypes.Removed, MessageTypeConstants.Email);
                var builder = new MockDbBuilder();
                InitDb(builder);

                IEnumerable<NotificationMessageEntity> added = null;
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added = x; });

                InitService();

                await _svc.CreateNotificationsAsync();
                var body = added.First().Message;
                var n = NOTIFICATION_DATUM;
                body.Should().Contain(LOADSHOP_URL);
                body.Should().Contain(n.User.FirstName);
                body.Should().Contain(REASON);
            }

            [Fact]
            public async Task Single_Removed_CellPhone_NoNotificationMessagesAdded()
            {
                InitScenarion_Single(TransactionTypes.Removed, MessageTypeConstants.CellPhone);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Never);
                VerifySaveChanges(Times.Once);
            }

            [Fact]
            public async Task Single_Removed_CellPhone_NoSmsMessagesSent()
            {
                InitScenarion_Single(TransactionTypes.Removed, MessageTypeConstants.CellPhone);
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                _smsService.Verify(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public async Task Multiple_New_NotificationsBuilt()
            {
                InitScenario_Multiple(TransactionTypes.New);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Exactly(NOTIFICATIONS.Count));
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Multiple_New_MessagesSent()
            {
                InitScenario_Multiple(TransactionTypes.New);
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), false), Times.Exactly(2));
                // ONE (1) CellPhone notification translates into TWO (2) SMS Messages
                _smsService.Verify(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Multiple_Pending_NoNotificationsBuilt()
            {
                InitScenario_Multiple(TransactionTypes.Pending);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Multiple_Accepted_NoNotificationsBuilt()
            {
                InitScenario_Multiple(TransactionTypes.Accepted);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.CreateNotificationsAsync();
                builder.MockNotificationMessages.Verify(x => x.AddRange(It.IsAny<List<NotificationMessageEntity>>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Multiple_Removed_EmailNotificationsBuilt_NoSmsMessagesBuilt()
            {
                InitScenario_Multiple(TransactionTypes.Removed);
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var added = new List<NotificationMessageEntity>();
                builder.MockNotificationMessages
                    .Setup(x => x.AddRange(It.IsAny<IEnumerable<NotificationMessageEntity>>()))
                    .Callback((IEnumerable<NotificationMessageEntity> x) => { added.AddRange(x); });


                await _svc.CreateNotificationsAsync();
                added.Should().HaveCount(2);
                added.ForEach(x => x.To.Should().Be(USER_NOTIFICATIONS.First().NotificationValue));
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            [Fact]
            public async Task Multiple_Removed_EmailMessagesSent_NoSmsMessagesSent()
            {
                InitScenario_Multiple(TransactionTypes.Removed);
                InitDb();
                InitService();

                await _svc.CreateNotificationsAsync();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), false), Times.Exactly(2));
                _smsService.Verify(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                VerifySaveChanges(() => Times.Exactly(NOTIFICATIONS.Count));
            }

            private void InitScenario_CommonData()
            {
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "notify@email.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    FirstName = "John",
                    UserNotifications = USER_NOTIFICATIONS,
                    SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>
                    {
                        new SecurityUserAccessRoleEntity
                        {
                            SecurityAccessRole = new SecurityAccessRoleEntity
                            {
                                SecurityAccessRoleAppActions = new List<SecurityAccessRoleAppActionEntity>
                                {
                                    new SecurityAccessRoleAppActionEntity
                                    {
                                        AppActionId = SecurityActions.Loadshop_Notifications_Email_Favorites
                                    },
                                    new SecurityAccessRoleAppActionEntity
                                    {
                                        AppActionId = SecurityActions.Loadshop_Notification_Email_Booked_Loads
                                    },
                                    new SecurityAccessRoleAppActionEntity
                                    {
                                        AppActionId = SecurityActions.Loadshop_Notification_Email_Load_Cancelled
                                    }
                                }
                            }
                        }
                    }
                };
                LOAD_CONTACTS = new List<LoadContactEntity>
                {
                    new LoadContactEntity
                    {
                        LoadId = LOAD_ID,
                        Email = "contact@email.com",
                        Phone = "123-123-1234",
                        Display = "John Doe"
                    }
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadDisplay = "RefLoadDisplay",
                    Contacts = LOAD_CONTACTS
                };
            }

            private void InitScenarion_Single(string transactionTypeId, string messageType)
            {
                InitScenario_CommonData();
                NOTIFICATION_DATUM = new NotificationDataEntity
                {
                    LoadId = LOAD_ID,
                    Load = LOAD,
                    UserId = USER_ID,
                    User = USER,
                    TransactionTypeId = transactionTypeId,
                    Origin = "Origin",
                    OriginDtTm = NOW.AddDays(-3),
                    Dest = "Dest",
                    DestDtTm = NOW.AddDays(-2),
                    EquipmentDesc = "Equipment Desc",
                    LineHaulRate = 100m,
                    FuelRate = 99m
                };
                NOTIFICATIONS = new List<NotificationEntity>
                {
                    new NotificationEntity
                    {
                        MessageTypeId = messageType,
                        NotificationData = NOTIFICATION_DATUM,
                        ProcessedDtTm = null
                    }
                };
            }

            private void InitScenario_Multiple(string transactionTypeId)
            {
                InitScenario_CommonData();
                NOTIFICATIONS = new List<NotificationEntity>
                {
                    new NotificationEntity
                    {
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationData = new NotificationDataEntity
                        {
                            LoadId = LOAD_ID,
                            Load = LOAD,
                            UserId = USER_ID,
                            User = USER,
                            TransactionTypeId = transactionTypeId,
                            Origin = "Origin",
                            OriginDtTm = NOW.AddDays(-3),
                            Dest = "Dest",
                            DestDtTm = NOW.AddDays(-2),
                            EquipmentDesc = "Equipment Desc",
                            LineHaulRate = 100m,
                            FuelRate = 99m
                        },
                        ProcessedDtTm = null
                    },
                    new NotificationEntity
                    {
                        MessageTypeId = MessageTypeConstants.Email_SingleCarrierScac,
                        NotificationData = new NotificationDataEntity
                        {
                            LoadId = LOAD_ID,
                            Load = LOAD,
                            UserId = USER_ID,
                            User = USER,
                            TransactionTypeId = transactionTypeId,
                            Origin = "Origin",
                            OriginDtTm = NOW.AddDays(-3),
                            Dest = "Dest",
                            DestDtTm = NOW.AddDays(-2),
                            EquipmentDesc = "Equipment Desc",
                            LineHaulRate = 100m,
                            FuelRate = 99m
                        },
                        ProcessedDtTm = null
                    },
                    new NotificationEntity
                    {
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationData = new NotificationDataEntity
                        {
                            LoadId = LOAD_ID,
                            Load = LOAD,
                            UserId = USER_ID,
                            User = USER,
                            TransactionTypeId = transactionTypeId,
                            Origin = "Origin",
                            OriginDtTm = NOW.AddDays(-3),
                            Dest = "Dest",
                            DestDtTm = NOW.AddDays(-2),
                            EquipmentDesc = "Equipment Desc",
                            LineHaulRate = 100m,
                            FuelRate = 99m
                        },
                        ProcessedDtTm = null
                    }
                };
            }

            private void VerifySaveChanges(Func<Times> times)
            {
                _db.Verify(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), times);
            }
        }

        public class SendPendingEmailTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private Mock<IConfigurationRoot> _config;
            private Mock<ISMSService> _smsService;
            private Mock<IEmailService> _emailService;
            private Mock<IRatingService> _ratingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;

            private NotificationService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string LOADSHOP_EMAIL = "loadshop@email.com";
            private static int CC_TO_LOADSHOP_THRESHOLD = 2;
            private static readonly string SCAC = "SCAC";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string CARRIER_NAME = "CarrierName";
            private readonly DateTime NOW = new DateTime(2020, 3, 3, 12, 23, 0);
            private static readonly string USER_CONTEXT_EMAIL = "user@email.com";

            private List<UserNotificationEntity> USER_NOTIFICATIONS;
            private List<LoadViewEntity> BOOKED_LOADS;
            private LoadEntity LOAD;
            private UserEntity USER;
            private LoadContactEntity LOAD_CONTACT;
            private List<ServiceTypeEntity> SERVICE_TYPES;
            private List<LoadServiceTypeEntity> LOAD_SERVICE_TYPES;
            private List<CarrierScacEntity> CARRIER_SCACS;
            private List<CarrierEntity> CARRIERS;
            private CustomerEntity SHIPPER;
            private LoadClaimEntity LOAD_CLAIM;

            public SendPendingEmailTests(TestFixture fixture)
            {
                InitSeedData();

                _mapper = fixture.Mapper;
                var options = new ConfigManagerOptions()
                {
                    ProcessNames = new List<string>() { "Loadshop.Web.API" },
                };
                _config = new Mock<IConfigurationRoot>();
                _config.SetupGet(x => x["LoadBoardNumberOfBookedLoadsToCCLoadshopEmail"]).Returns(CC_TO_LOADSHOP_THRESHOLD.ToString());
                _config.SetupGet(x => x["LoadshopEmail"]).Returns(LOADSHOP_EMAIL);

                _smsService = new Mock<ISMSService>();
                _emailService = new Mock<IEmailService>();
                _ratingService = new Mock<IRatingService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();
                _loadQueryRepository.Setup(ls => ls.GetNumberOfBookedLoadsForCarrierByUserIdentId(It.IsAny<Guid>())).Returns(5);
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);


                InitDb();
                InitService();
            }

            private void InitService()
            {
                _svc = new NotificationService(_db.Object, _mapper, _config.Object, _smsService.Object, _emailService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities, _loadQueryRepository.Object);
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithUserNotifications(USER_NOTIFICATIONS)
                    .WithBookedLoads(BOOKED_LOADS)
                    .WithServiceTypes(SERVICE_TYPES)
                    .WithLoadServiceTypes(LOAD_SERVICE_TYPES)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .WithCarriers(CARRIERS)
                    .WithCustomer(SHIPPER)
                    .Build();
            }

            [Fact]
            public void NoLoad_NoMessageSent()
            {
                var response = _svc.SendPendingEmail(null, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void NoUser_NoMessageSent()
            {
                var response = _svc.SendPendingEmail(LOAD, null, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void NoLoadContact_NoMessageSent()
            {
                var response = _svc.SendPendingEmail(LOAD, USER, null, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void MessageShouldBeSent()
            {
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Should().NotBeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Once);
            }

            [Fact]
            public void SubjectHasRefLoadDisplay()
            {
                var expected = $"Success! Loadshop Booking Confirmation for # {LOAD.ReferenceLoadDisplay}";
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Subject.Should().Contain(expected);
            }

            [Fact]
            public void SubjectHasOriginToDestDescription()
            {
                var origin = LOAD.LoadStops.Where(x => x.StopNbr == 1).FirstOrDefault();
                var dest = LOAD.LoadStops.Where(x => x.StopNbr == 2).FirstOrDefault();
                var expected = $"{origin.City}, {origin.State} to {dest.City}, {dest.State} - {LOAD.Equipment.EquipmentDesc}";

                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Subject.Should().Contain(expected);
            }

            [Fact]
            public void MessageSentToUser()
            {
                var userEmail = USER.UserNotifications.FirstOrDefault(x => x.MessageTypeId == MessageTypeConstants.Email).NotificationValue;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.To.Should().Be(userEmail);
            }

            [Fact]
            public void MessageCopiedToLoadContact()
            {
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.CC.Should().Be(LOAD_CONTACT.Email);
            }

            [Fact]
            public void MessageCopiedToLoadshopWhenFewerThanConfiguredLoadsBookedByUser()
            {
                _config.SetupGet(x => x["LoadBoardNumberOfBookedLoadsToCCLoadshopEmail"]).Returns("10");
                InitService();

                var expected = $"{LOAD_CONTACT.Email};{LOADSHOP_EMAIL}";
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.CC.Should().Be(expected);
            }

            [Fact]
            public void BodyDoesNotContainFuelEstimateDisclaimer()
            {
                var expected = "fuel cost listed above is estimated";
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().NotContain(expected);
            }

            [Fact]
            public void BodyContainsFuelEstimateDisclaimerWhenPickupDateMoreThanFiveDaysOld()
            {
                LOAD.LoadStops.First().LateDtTm = NOW.AddDays(7);
                LOAD.LoadStops.Last().LateDtTm = NOW.AddDays(6);
                SHIPPER.UseFuelRerating = true;
                SHIPPER.FuelReratingNumberOfDays = 5;
                InitDb();
                InitService();

                var expected = "fuel cost listed above is estimated";
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsLoadContactFirstName()
            {
                var expected = USER.FirstName;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsUserCellPhone()
            {
                var expected = USER.UserNotifications.FirstOrDefault(x => x.MessageTypeId == MessageTypeConstants.CellPhone).NotificationValue;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsShipperName()
            {
                var expected = SHIPPER.Name;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsServiceTypes()
            {
                var expected = string.Join(", ", SERVICE_TYPES.Select(x => x.Name).ToList());
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsOperatingAuthorityNumber()
            {
                var expected = CARRIERS.First().OperatingAuthNbr;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsLineHaulMinusLoadshopFee()
            {
                var expected = LOAD.LineHaulRate - LOAD_CLAIM.LoadshopFee;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain($"<td>${expected.ToString("0.00")}</td>");
            }

            [Fact]
            public void BodyContainsLineHaulWhenFeeIsAdded()
            {
                LOAD_CLAIM.FeeAdd = true;
                var expected = LOAD.LineHaulRate;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, LOAD_CLAIM);
                response.Message.Should().Contain($"<td>${expected.ToString("0.00")}</td>");
            }

            [Fact]
            public void BodyContainsLineHaulWhenClaimIsNull()
            {
                var expected = LOAD.LineHaulRate;
                var response = _svc.SendPendingEmail(LOAD, USER, LOAD_CONTACT, USER_CONTEXT_EMAIL, null);
                response.Message.Should().Contain($"<td>${expected.ToString("0.00")}</td>");
            }

            private void InitSeedData()
            {
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "email@test.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
                BOOKED_LOADS = new List<LoadViewEntity>
                {
                    new LoadViewEntity(),
                    new LoadViewEntity(),
                    new LoadViewEntity()
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadDisplay = "RefLoadDisplay",
                    LineHaulRate = 10M,
                    FuelRate = 5M,
                    CustomerId = CUSTOMER_ID,
                    LoadStops = new List<LoadStopEntity>
                    {
                        new LoadStopEntity
                        {
                            StopNbr = 1,
                            City = "Origin City",
                            State = "WI",
                            Country = "USA",
                            ApptType = "Origin Appt Type",
                            EarlyDtTm = null,
                            LateDtTm = NOW.AddDays(1)
                        },
                        new LoadStopEntity
                        {
                            StopNbr = 2,
                            City = "Dest City",
                            State = "WI",
                            Country = "USA",
                            ApptType = "Dest Appt Type",
                            EarlyDtTm = null,
                            LateDtTm = NOW.AddDays(2)
                        }
                    },
                    Equipment = new EquipmentEntity
                    {
                        EquipmentDesc = "Equipment Description"
                    }
                };
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    FirstName = "John",
                    LastName = "Doe",
                    IdentUserId = USER_ID,
                    UserNotifications = USER_NOTIFICATIONS,
                    PrimaryScac = SCAC
                };
                LOAD_CONTACT = new LoadContactEntity
                {
                    Phone = "123-123-1234",
                    Email = "email@address.com",
                    Display = $"{USER.FirstName} {USER.LastName}"
                };
                SERVICE_TYPES = new List<ServiceTypeEntity>
                {
                    new ServiceTypeEntity
                    {
                        ServiceTypeId = 1,
                        Name = "Service 1",
                    },
                    new ServiceTypeEntity
                    {
                        ServiceTypeId = 2,
                        Name = "Service 2",
                    }
                };
                LOAD_SERVICE_TYPES = new List<LoadServiceTypeEntity>
                {
                    new LoadServiceTypeEntity
                    {
                        LoadId = LOAD_ID,
                        ServiceTypeId = 1
                    },
                    new LoadServiceTypeEntity
                    {
                        LoadId = LOAD_ID,
                        ServiceTypeId = 2
                    }
                };
                CARRIER_SCACS = new List<CarrierScacEntity>
                {
                    new CarrierScacEntity
                    {
                        Scac = SCAC,
                        CarrierId = CARRIER_ID,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            CarrierName = CARRIER_NAME,
                            OperatingAuthNbr = "Operating Auth Nbr"
                        }
                    }
                };
                CARRIERS = new List<CarrierEntity>
                {
                    CARRIER_SCACS.First().Carrier
                };
                SHIPPER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID,
                    Name = "ShipperName"
                };
                LOAD_CLAIM = new LoadClaimEntity
                {
                    FlatFee = 1m,
                    PercentFee = 0.0125m,
                    FeeAdd = false,
                    LoadshopFee = 1.5m
                };
            }
        }

        public class SendCarrierRemovedEmailTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private Mock<IConfigurationRoot> _config;
            private Mock<ISMSService> _smsService;
            private Mock<IEmailService> _emailService;
            private Mock<IRatingService> _ratingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;

            private NotificationService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string LOADSHOP_URL = "https://www.loadshop.com";
            private static readonly string SCAC = "SCAC";
            private readonly DateTime NOW = new DateTime(2020, 3, 3, 12, 23, 0);
            private static readonly string USER_CONTEXT_EMAIL = "user@email.com";
            private static readonly string REASON = "reason";

            private List<UserNotificationEntity> USER_NOTIFICATIONS;
            private LoadEntity LOAD;
            private UserEntity USER;
            private List<LoadContactEntity> LOAD_CONTACTS;

            public SendCarrierRemovedEmailTests(TestFixture fixture)
            {
                InitSeedData();

                _mapper = fixture.Mapper;
                var options = new ConfigManagerOptions()
                {
                    ProcessNames = new List<string>() { "Loadshop.Web.API" },
                };
                _config = new Mock<IConfigurationRoot>();
                _config.SetupGet(x => x["LoadShopUrl"]).Returns(LOADSHOP_URL);

                _smsService = new Mock<ISMSService>();
                _emailService = new Mock<IEmailService>();
                _ratingService = new Mock<IRatingService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                InitDb();
                InitService();
            }

            private void InitService()
            {
                _svc = new NotificationService(_db.Object, _mapper, _config.Object, _smsService.Object, _emailService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities, _loadQueryRepository.Object);
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithUserNotifications(USER_NOTIFICATIONS)
                    .Build();
            }

            [Fact]
            public void NoLoad_NoMessageSent()
            {
                var response = _svc.SendCarrierRemovedEmail(null, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void NoUser_NoMessageSent()
            {
                var response = _svc.SendCarrierRemovedEmail(LOAD, null, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void NoLoadContacts_NoMessageSent()
            {
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, null, USER_CONTEXT_EMAIL, REASON);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void MessageShouldBeSent()
            {
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Should().NotBeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Once);
            }

            [Fact]
            public void SubjectHasRefLoadDisplay()
            {
                var expected = $"Booking Cancelled – load # {LOAD.ReferenceLoadDisplay}";
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Subject.Should().Contain(expected);
            }

            [Fact]
            public void SubjectHasOriginToDestDescription()
            {
                var origin = LOAD.LoadStops.Where(x => x.StopNbr == 1).FirstOrDefault();
                var dest = LOAD.LoadStops.Where(x => x.StopNbr == 2).FirstOrDefault();
                var expected = $"{origin.City}, {origin.State} to {dest.City}, {dest.State} - {LOAD.Equipment.EquipmentDesc}";

                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Subject.Should().Contain(expected);
            }

            [Fact]
            public void MessageSentToUser()
            {
                var userEmail = USER.UserNotifications.FirstOrDefault(x => x.MessageTypeId == MessageTypeConstants.Email).NotificationValue;
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.To.Should().Be(userEmail);
            }

            [Fact]
            public void MessageSentToUserContextEmailIfUserHasNoEmail()
            {
                USER.UserNotifications.RemoveAll(x => x.MessageTypeId == MessageTypeConstants.Email);
                InitDb();
                InitService();

                var expected = USER_CONTEXT_EMAIL;
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.To.Should().Be(expected);
            }

            [Fact]
            public void MessageCopiedToLoadContacts()
            {
                var expected = string.Join(";", LOAD_CONTACTS.Select(x => x.Email));
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.CC.Should().Be(expected);
            }

            [Fact]
            public void BodyContainsUserFirstName()
            {
                var expected = USER.FirstName;
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsReason()
            {
                var expected = REASON;
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsLoadshopUrl()
            {
                var expected = LOADSHOP_URL;
                var response = _svc.SendCarrierRemovedEmail(LOAD, USER, LOAD_CONTACTS, USER_CONTEXT_EMAIL, REASON);
                response.Message.Should().Contain(expected);
            }

            private void InitSeedData()
            {
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "email@test.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadDisplay = "RefLoadDisplay",
                    LineHaulRate = 10M,
                    FuelRate = 5M,
                    CustomerId = CUSTOMER_ID,
                    LoadStops = new List<LoadStopEntity>
                    {
                        new LoadStopEntity
                        {
                            StopNbr = 1,
                            City = "Origin City",
                            State = "WI",
                            Country = "USA",
                            ApptType = "Origin Appt Type",
                            EarlyDtTm = null,
                            LateDtTm = NOW.AddDays(1)
                        },
                        new LoadStopEntity
                        {
                            StopNbr = 2,
                            City = "Dest City",
                            State = "WI",
                            Country = "USA",
                            ApptType = "Dest Appt Type",
                            EarlyDtTm = null,
                            LateDtTm = NOW.AddDays(2)
                        }
                    },
                    Equipment = new EquipmentEntity
                    {
                        EquipmentDesc = "Equipment Description"
                    }
                };
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    FirstName = "John",
                    LastName = "Doe",
                    IdentUserId = USER_ID,
                    UserNotifications = USER_NOTIFICATIONS,
                    PrimaryScac = SCAC
                };
                LOAD_CONTACTS = new List<LoadContactEntity>
                {
                    new LoadContactEntity
                    {
                        Phone = "123-123-1234",
                        Email = "email1@shipper.com",
                        Display = $"{USER.FirstName} {USER.LastName}"
                    },
                    new LoadContactEntity
                    {
                        Phone = "123-123-1234",
                        Email = "email2@shipper.com",
                        Display = $"{USER.FirstName} {USER.LastName}"
                    }
                };
            }
        }

        public class SendFuelUpdateEmailTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private Mock<IConfigurationRoot> _config;
            private Mock<ISMSService> _smsService;
            private Mock<IEmailService> _emailService;
            private Mock<IRatingService> _ratingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;

            private NotificationService _svc;

            private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid CUSTOMER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static readonly string LOADSHOP_EMAIL = "loadshop@email.com";
            private static int CC_TO_LOADSHOP_THRESHOLD = 2;
            private decimal OLD_FUEL_RATE = 9.99M;
            private static readonly string SCAC = "SCAC";
            private static readonly string CARRIER_ID = "CarrierId";
            private static readonly string CARRIER_NAME = "CarrierName";
            private readonly DateTime NOW = new DateTime(2020, 3, 3, 12, 23, 0);

            private List<UserNotificationEntity> USER_NOTIFICATIONS;
            private List<LoadViewEntity> BOOKED_LOADS;
            private LoadEntity LOAD;
            private UserEntity USER;
            private LoadContactEntity LOAD_CONTACT;
            private List<ServiceTypeEntity> SERVICE_TYPES;
            private List<LoadServiceTypeEntity> LOAD_SERVICE_TYPES;
            private List<CarrierScacEntity> CARRIER_SCACS;
            private List<CarrierEntity> CARRIERS;
            private CustomerEntity SHIPPER;
            private LoadClaimEntity LOAD_CLAIM;

            public SendFuelUpdateEmailTests(TestFixture fixture)
            {
                InitSeedData();

                _mapper = fixture.Mapper;
                var options = new ConfigManagerOptions()
                {
                    ProcessNames = new List<string>() { "Loadshop.Web.API" },
                };
                _config = new Mock<IConfigurationRoot>();
                _config.SetupGet(x => x["LoadBoardNumberOfBookedLoadsToCCLoadshopEmail"]).Returns(CC_TO_LOADSHOP_THRESHOLD.ToString());
                _config.SetupGet(x => x["LoadshopEmail"]).Returns(LOADSHOP_EMAIL);

                _smsService = new Mock<ISMSService>();
                _emailService = new Mock<IEmailService>();
                _ratingService = new Mock<IRatingService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();
                _loadQueryRepository.Setup(ls => ls.GetNumberOfBookedLoadsForCarrierByUserIdentId(It.IsAny<Guid>())).Returns(5);
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                InitDb();
                InitService();
            }

            private void InitService()
            {
                _svc = new NotificationService(_db.Object, _mapper, _config.Object, _smsService.Object, _emailService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities, _loadQueryRepository.Object);
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .WithUserNotifications(USER_NOTIFICATIONS)
                    .WithBookedLoads(BOOKED_LOADS)
                    .WithServiceTypes(SERVICE_TYPES)
                    .WithLoadServiceTypes(LOAD_SERVICE_TYPES)
                    .WithCarrierScacs(CARRIER_SCACS)
                    .WithCarriers(CARRIERS)
                    .WithCustomer(SHIPPER)
                    .Build();
            }

            [Fact]
            public void NoLoad_NoMessageSent()
            {
                var response = _svc.SendFuelUpdateEmail(null, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void NoUser_NoMessageSent()
            {
                var response = _svc.SendFuelUpdateEmail(LOAD, null, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void NoLoadContact_NoMessageSent()
            {
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, null, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void MessageShouldBeSent()
            {
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, null);
                response.Should().NotBeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Once);
            }

            [Fact]
            public void SubjectIsLoadshopFuelUpdate()
            {
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Subject.Should().Be("Loadshop Fuel Update");
            }

            [Fact]
            public void MessageSentToUser()
            {
                var userEmail = USER.UserNotifications.FirstOrDefault(x => x.MessageTypeId == MessageTypeConstants.Email).NotificationValue;
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, null);
                response.To.Should().Be(userEmail);
            }

            [Fact]
            public void MessageCopiedToLoadContact()
            {
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.CC.Should().Be(LOAD_CONTACT.Email);
            }

            [Fact]
            public void MessageCopiedToLoadshopWhenFewerThanConfiguredLoadsBookedByUser()
            {
                _config.SetupGet(x => x["LoadBoardNumberOfBookedLoadsToCCLoadshopEmail"]).Returns("10");
                InitService();

                var expected = $"{LOAD_CONTACT.Email};{LOADSHOP_EMAIL}";
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, null);
                response.CC.Should().Be(expected);
            }

            [Fact]
            public void BodyContainsFuelEstimateDisclaimer()
            {
                var expected = "fuel cost listed above is estimated";
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsLoadContactFirstName()
            {
                var expected = USER.FirstName;
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsUserCellPhone()
            {
                var expected = USER.UserNotifications.FirstOrDefault(x => x.MessageTypeId == MessageTypeConstants.CellPhone).NotificationValue;
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsShipperName()
            {
                var expected = SHIPPER.Name;
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsServiceTypes()
            {
                var expected = string.Join(", ", SERVICE_TYPES.Select(x => x.Name).ToList());
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            [Fact]
            public void BodyContainsOperatingAuthorityNumber()
            {
                var expected = CARRIERS.First().OperatingAuthNbr;
                var response = _svc.SendFuelUpdateEmail(LOAD, USER, LOAD_CONTACT, OLD_FUEL_RATE, LOAD_CLAIM);
                response.Message.Should().Contain(expected);
            }

            private void InitSeedData()
            {
                USER_NOTIFICATIONS = new List<UserNotificationEntity>
                {
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.Email,
                        NotificationEnabled = true,
                        NotificationValue = "email@test.com"
                    },
                    new UserNotificationEntity
                    {
                        UserId = USER_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        NotificationEnabled = true,
                        NotificationValue = "123-123-1234"
                    }
                };
                BOOKED_LOADS = new List<LoadViewEntity>
                {
                    new LoadViewEntity(),
                    new LoadViewEntity(),
                    new LoadViewEntity()
                };
                LOAD = new LoadEntity
                {
                    LoadId = LOAD_ID,
                    ReferenceLoadDisplay = "RefLoadDisplay",
                    LineHaulRate = 10M,
                    FuelRate = 5M,
                    CustomerId = CUSTOMER_ID,
                    LoadStops = new List<LoadStopEntity>
                    {
                        new LoadStopEntity
                        {
                            StopNbr = 1,
                            City = "Origin City",
                            State = "WI",
                            Country = "USA",
                            ApptType = "Origin Appt Type",
                            EarlyDtTm = null,
                            LateDtTm = NOW.AddDays(1)
                        },
                        new LoadStopEntity
                        {
                            StopNbr = 2,
                            City = "Dest City",
                            State = "WI",
                            Country = "USA",
                            ApptType = "Dest Appt Type",
                            EarlyDtTm = null,
                            LateDtTm = NOW.AddDays(2)
                        }
                    },
                    Equipment = new EquipmentEntity
                    {
                        EquipmentDesc = "Equipment Description"
                    }
                };
                USER = new UserEntity
                {
                    UserId = USER_ID,
                    FirstName = "John",
                    LastName = "Doe",
                    IdentUserId = USER_ID,
                    UserNotifications = USER_NOTIFICATIONS,
                    PrimaryScac = SCAC
                };
                LOAD_CONTACT = new LoadContactEntity
                {
                    Phone = "123-123-1234",
                    Email = "email@address.com",
                    Display = $"{USER.FirstName} {USER.LastName}"
                };
                SERVICE_TYPES = new List<ServiceTypeEntity>
                {
                    new ServiceTypeEntity
                    {
                        ServiceTypeId = 1,
                        Name = "Service 1",
                    },
                    new ServiceTypeEntity
                    {
                        ServiceTypeId = 2,
                        Name = "Service 2",
                    }
                };
                LOAD_SERVICE_TYPES = new List<LoadServiceTypeEntity>
                {
                    new LoadServiceTypeEntity
                    {
                        LoadId = LOAD_ID,
                        ServiceTypeId = 1
                    },
                    new LoadServiceTypeEntity
                    {
                        LoadId = LOAD_ID,
                        ServiceTypeId = 2
                    }
                };
                CARRIER_SCACS = new List<CarrierScacEntity>
                {
                    new CarrierScacEntity
                    {
                        Scac = SCAC,
                        CarrierId = CARRIER_ID,
                        Carrier = new CarrierEntity
                        {
                            CarrierId = CARRIER_ID,
                            CarrierName = CARRIER_NAME,
                            OperatingAuthNbr = "Operating Auth Nbr"
                        }
                    }
                };
                CARRIERS = new List<CarrierEntity>
                {
                    CARRIER_SCACS.First().Carrier
                };
                SHIPPER = new CustomerEntity
                {
                    CustomerId = CUSTOMER_ID,
                    Name = "ShipperName"
                };
                LOAD_CLAIM = new LoadClaimEntity
                {
                    FlatFee = 1m,
                    PercentFee = 0.0125m,
                    FeeAdd = false,
                    LoadshopFee = 1.5m
                };
            }
        }

        public class RemoveOwnerIdTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db = new Mock<LoadshopDataContext>();
            private readonly IMapper _mapper;
            private Mock<IConfigurationRoot> _config = new Mock<IConfigurationRoot>();
            private Mock<ISMSService> _smsService = new Mock<ISMSService>();
            private Mock<IEmailService> _emailService = new Mock<IEmailService>();
            private Mock<IRatingService> _ratingService = new Mock<IRatingService>();
            private Mock<ILoadQueryRepository> _loadQueryRepository = new Mock<ILoadQueryRepository>();
            private Mock<IDateTimeProvider> _dateTime = new Mock<IDateTimeProvider>();
            private ServiceUtilities _serviceUtilities;

            private NotificationService _svc;

            public RemoveOwnerIdTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                InitService();
            }

            private void InitService()
            {
                _svc = new NotificationService(_db.Object, _mapper, _config.Object, _smsService.Object, _emailService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities, _loadQueryRepository.Object);
            }

            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [Theory]
            public void RemoveOwnerId_NullOrEmpty(string referenceLoadId)
            {
                _svc.RemoveOwnerId(referenceLoadId).Should().Be(string.Empty);
            }

            [Fact]
            public void RemoveOwnerId_NoOwner()
            {
                _svc.RemoveOwnerId("12345").Should().Be("12345");
            }

            [Fact]
            public void RemoveOwnerId()
            {
                _svc.RemoveOwnerId("OWNER-12345").Should().Be("12345");
            }
        }

        public class GetTitleCaseTests
        {
            public GetTitleCaseTests() { }

            [Fact]
            public void NoNeedToTest()
            {
                true.Should().BeTrue();
            }
        }

        public class GetLowerCaseTests
        {
            public GetLowerCaseTests() { }

            [Fact]
            public void NoNeedToTest()
            {
                true.Should().BeTrue();
            }
        }

        public class GetShipperNameTests
        {
            public GetShipperNameTests() { }

            [Fact]
            public void NoNeedToTest()
            {
                true.Should().BeTrue();
            }
        }

        public class GetCarrierNameTests
        {
            public GetCarrierNameTests() { }

            [Fact]
            public void NoNeedToTest()
            {
                true.Should().BeTrue();
            }
        }

        public class SendShipperFeeChangemailTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private Mock<IConfigurationRoot> _config;
            private Mock<IConfigurationSection> _configSection;
            private Mock<ISMSService> _smsService;
            private Mock<IEmailService> _emailService;
            private Mock<IRatingService> _ratingService;
            private Mock<ILoadQueryRepository> _loadQueryRepository;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;

            private NotificationService _svc;

            private static string CUSTOMER_NAME = "Test Customer";
            private static string UPDATE_USER = "Updating User";
            private static readonly string USER_CONTEXT_EMAIL = "user@email.com";
            private readonly DateTime NOW = new DateTime(2020, 3, 3, 12, 23, 0);


            private List<UserNotificationEntity> USER_NOTIFICATIONS;
            private LoadEntity LOAD;
            private UserEntity USER;
            private List<LoadContactEntity> LOAD_CONTACTS;

            public SendShipperFeeChangemailTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                var options = new ConfigManagerOptions()
                {
                    ProcessNames = new List<string>() { "Loadshop.Web.API", "Loadshop.Shared" },
                };
                _config = new Mock<IConfigurationRoot>();
                _configSection = new Mock<IConfigurationSection>();
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == "AccountsReceivableEmail"))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns(USER_CONTEXT_EMAIL);

                _smsService = new Mock<ISMSService>();
                _emailService = new Mock<IEmailService>();
                _ratingService = new Mock<IRatingService>();
                _loadQueryRepository = new Mock<ILoadQueryRepository>();
                _dateTime = new Mock<IDateTimeProvider>();
                _dateTime.SetupGet(x => x.Now).Returns(NOW);
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

                InitDb();
                InitService();
            }

            private void InitService()
            {
                _svc = new NotificationService(_db.Object, _mapper, _config.Object, _smsService.Object, _emailService.Object, _ratingService.Object, _dateTime.Object, _serviceUtilities, _loadQueryRepository.Object);
            }

            private void InitDb()
            {
                _db = new MockDbBuilder()
                    .Build();
            }

            [Fact]
            public void NoUpdatedCustomer_NoMessageSent()
            {
                var response = _svc.SendShipperFeeChangeEmail(new CustomerEntity(), null, UPDATE_USER);
                response.Should().BeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Never);
            }

            [Fact]
            public void SendsToAccountsReceivableEmail()
            {
                var response = _svc.SendShipperFeeChangeEmail(new CustomerEntity(), new CustomerProfileData(), UPDATE_USER);
                response.To.Should().Be(USER_CONTEXT_EMAIL);
            }

            [Fact]
            public void MessageShouldBeSent()
            {
                var response = _svc.SendShipperFeeChangeEmail(new CustomerEntity(), new CustomerProfileData(), UPDATE_USER);
                response.Should().NotBeNull();
                _emailService.Verify(x => x.SendMailMessage(It.IsAny<NotificationMessageEntity>(), It.IsAny<bool>()), Times.Once);
            }

            [Fact]
            public void SubjectIsExpected()
            {
                var response = _svc.SendShipperFeeChangeEmail(new CustomerEntity(), new CustomerProfileData(), UPDATE_USER);
                response.Subject.Should().Be("LoadShop Customer Fee Change");
            }

            [Fact]
            public void FeeForNewShipperMessage()
            {
                var customer = new CustomerProfileData
                {
                    Name = CUSTOMER_NAME,
                    InNetworkFlatFee = 10m,
                    InNetworkPercentFee = .125m,
                    InNetworkFeeAdd = true,
                    OutNetworkFlatFee = 15m,
                    OutNetworkPercentFee = .175m,
                    OutNetworkFeeAdd = false,
                };
                var response = _svc.SendShipperFeeChangeEmail(null, customer, UPDATE_USER);
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">In Network Flat Fee:</td><td>$10.00</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">In Network Percent Fee:</td><td>12.50%</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">In Network Fee Add:</td><td>Yes</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">Out Network Flat Fee:</td><td>$15.00</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">Out Network Percent Fee:</td><td>17.50%</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">Out Network Fee Add:</td><td>No</td></tr>");
            }

            [Fact]
            public void FeeForUpdatedShipperMessage()
            {
                var customer = new CustomerProfileData
                {
                    Name = CUSTOMER_NAME,
                    InNetworkFlatFee = 10m,
                    InNetworkPercentFee = .125m,
                    InNetworkFeeAdd = true,
                    OutNetworkFlatFee = 15m,
                    OutNetworkPercentFee = .175m,
                    OutNetworkFeeAdd = false,
                };
                var customerEntity = new CustomerEntity
                {
                    Name = CUSTOMER_NAME,
                    InNetworkFlatFee = 11m,
                    InNetworkPercentFee = .126m,
                    InNetworkFeeAdd = false,
                    OutNetworkFlatFee = 16m,
                    OutNetworkPercentFee = .176m,
                    OutNetworkFeeAdd = true,
                };
                var response = _svc.SendShipperFeeChangeEmail(customerEntity, customer, UPDATE_USER);
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">In Network Flat Fee:</td><td>$11.00</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">In Network Percent Fee:</td><td>12.60%</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">In Network Fee Add:</td><td>No</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">Out Network Flat Fee:</td><td>$16.00</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">Out Network Percent Fee:</td><td>17.60%</td></tr>");
                response.Message.Should().Contain(@"<tr><td style=""width: 200px"">Out Network Fee Add:</td><td>Yes</td></tr>");
            }

            [Fact]
            public void MessageIncludesShipperName()
            {
                var customer = new CustomerProfileData
                {
                    Name = CUSTOMER_NAME,
                    InNetworkFlatFee = 10m,
                    InNetworkPercentFee = .125m,
                    InNetworkFeeAdd = true,
                    OutNetworkFlatFee = 15m,
                    OutNetworkPercentFee = .175m,
                    OutNetworkFeeAdd = false,
                };
                var response = _svc.SendShipperFeeChangeEmail(null, customer, UPDATE_USER);
                response.Message.Should().Contain(CUSTOMER_NAME);
            }

            [Fact]
            public void MessageIncludesChangingUser()
            {
                var customer = new CustomerProfileData
                {
                    Name = CUSTOMER_NAME,
                    InNetworkFlatFee = 10m,
                    InNetworkPercentFee = .125m,
                    InNetworkFeeAdd = true,
                    OutNetworkFlatFee = 15m,
                    OutNetworkPercentFee = .175m,
                    OutNetworkFeeAdd = false,
                };
                var response = _svc.SendShipperFeeChangeEmail(null, customer, UPDATE_USER);
                response.Message.Should().Contain(UPDATE_USER);
            }
        }
    }
}
