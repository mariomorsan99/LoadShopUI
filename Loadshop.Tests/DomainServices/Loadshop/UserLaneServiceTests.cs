using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Common.Services.Data;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class UserLaneServiceTests
    {
        public class GetSavedLanesAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly Mock<ICommonService> _common;
            private readonly Mock<ISecurityService> _security;
            private readonly IMapper _mapper;

            private IUserLaneService _svc;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");

            private List<UserLaneEntity> USER_LANES;
            private List<MessageTypeEntity> MESSAGE_TYPES;

            public GetSavedLanesAsyncTests(TestFixture fixture)
            {
                _common = new Mock<ICommonService>();
                _common.Setup(x => x.GetUSCANStateProvince(It.IsAny<string>())).Returns(new StateData { Abbreviation = string.Empty });

                _security = new Mock<ISecurityService>();
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(true);

                _mapper = fixture.Mapper;

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
                    .WithUserLanes(USER_LANES)
                    .WithMessageTypes(MESSAGE_TYPES)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserLaneService(_db.Object, _common.Object, _security.Object, _mapper);
            }

            [Fact]
            public async Task NoUserLanes_ReturnsEmptyList()
            {
                USER_LANES = new List<UserLaneEntity>();
                InitDb();
                InitService();

                var actual = await _svc.GetSavedLanesAsync(USER_ID);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task InvalidUserId_ReturnsEmptyList()
            {
                var actual = await _svc.GetSavedLanesAsync(Guid.Empty);
                actual.Should().BeEmpty();
            }

            [Fact]
            public void NoAccessToUserProfileFavoritesView_ThrowsException()
            {
                var expected = new UnauthorizedAccessException("Nah bruh, GTFOH");
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Throws(expected);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ThrowsAsync(expected);
                InitService();

                Func<Task> action = async () => await _svc.GetSavedLanesAsync(USER_ID);
                action.Should().Throw<UnauthorizedAccessException>(expected.Message);
            }

            [Fact]
            public async Task ReturnsUserLaneData()
            {
                var actual = await _svc.GetSavedLanesAsync(USER_ID);
                actual.Should().NotBeEmpty();
            }

            [Fact]
            public async Task UserLaneMessageTypesExist_ReturnsUserLaneDataWithMessageTypes()
            {
                var actual = await _svc.GetSavedLanesAsync(USER_ID);
                actual.Should().NotBeEmpty();
                actual.ForEach(x =>
                {
                    x.UserLaneMessageTypes.Should().NotBeEmpty();
                });
            }

            [Fact]
            public async Task NoUserLaneMessageTypes_AddsUserLaneMessageTypes()
            {
                InitSeedData();
                USER_LANES.ForEach(x =>
                {
                    x.UserLaneMessageTypes = new List<UserLaneMessageTypeEntity>();
                });
                InitDb();
                InitService();

                var actual = await _svc.GetSavedLanesAsync(USER_ID);
                actual.Should().NotBeEmpty();
                actual.ForEach(x =>
                {
                    x.UserLaneMessageTypes.Should().NotBeEmpty();
                });
            }

            private void InitSeedData()
            {
                USER_LANES = new List<UserLaneEntity>
                {
                    new UserLaneEntity
                    {
                        UserLaneMessageTypes = new List<UserLaneMessageTypeEntity>
                        {
                            new UserLaneMessageTypeEntity
                            {
                                MessageTypeId = MessageTypeConstants.Email
                            },
                            new UserLaneMessageTypeEntity
                            {
                                MessageTypeId = MessageTypeConstants.CellPhone
                            }
                        },
                        UserLaneEquipments = new List<UserLaneEquipmentEntity>
                        {
                            new UserLaneEquipmentEntity
                            {

                            }
                        },
                        User = new UserEntity
                        {
                            IdentUserId = USER_ID
                        }
                    }
                };
                MESSAGE_TYPES = new List<MessageTypeEntity>
                {
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.Email
                    },
                    new MessageTypeEntity
                    {
                        MessageTypeId = MessageTypeConstants.CellPhone
                    }
                };
            }
        }

        public class CreateLaneAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly Mock<ICommonService> _common;
            private readonly Mock<ISecurityService> _security;
            private readonly IMapper _mapper;

            private IUserLaneService _svc;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid USER_LANE_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string USER_NAME = "username";

            private List<UserLaneEntity> USER_LANES;
            private List<UserEntity> USERS;

            private UserLaneData _laneData = new UserLaneData
            {
                UserLaneId = USER_LANE_ID.ToString(),
                EquipmentIds = new List<string>
                {
                    "Equipment 1",
                    "Equipment 2"
                },
                UserLaneMessageTypes = new List<UserLaneMessageTypeData>
                {
                    new UserLaneMessageTypeData
                    {
                        Description = "User Lane Message 1",
                        MessageTypeId = MessageTypeConstants.Email,
                        Selected = true
                    },
                    new UserLaneMessageTypeData
                    {
                        Description = "User Lane Message 2",
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        Selected = true
                    }
                },
                OrigLat = 1m,
                OrigLng = 1m,
                OrigCity = "Orig City",
                OrigState = "Orig State",
                OrigCountry = "Orig Country",
                OrigDH = null,
                DestLat = 1m,
                DestLng = 1m,
                DestCity = "Dest City",
                DestState = "Dest State",
                DestCountry = "Dest Country",
                DestDH = null,
            };

            public CreateLaneAsyncTests(TestFixture fixture)
            {
                _common = new Mock<ICommonService>();
                _common.Setup(x => x.GetUSCANStateProvince(It.Is<string>(str => !string.IsNullOrWhiteSpace(str))))
                    .Returns(new StateData { Abbreviation = "ST" });
                _common.Setup(x => x.GetUSCANStateProvince(It.Is<string>(str => string.IsNullOrWhiteSpace(str))))
                    .Returns(new StateData { Abbreviation = "" });

                _security = new Mock<ISecurityService>();
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(true);

                _mapper = fixture.Mapper;

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
                    .WithUserLanes(USER_LANES)
                    .WithUsers(USERS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserLaneService(_db.Object, _common.Object, _security.Object, _mapper);
            }

            [Fact]
            public void NoUserAccess_ThrowsException()
            {
                var expected = new UnauthorizedAccessException("Nah bruh, GTFOH");
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Throws(expected);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ThrowsAsync(expected);
                InitService();

                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<UnauthorizedAccessException>(expected.Message);
            }

            [Fact]
            public void UserLaneExists_ThrowsException()
            {
                InitSeedData();
                USER_LANES.Add(new UserLaneEntity
                {
                    UserLaneId = USER_LANE_ID
                });
                InitDb();
                InitService();

                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("User Lane already exists");
            }

            [Fact]
            public void NullEquipmentIds_ThrowsException()
            {
                _laneData.EquipmentIds = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have at least on equipment type selected");
            }

            [Fact]
            public void EmptyEquipmentIds_ThrowsException()
            {
                _laneData.EquipmentIds = new List<string>();
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have at least on equipment type selected");
            }

            [Fact]
            public void InvalidUserId_ThrowsException()
            {
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, Guid.Empty, USER_NAME);
                action.Should().Throw<Exception>("Invalid userId");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingOriginCity_MissingDest_ThrowsException(string input)
            {
                _laneData.OrigCity = input;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingOriginState_MissingDest_ThrowsException(string input)
            {
                _laneData.OrigState = input;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingOriginCountry_MissingDest_ThrowsException(string input)
            {
                _laneData.OrigCountry = input;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingOriginLat_MissingDest_ThrowsException()
            {
                _laneData.OrigLat = null;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingOriginLng_MissingDest_ThrowsException()
            {
                _laneData.OrigLng = null;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public async Task MissingOriginLatAndLng_HasOrigCity_MissingDest_UserLaneAdded()
            {
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                builder.MockUserLanes.Verify(x => x.Add(It.IsAny<UserLaneEntity>()), Times.Once);
                _db.Verify(x => x.SaveChangesAsync(USER_NAME, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingDestCity_MissingOrig_ThrowsException(string input)
            {
                _laneData.DestCity = input;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingDestState_MissingOrig_ThrowsException(string input)
            {
                _laneData.DestState = input;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingDestCountry_MissingOrig_ThrowsException(string input)
            {
                _laneData.DestCountry = input;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingDestLat_MissingOrig_ThrowsException()
            {
                _laneData.DestLat = null;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingDestLng_MissingOrig_ThrowsException()
            {
                _laneData.DestLng = null;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public async Task MissingDestLatAndLng_HasDestCity_MissingOrig_UserLaneAdded()
            {
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                builder.MockUserLanes.Verify(x => x.Add(It.IsAny<UserLaneEntity>()), Times.Once);
                _db.Verify(x => x.SaveChangesAsync(USER_NAME, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task UserLaneAdded()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                builder.MockUserLanes.Verify(x => x.Add(It.IsAny<UserLaneEntity>()), Times.Once);
                _db.Verify(x => x.SaveChangesAsync(USER_NAME, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task UserLaneMessageTypesAdded()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                UserLaneEntity added = null;
                builder.MockUserLanes
                    .Setup(x => x.Add(It.IsAny<UserLaneEntity>()))
                    .Callback((UserLaneEntity x) => { added = x; });

                var actual = await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                added.UserLaneMessageTypes.Should().NotBeEmpty();
            }

            [Fact]
            public async Task UserLaneEquipmentsAdded()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                UserLaneEntity added = null;
                builder.MockUserLanes
                    .Setup(x => x.Add(It.IsAny<UserLaneEntity>()))
                    .Callback((UserLaneEntity x) => { added = x; });

                var actual = await _svc.CreateLaneAsync(_laneData, USER_ID, USER_NAME);
                added.UserLaneEquipments.Should().NotBeEmpty();
            }

            private void InitSeedData()
            {
                USER_LANES = new List<UserLaneEntity>();
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        IdentUserId = USER_ID
                    }
                };
            }
        }

        public class UpdateLaneAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly Mock<ICommonService> _common;
            private readonly Mock<ISecurityService> _security;
            private readonly IMapper _mapper;

            private IUserLaneService _svc;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid USER_LANE_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string USER_NAME = "username";

            private List<UserLaneEntity> USER_LANES;
            private List<UserLaneMessageTypeEntity> USER_LANE_MESSAGES_TYPES;
            private List<UserLaneEquipmentEntity> USER_LANE_EQUIPMENT;
            private List<UserEntity> USERS;

            private UserLaneData _laneData = new UserLaneData
            {
                UserLaneId = USER_LANE_ID.ToString(),
                EquipmentIds = new List<string>
                {
                    "Equipment 1",
                    "Equipment 2"
                },
                UserLaneMessageTypes = new List<UserLaneMessageTypeData>
                {
                    new UserLaneMessageTypeData
                    {
                        Description = "User Lane Message 1",
                        MessageTypeId = MessageTypeConstants.Email,
                        Selected = true
                    },
                    new UserLaneMessageTypeData
                    {
                        Description = "User Lane Message 2",
                        MessageTypeId = MessageTypeConstants.CellPhone,
                        Selected = true
                    }
                },
                OrigLat = 1m,
                OrigLng = 1m,
                OrigCity = "Orig City",
                OrigState = "Orig State",
                OrigCountry = "Orig Country",
                OrigDH = null,
                DestLat = 1m,
                DestLng = 1m,
                DestCity = "Dest City",
                DestState = "Dest State",
                DestCountry = "Dest Country",
                DestDH = null,
            };

            public UpdateLaneAsyncTests(TestFixture fixture)
            {
                _common = new Mock<ICommonService>();
                _common.Setup(x => x.GetUSCANStateProvince(It.Is<string>(str => !string.IsNullOrWhiteSpace(str))))
                    .Returns(new StateData { Abbreviation = "ST" });
                _common.Setup(x => x.GetUSCANStateProvince(It.Is<string>(str => string.IsNullOrWhiteSpace(str))))
                    .Returns(new StateData { Abbreviation = "" });

                _security = new Mock<ISecurityService>();
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(true);

                _mapper = fixture.Mapper;

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
                    .WithUserLanes(USER_LANES)
                    .WithUserLaneMessageTypes(USER_LANE_MESSAGES_TYPES)
                    .WithUserLaneEquipment(USER_LANE_EQUIPMENT)
                    .WithUsers(USERS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserLaneService(_db.Object, _common.Object, _security.Object, _mapper);
            }

            [Fact]
            public void NoUserAccess_ThrowsException()
            {
                var expected = new UnauthorizedAccessException("Nah bruh, GTFOH");
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Throws(expected);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ThrowsAsync(expected);
                InitService();

                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<UnauthorizedAccessException>(expected.Message);
            }

            [Fact]
            public void UserLaneNotFound_ThrowsException()
            {
                InitSeedData();
                USER_LANES = new List<UserLaneEntity>();
                InitDb();
                InitService();

                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("User Lane not found");
            }

            [Fact]
            public void NullEquipmentIds_ThrowsException()
            {
                _laneData.EquipmentIds = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have at least on equipment type selected");
            }

            [Fact]
            public void EmptyEquipmentIds_ThrowsException()
            {
                _laneData.EquipmentIds = new List<string>();
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have at least on equipment type selected");
            }

            [Fact]
            public void InvalidUserId_ThrowsException()
            {
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, Guid.Empty, USER_NAME);
                action.Should().Throw<Exception>("Invalid userId");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingOriginCity_MissingDest_ThrowsException(string input)
            {
                _laneData.OrigCity = input;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingOriginState_MissingDest_ThrowsException(string input)
            {
                _laneData.OrigState = input;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingOriginCountry_MissingDest_ThrowsException(string input)
            {
                _laneData.OrigCountry = input;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingOriginLat_MissingDest_ThrowsException()
            {
                _laneData.OrigLat = null;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingOriginLng_MissingDest_ThrowsException()
            {
                _laneData.OrigLng = null;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public async Task MissingOriginLatAndLng_HasOrigCity_MissingDest_UserLaneSaved()
            {
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.DestCity = null;
                _laneData.DestState = null;
                _laneData.DestCountry = null;
                _laneData.DestDH = null;

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                _db.Verify(x => x.SaveChangesAsync(USER_NAME, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingDestCity_MissingOrig_ThrowsException(string input)
            {
                _laneData.DestCity = input;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingDestState_MissingOrig_ThrowsException(string input)
            {
                _laneData.DestState = input;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void MissingDestCountry_MissingOrig_ThrowsException(string input)
            {
                _laneData.DestCountry = input;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingDestLat_MissingOrig_ThrowsException()
            {
                _laneData.DestLat = null;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public void MissingDestLng_MissingOrig_ThrowsException()
            {
                _laneData.DestLng = null;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;
                Func<Task> action = async () => await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                action.Should().Throw<Exception>("Must have an Origin or Destination");
            }

            [Fact]
            public async Task MissingDestLatAndLng_HasDestCity_MissingOrig_UserLaneSaved()
            {
                _laneData.DestLat = null;
                _laneData.DestLng = null;
                _laneData.OrigLat = null;
                _laneData.OrigLng = null;
                _laneData.OrigCity = null;
                _laneData.OrigState = null;
                _laneData.OrigCountry = null;
                _laneData.OrigDH = null;

                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                _db.Verify(x => x.SaveChangesAsync(USER_NAME, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task UserLaneSaved()
            {
                var actual = await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                _db.Verify(x => x.SaveChangesAsync(USER_NAME, It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task NewUserLaneMessageTypesAdded()
            {
                USER_LANE_MESSAGES_TYPES = new List<UserLaneMessageTypeEntity>();
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                builder.MockUserLaneMessageTypes.Verify(x => x.Add(It.IsAny<UserLaneMessageTypeEntity>()), Times.Exactly(2));
            }

            [Fact]
            public async Task NewUserLaneEquipmentAdded()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                builder.MockUserLaneEquipments.Verify(x => x.Add(It.IsAny<UserLaneEquipmentEntity>()), Times.Exactly(2));
            }

            [Fact]
            public async Task ExistingUserLaneMessageTypesDeselected_MessageTypesRemoved()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                _laneData.UserLaneMessageTypes.ForEach(x => x.Selected = false);

                var actual = await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                builder.MockUserLaneMessageTypes.Verify(x => x.Remove(It.IsAny<UserLaneMessageTypeEntity>()), Times.Exactly(2));
            }

            [Fact]
            public async Task ExistingUserLaneEquipmentNotOnUpdate_EquipmentRemoved()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = await _svc.UpdateLaneAsync(_laneData, USER_ID, USER_NAME);
                builder.MockUserLaneEquipments.Verify(x => x.Remove(It.IsAny<UserLaneEquipmentEntity>()), Times.Exactly(2));
            }

            private void InitSeedData()
            {
                USER_LANE_MESSAGES_TYPES = new List<UserLaneMessageTypeEntity>
                {
                    new UserLaneMessageTypeEntity
                    {
                        UserLaneId = USER_LANE_ID,
                        MessageTypeId = MessageTypeConstants.Email
                    },
                    new UserLaneMessageTypeEntity
                    {
                        UserLaneId = USER_LANE_ID,
                        MessageTypeId = MessageTypeConstants.CellPhone
                    }
                };
                USER_LANE_EQUIPMENT = new List<UserLaneEquipmentEntity>
                {
                    new UserLaneEquipmentEntity
                    {
                        UserLaneId = USER_LANE_ID,
                        EquipmentId = "Remove Me 1"
                    },
                    new UserLaneEquipmentEntity
                    {
                        UserLaneId = USER_LANE_ID,
                        EquipmentId = "Remove Me 2"
                    }
                };
                USER_LANES = new List<UserLaneEntity>
                {
                    new UserLaneEntity
                    {
                        UserLaneId = USER_LANE_ID,
                        UserLaneMessageTypes = USER_LANE_MESSAGES_TYPES,
                        UserLaneEquipments = USER_LANE_EQUIPMENT
                    }
                };
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        UserId = USER_ID,
                        IdentUserId = USER_ID
                    }
                };
            }
        }

        public class DeleteLaneAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly Mock<ICommonService> _common;
            private readonly Mock<ISecurityService> _security;
            private readonly IMapper _mapper;

            private IUserLaneService _svc;

            private static readonly Guid USER_LANE_ID = new Guid("11111111-1111-1111-1111-111111111111");

            private List<UserLaneEntity> USER_LANES;

            public DeleteLaneAsyncTests(TestFixture fixture)
            {
                _common = new Mock<ICommonService>();
                _common.Setup(x => x.GetUSCANStateProvince(It.IsAny<string>())).Returns(new StateData { Abbreviation = string.Empty });

                _security = new Mock<ISecurityService>();
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ReturnsAsync(true);

                _mapper = fixture.Mapper;

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
                    .WithUserLanes(USER_LANES)
                    .Build();
            }

            private void InitService()
            {
                _svc = new UserLaneService(_db.Object, _common.Object, _security.Object, _mapper);
            }

            [Fact]
            public void NoUserAccess_ThrowsException()
            {
                var expected = new UnauthorizedAccessException("Nah bruh, GTFOH");
                _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Throws(expected);
                _security.Setup(x => x.UserHasActionAsync(It.IsAny<string[]>())).ThrowsAsync(expected);
                InitService();

                Func<Task> action = async () => await _svc.DeleteLaneAsync(USER_LANE_ID);
                action.Should().Throw<UnauthorizedAccessException>(expected.Message);
            }

            [Fact]
            public void NoUserLanes_ThrowsException()
            {
                USER_LANES = new List<UserLaneEntity>();
                InitDb();
                InitService();

                Func<Task> action = async () => await _svc.DeleteLaneAsync(USER_LANE_ID);
                action.Should().Throw<Exception>("UserLane not found");
            }

            [Fact]
            public void InvalidUserLaneId_ThrowsException()
            {
                Func<Task> action = async () => await _svc.DeleteLaneAsync(Guid.Empty);
                action.Should().Throw<Exception>("UserLane not found");
            }

            [Fact]
            public async Task UserLaneRemoved()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.DeleteLaneAsync(USER_LANE_ID);
                builder.MockUserLanes.Verify(x => x.Remove(It.IsAny<UserLaneEntity>()), Times.Once);
                _db.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
            }

            private void InitSeedData()
            {
                USER_LANES = new List<UserLaneEntity>
                {
                    new UserLaneEntity
                    {
                        UserLaneId = USER_LANE_ID,
                    }
                };
            }
        }
    }
}
