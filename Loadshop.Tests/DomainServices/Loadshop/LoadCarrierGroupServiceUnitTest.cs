using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Common.Services.Data;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class LoadCarrierGroupServiceTest : IClassFixture<TestFixture>
    {
        private Mock<LoadshopDataContext> _db;
        private readonly TestFixture _testFixture;
        private readonly IMapper _mapper;
        private Mock<ICommonService> _common;
        private Mock<IUserContext> _userContext;
        private Mock<ISecurityService> _security;

        private LoadCarrierGroupService _svc;

        private static Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
        private static Guid CUSTOMER_ID_2 = new Guid("22222222-2222-2222-2222-222222222222");
        private static Guid ADMIN_IDENT_USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
        private static Guid ADMIN_USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
        private static Guid ADMIN_ROLE_ID = new Guid("11111111-1111-1111-1111-111111111111");

        private LoadCarrierGroupEntity GROUP_1;
        private LoadCarrierGroupEntity GROUP_2;
        private LoadCarrierGroupEntity GROUP_3;
        private List<LoadCarrierGroupCarrierEntity> GROUP_CARRIER_GROUPS;
        private UserEntity ADMIN_USER;

        public LoadCarrierGroupServiceTest(TestFixture testFixture)
        {
            this._testFixture = testFixture;
            _mapper = _testFixture.Mapper;

            InitSeedData();

            _db = new MockDbBuilder()
                .WithUser(ADMIN_USER)
                .WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { GROUP_1, GROUP_2, GROUP_3 })
                .WithLoadCarrierGroupCarriers(GROUP_CARRIER_GROUPS)
                .Build();
            _common = new Mock<ICommonService>();
            _userContext = new Mock<IUserContext>();
            _userContext.SetupGet(x => x.UserId).Returns(() => ADMIN_USER_ID);


            _security = new Mock<ISecurityService>();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID }
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();
        }

        private void InitService()
        {
            _svc = new LoadCarrierGroupService(_db.Object, _common.Object, _mapper, _userContext.Object, _security.Object);
        }

        [Fact]
        public void GetLoadCarrierGroup_ShouldReturnExpectedGroup()
        {
            var expected = _mapper.Map<LoadCarrierGroupDetailData>(GROUP_3);
            expected.CarrierCount = 2;

            var group = _svc.GetLoadCarrierGroup(300);
            group.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetLoadCarrierGroup_WhenNoGroups_ShouldThrowException()
        {
            _db = new MockDbBuilder().Build();
            InitService();

            Action action = () => _svc.GetLoadCarrierGroup(3);
            action.Should().Throw<Exception>().WithMessage("Load Carrier Group not found.");

        }

        [Fact]
        public void GetLoadCarrierGroup_WhenGroupDoesNotExist_ShouldThrowException()
        {
            Action action = () => _svc.GetLoadCarrierGroup(-1);
            action.Should().Throw<Exception>().WithMessage("Load Carrier Group not found.");
        }

        [Fact]
        public void GetLoadCarrierGroup_WhenDBThrowsException_ShouldReturnNullGroup()
        {
            var ex = new Exception("Testing");
            _db.Setup(x => x.LoadCarrierGroups).Throws(ex);

            var thrownEx = Assert.Throws<Exception>(() => _svc.GetLoadCarrierGroup(-1));
            thrownEx.Should().Be(ex);
        }

        [Fact]
        public void GetLoadCarrierGroups_ShouldReturnExpectedGroups()
        {
            var groups = _svc.GetLoadCarrierGroups();
            groups.Should().HaveCount(2);
            groups.Should().Contain(_ => _.LoadCarrierGroupId == 100);
            groups.Should().Contain(_ => _.LoadCarrierGroupId == 300);
        }

        [Fact]
        public void GetLoadCarrierGroups_WhenNoGroups_ShouldReturnEmptyList()
        {
            _db = new MockDbBuilder().Build();
            InitService();

            var groups = _svc.GetLoadCarrierGroups();
            groups.Should().BeEmpty();
        }

        [Fact]
        public void GetLoadCarrierGroups_WhenDBThrowsException_ShouldThrowException()
        {
            var ex = new Exception("Testing");
            _db.Setup(x => x.LoadCarrierGroups).Throws(ex);

            var thrownEx = Assert.Throws<Exception>(() => _svc.GetLoadCarrierGroup(-1));
            thrownEx.Should().Be(ex);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenNoLoadCarrigerGroupId_ShouldReturnModelStateError()
        {
            var updateGroup = new LoadCarrierGroupDetailData();

            var expectedErrorMessage = "LoadCarrierGroup should have an LoadCarrierGroupId assigned when updating.";
            var response = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenNoCustomerId_ShouldReturnModelStateError()
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 4,
                OriginCity = "Test",
                GroupName = "test"
            };

            var expectedErrorMessage = $"Must have a Customer{Environment.NewLine}";
            var response = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenNoOriginDestinationEquipmentAndReferenceLoadPrefix_ShouldReturnModelStateError()
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 4,
                GroupName = "test",
                CustomerId = new Guid("EB7BF2DC-FACF-44C8-8F7C-89A106A1C439")
            };

            var expectedErrorMessage = $"Must have an Origin, Destination, or Equipment{Environment.NewLine}";
            var response = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenOnlyIdSet_ShouldReturnModelStateError()
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 400,
                GroupName = "test"
            };

            var expectedErrorMessage = $"Must have a Customer{Environment.NewLine}Must have an Origin, Destination, or Equipment{Environment.NewLine}";
            var response = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Theory]
        [InlineData("City", "", "")]
        [InlineData("", "State", "")]
        [InlineData("", "", "Country")]
        public void UpdateLoadCarrierGroup_WhenAtLeastOnePieceOfOriginSet_ShouldNotThrowException(string city, string state, string country)
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 100,
                CustomerId = CUSTOMER_ID_2,
                OriginCity = city,
                OriginState = state,
                OriginCountry = country
            };

            Action action = () => _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            action.Should().NotThrow();
        }

        [Theory]
        [InlineData("City", "", "")]
        [InlineData("", "State", "")]
        [InlineData("", "", "Country")]
        public void UpdateLoadCarrierGroup_WhenAtLeastOnePieceOfDestinationSet_ShouldNotThrowException(string city, string state, string country)
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 100,
                CustomerId = CUSTOMER_ID_2,
                DestinationCity = city,
                DestinationState = state,
                DestinationCountry = country
            };

            Action action = () => _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            action.Should().NotThrow();
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenAtLeastEquipmentIsSet_ShouldNotThrowException()
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 100,
                CustomerId = CUSTOMER_ID,
                LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentData>{
                    new LoadCarrierGroupEquipmentData
                    {
                        EquipmentId = "Equipment Id",
                        LoadCarrierGroupId = 300,
                    }
                }
            };

            Action action = () => _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            action.Should().NotThrow();
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenUpdateSucceeds_ShouldReturnUpdatedGroup()
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 300,
                GroupName = "Group 3 Update",
                GroupDescription = "Group Description 3  Update",
                CustomerId = CUSTOMER_ID,
                OriginAddress1 = "Origin Address 3 Update",
                OriginCity = "Origin City 3 Update",
                OriginState = "Origin State 3 Update",
                OriginPostalCode = "OZipUpdate",
                OriginCountry = "Origin Country 3 Update",
                DestinationAddress1 = "Destination Address 3 Update",
                DestinationCity = "Destination City 3 Update",
                DestinationState = "Destination State 3 Update",
                DestinationPostalCode = "DZipUpdate",
                DestinationCountry = "Destination Country 3 Update",
                LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentData>{
                    new LoadCarrierGroupEquipmentData
                    {
                        EquipmentId = "Equipment Update 1",
                        LoadCarrierGroupId = 300
                    }
                },
                CarrierCount = 1,
                Carriers = new List<LoadCarrierGroupCarrierData>()
                {
                    new LoadCarrierGroupCarrierData()
                    {
                        CarrierId = "test",
                        LoadCarrierGroupCarrierId = 1,
                        LoadCarrierGroupId = 300
                    }
                }
            };

            var group = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");

            updateGroup.LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentData>
                                                    {
                                                        new LoadCarrierGroupEquipmentData
                                                        {
                                                            EquipmentId = "EQ 1",
                                                            LoadCarrierGroupId = 300
                                                        }
                                                    };

            group.LoadCarrierGroupData.Should().BeEquivalentTo(updateGroup);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenUpdateSucceeds_ShouldCallSaveChanges()
        {
            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
            InitService();

            var updateGroup = _mapper.Map<LoadCarrierGroupDetailData>(GROUP_3);
            updateGroup.LoadCarrierGroupId = 200;
            updateGroup.OriginCity = "New Origin";
            updateGroup.Carriers.ForEach(x => x.LoadCarrierGroupId = 200);

            _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            _db.Verify(x => x.SaveChanges("testuser"), Times.Once);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenDatabaseThrowsDuplicate_ShouldReturnModelStateError()
        {
            var updateGroup = _mapper.Map<LoadCarrierGroupDetailData>(GROUP_3);
            updateGroup.LoadCarrierGroupId = 2;
            updateGroup.Carriers.ForEach(x => x.LoadCarrierGroupId = 2);

            var ex = new DbUpdateException("Testing", new Exception("Violation of UNIQUE KEY constraint"));
            _db.Setup(x => x.LoadCarrierGroups).Throws(ex);

            var expectedErrorMessage = "A carrier group already exists for:" + Environment.NewLine
                + $"Origin Address1 - {updateGroup.OriginAddress1}{Environment.NewLine}"
                + $"Origin City - {updateGroup.OriginCity}{Environment.NewLine}"
                + $"Origin State - {updateGroup.OriginState}{Environment.NewLine}"
                + $"Origin Postal Code - {updateGroup.OriginPostalCode}{Environment.NewLine}"
                + $"Origin Country - {updateGroup.OriginCountry}{Environment.NewLine}"
                + $"Destination Address1 - {updateGroup.DestinationAddress1}{Environment.NewLine}"
                + $"Destination City - {updateGroup.DestinationCity}{Environment.NewLine}"
                + $"Destination State - {updateGroup.DestinationState}{Environment.NewLine}"
                + $"Destination Postal Code - {updateGroup.DestinationPostalCode}{Environment.NewLine}"
                + $"Destination Country - {updateGroup.DestinationCountry}{Environment.NewLine}"
                + $"Equipment Type(s) - {string.Join(", ", updateGroup.LoadCarrierGroupEquipment.Select(x => x.EquipmentId))}{Environment.NewLine}";

            var response = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenUpdatedGroupDoesntExist_ShouldReturnModelStateError()
        {
            var updateGroup = _mapper.Map<LoadCarrierGroupDetailData>(GROUP_3);
            updateGroup.LoadCarrierGroupId = 99;
            updateGroup.Carriers.ForEach(x => x.LoadCarrierGroupId = 99);

            var expectedErrorMessage = "LoadCarrierGroup not found";
            var response = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void UpdateLoadCarrierGroup_WhenStateNameHasAbbreviation_ShouldUpdateToAbbreviation()
        {
            var updateGroup = new LoadCarrierGroupDetailData()
            {
                LoadCarrierGroupId = 300,
                GroupName = "Group 3",
                GroupDescription = "Group Description 3 ",
                CustomerId = CUSTOMER_ID,
                OriginState = "Origin State 3",
                DestinationState = "Destination State 3"
            };
            _common.Reset();
            _common.Setup(_ => _.GetUSCANStateProvince("Origin State 3")).Returns(new StateData { Abbreviation = "OS3" });
            _common.Setup(_ => _.GetUSCANStateProvince("Destination State 3")).Returns(new StateData { Abbreviation = "DS3" });
            InitService();

            var group = _svc.UpdateLoadCarrierGroup(updateGroup, "testuser");
            group.LoadCarrierGroupData.OriginState.Should().Be("OS3");
            group.LoadCarrierGroupData.DestinationState.Should().Be("DS3");
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenGroupHasLoadCarrigerGroupId_ShouldReturnModelStateError()
        {
            var createGroup = new LoadCarrierGroupDetailData();
            createGroup.LoadCarrierGroupId = 99;

            var expectedErrorMessage = $"LoadCarrierGroup should not have an LoadCarrierGroupId assigned when creating.";
            var response = _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenNoCustomerId_ShouldReturnModelStateError()
        {
            var createGroup = new LoadCarrierGroupDetailData()
            {
                OriginCity = "Test",
                GroupName = "test"
            };

            var expectedErrorMessage = $"Must have a Customer{Environment.NewLine}";
            var response = _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenNoOriginDestinationEquipment_ShouldReturnModelStateError()
        {
            var createGroup = new LoadCarrierGroupDetailData()
            {
                CustomerId = CUSTOMER_ID_2,
                GroupName = "test"
            };

            var expectedErrorMessage = $"Must have an Origin, Destination, or Equipment{Environment.NewLine}";
            var response = _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenEmpty_ShouldReturnModelStateError()
        {
            var createGroup = new LoadCarrierGroupDetailData();

            var expectedErrorMessage = $"Must have a Customer{Environment.NewLine}Must have a name{Environment.NewLine}Must have an Origin, Destination, or Equipment{Environment.NewLine}";
            var response = _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Theory]
        [InlineData("Address", "", "", "", "")]
        [InlineData("", "City", "", "", "")]
        [InlineData("", "", "State", "", "")]
        [InlineData("", "", "", "PostalCode", "")]
        [InlineData("", "", "", "", "Country")]
        public void CreateLoadCarrierGroup_WhenAtLeastOnePieceOfOriginSet_ShouldNotThrowException(string address, string city, string state, string postalCode, string country)
        {
            var newEntity = new LoadCarrierGroupEntity { LoadCarrierGroupId = 0, LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>() };
            _db = new MockDbBuilder().WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { newEntity }).Build();
            InitService();

            var createGroup = new LoadCarrierGroupDetailData()
            {
                CustomerId = CUSTOMER_ID_2,
                OriginAddress1 = address,
                OriginCity = city,
                OriginState = state,
                OriginPostalCode = postalCode,
                OriginCountry = country
            };

            Action action = () => _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            action.Should().NotThrow();
        }

        [Theory]
        [InlineData("Address", "", "", "", "")]
        [InlineData("", "City", "", "", "")]
        [InlineData("", "", "State", "", "")]
        [InlineData("", "", "", "PostalCode", "")]
        [InlineData("", "", "", "", "Country")]
        public void CreateLoadCarrierGroup_WhenAtLeastOnePieceOfDestinationSet_ShouldNotThrowException(string address, string city, string state, string postalCode, string country)
        {
            var newEntity = new LoadCarrierGroupEntity { LoadCarrierGroupId = 0, LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>() };
            _db = new MockDbBuilder().WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { newEntity }).Build();
            InitService();

            var createGroup = new LoadCarrierGroupDetailData()
            {
                CustomerId = CUSTOMER_ID_2,
                DestinationAddress1 = address,
                DestinationCity = city,
                DestinationState = state,
                DestinationPostalCode = postalCode,
                DestinationCountry = country
            };

            Action action = () => _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            action.Should().NotThrow();
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenAtLeastEquipmentIsSet_ShouldNotThrowException()
        {
            var newEntity = new LoadCarrierGroupEntity { LoadCarrierGroupId = 0, LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>() };
            _db = new MockDbBuilder().WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { newEntity }).Build();
            InitService();

            var createGroup = new LoadCarrierGroupDetailData()
            {
                CustomerId = CUSTOMER_ID_2,
                LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentData>
                {
                    new LoadCarrierGroupEquipmentData
                    {
                        LoadCarrierGroupId = 300,
                        EquipmentId = "Equipment Id"
                    }
                }
            };

            Action action = () => _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            action.Should().NotThrow();
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenCreateSucceeds_ShouldCallAdd()
        {
            var newEntity = new LoadCarrierGroupEntity {
                LoadCarrierGroupId = 0,
                LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>(),
                CustomerId = CUSTOMER_ID
            };
            var builder = new MockDbBuilder();
            _db = builder
                .WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { newEntity })
                .Build();

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            LoadCarrierGroupEntity added = null;
            builder.MockLoadCarrierGroups.Setup(_ => _.Add(It.IsAny<LoadCarrierGroupEntity>())).Callback((LoadCarrierGroupEntity _) => { added = _; });

            InitService();

            var createGroup = new LoadCarrierGroupDetailData()
            {
                GroupName = "Group 3 Update",
                GroupDescription = "Group Description 3  Update",
                CustomerId = CUSTOMER_ID_2,
                OriginAddress1 = "Origin Address 3 Update",
                OriginCity = "Origin City 3 Update",
                OriginState = "Origin State 3 Update",
                OriginPostalCode = "OZipUpdate",
                OriginCountry = "Origin Country 3 Update",
                DestinationAddress1 = "Destination Address 3 Update",
                DestinationCity = "Destination City 3 Update",
                DestinationState = "Destination State 3 Update",
                DestinationPostalCode = "DZipUpdate",
                DestinationCountry = "Destination Country 3 Update",
                LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentData>
                {
                    new LoadCarrierGroupEquipmentData
                    {
                        LoadCarrierGroupId = 0,
                        EquipmentId = "Equipment 1 Update"
                    }
                },
                Carriers = new List<LoadCarrierGroupCarrierData>()
                {
                    new LoadCarrierGroupCarrierData()
                    {
                        LoadCarrierGroupId = 0,
                        CarrierId = "test"
                    }
                }
            };
            var expected = _mapper.Map<LoadCarrierGroupEntity>(createGroup);
            // manually assign properties as these do not get automaped
            expected.LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentEntity>()
            {
                new LoadCarrierGroupEquipmentEntity()
                {
                    LoadCarrierGroupId = 0,
                    EquipmentId = "Equipment 1 Update"
                }
            };

            expected.LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>()
            {
                new LoadCarrierGroupCarrierEntity()
                {
                    LoadCarrierGroupId =0,
                    CarrierId = "test"
                }
            };
            
            _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            added.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenCreateSucceeds_ShouldCallSaveChanges()
        {
            var newEntity = new LoadCarrierGroupEntity { LoadCarrierGroupId = 0, LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>(), CustomerId = CUSTOMER_ID };
            _db = new MockDbBuilder().WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { newEntity }).Build();
            InitService();

            var createGroup = _mapper.Map<LoadCarrierGroupDetailData>(GROUP_3);
            createGroup.LoadCarrierGroupId = 0;
            createGroup.Carriers.ForEach(x => x.LoadCarrierGroupId = 0);

            _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            _db.Verify(_ => _.SaveChanges("testuser"), Times.Once());
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenDatabaseThrowsDuplicate_ShouldReturnModelStateError()
        {
            var createGroup = _mapper.Map<LoadCarrierGroupDetailData>(GROUP_3);
            createGroup.LoadCarrierGroupId = 0;
            createGroup.Carriers.ForEach(x => x.LoadCarrierGroupId = 0);

            // map the carrier id in carriers as it is not automapped

            var ex = new DbUpdateException("Testing", new Exception("Violation of UNIQUE KEY constraint"));
            _db.Setup(x => x.LoadCarrierGroups).Throws(ex);

            var expectedErrorMessage =
                "A carrier group already exists for:" + Environment.NewLine
                  + $"Origin Address1 - {createGroup.OriginAddress1}{Environment.NewLine}"
                  + $"Origin City - {createGroup.OriginCity}{Environment.NewLine}"
                  + $"Origin State - {createGroup.OriginState}{Environment.NewLine}"
                  + $"Origin Postal Code - {createGroup.OriginPostalCode}{Environment.NewLine}"
                  + $"Origin Country - {createGroup.OriginCountry}{Environment.NewLine}"
                  + $"Destination Address1 - {createGroup.DestinationAddress1}{Environment.NewLine}"
                  + $"Destination City - {createGroup.DestinationCity}{Environment.NewLine}"
                  + $"Destination State - {createGroup.DestinationState}{Environment.NewLine}"
                  + $"Destination Postal Code - {createGroup.DestinationPostalCode}{Environment.NewLine}"
                  + $"Destination Country - {createGroup.DestinationCountry}{Environment.NewLine}"
                  + $"Equipment Type(s) - {string.Join(", ", createGroup.LoadCarrierGroupEquipment.Select(x => x.EquipmentId))}{Environment.NewLine}";

            var response = _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            response.IsSuccess.Should().BeFalse();
            response.ModelState["urn:LoadCarrierGroup"].Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Fact]
        public void CreateLoadCarrierGroup_WhenStateNameHasAbbreviation_ShouldUpdateToAbbreviation()
        {
            var newEntity = new LoadCarrierGroupEntity { LoadCarrierGroupId = 0, LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>(), CustomerId = CUSTOMER_ID };

            var builder = new MockDbBuilder();
            _db = builder.WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { newEntity }).Build();
            
            builder.MockLoadCarrierGroups.Setup(_ => _.Add(It.IsAny<LoadCarrierGroupEntity>()));
            _common.Reset();
            _common.Setup(_ => _.GetUSCANStateProvince("Origin State 3")).Returns(new StateData { Abbreviation = "OS3" });
            _common.Setup(_ => _.GetUSCANStateProvince("Destination State 3")).Returns(new StateData { Abbreviation = "DS3" });

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();

            var createGroup = new LoadCarrierGroupDetailData()
            {
                GroupName = "Group 3",
                GroupDescription = "Group Description 3 ",
                CustomerId = CUSTOMER_ID_2,
                OriginState = "Origin State 3",
                DestinationState = "Destination State 3"
            };
            _svc.CreateLoadCarrierGroup(createGroup, "testuser");
            builder.MockLoadCarrierGroups.Verify(_ => _.Add(It.Is<LoadCarrierGroupEntity>(e => e.OriginState == "OS3" && e.DestinationState == "DS3")), Times.Once());
        }

        [Fact]
        public void DeleteLoadCarrierGroup_WhenDeleteSucceeds_ShouldCallRemove()
        {
            var deleteGroup = GROUP_3;

            LoadCarrierGroupEntity removed = null;
            var builder = new MockDbBuilder();
            _db = builder
                .WithUser(ADMIN_USER)
                .WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { GROUP_1, GROUP_2, GROUP_3 })
                .WithLoadCarrierGroupCarriers(GROUP_CARRIER_GROUPS)
                .Build();
            builder.MockLoadCarrierGroups.Setup(_ => _.Remove(It.IsAny<LoadCarrierGroupEntity>())).Callback((LoadCarrierGroupEntity _) => { removed = _; });

            InitService();
            _svc.DeleteLoadCarrierGroup(deleteGroup.LoadCarrierGroupId);
            removed.Should().BeEquivalentTo(deleteGroup);
        }

        [Fact]
        public void DeleteLoadCarrierGroup_WhenDeleteSucceeds_ShouldCallSaveChanges()
        {
            var deleteGroup = GROUP_3;
            _svc.DeleteLoadCarrierGroup(deleteGroup.LoadCarrierGroupId);
            _db.Verify(_ => _.SaveChanges(), Times.Once());
        }

        [Fact]
        public void DeleteLoadCarrierGroup_WhenEntityNotFound_ShouldThrowException()
        {
            Action action = () => _svc.DeleteLoadCarrierGroup(99);
            action.Should().Throw<Exception>().WithMessage($"LoadCarrierGroup not found in the database with LoadCarrierGroupId: {99}");
        }

        [Fact]
        public void DeleteLoadCarrierGroup_WhenDbSetThrowsExceptoin_ShouldThrowException()
        {
            var deleteGroup = GROUP_3;
            var ex = new Exception("Testing");
            _db.Setup(x => x.LoadCarrierGroups).Throws(ex);

            Action action = () => _svc.DeleteLoadCarrierGroup(deleteGroup.LoadCarrierGroupId);
            action.Should().Throw<Exception>().And.Equals(ex);
        }

        [Fact]
        public void GetLoadCarrierGroupCarriers_ShouldReturnExpectedCarriers()
        {
            _security = new Mock<ISecurityService>();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2 }
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();

            var loadCarrierGroup = _db.Object.LoadCarrierGroups.Single(lcg => lcg.LoadCarrierGroupId == 200);
            loadCarrierGroup.LoadCarrierGroupCarriers = _db.Object.LoadCarrierGroupCarriers.Where(x => x.LoadCarrierGroupId == 200).ToList();

            var carriers = _svc.GetLoadCarrierGroupCarriers(200);

            carriers.Should().BeEquivalentTo(new List<LoadCarrierGroupCarrierData>
            {
                new LoadCarrierGroupCarrierData { LoadCarrierGroupCarrierId = 3, LoadCarrierGroupId = 200, CarrierId = "Carrier 3" },
                new LoadCarrierGroupCarrierData { LoadCarrierGroupCarrierId = 4, LoadCarrierGroupId = 200, CarrierId = "Carrier 4" }
            });
        }

        [Fact]
        public void GetLoadCarrierGroupCarriers_WhenNoCarriersForGroup_ShouldReturnEmptyList()
        {
            var exMessage = "LoadCarrierGroup not found in the database with LoadCarrierGroupId: 99";

            var thrownEx = Assert.Throws<Exception>(() => _svc.GetLoadCarrierGroupCarriers(99));
            thrownEx.Message.Should().Be(exMessage);
        }

        [Fact]
        public void GetLoadCarrierGroupCarriers_WhenDBThrowsException_ShouldReturnNullGroup()
        {
            var ex = new Exception("Db Exception");
            _db.Setup(x => x.LoadCarrierGroups).Throws(ex);

            var thrownEx = Assert.Throws<Exception>(() => _svc.GetLoadCarrierGroupCarriers(-1));
            thrownEx.Should().Be(ex);
        }

        [Fact]
        public void AddLoadCarrierGroupCarriers_WhenAddSucceeds_ShouldCallAddRange()
        {
            var newEntities = new List<LoadCarrierGroupCarrierEntity>
            {
                new LoadCarrierGroupCarrierEntity { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 1" },
                new LoadCarrierGroupCarrierEntity { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 2" }
            };
            var addCarriers = new List<LoadCarrierGroupCarrierData>()
            {
                new LoadCarrierGroupCarrierData { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 1" },
                new LoadCarrierGroupCarrierData { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 2" }
            };

            var builder = new MockDbBuilder();

            _db = builder
               .WithUser(ADMIN_USER)
               .WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { GROUP_1, GROUP_2, GROUP_3 })
               .WithLoadCarrierGroupCarriers(newEntities)
               .Build();
            
            builder.MockLoadCarrierGroups.Setup(_ => _.Add(It.IsAny<LoadCarrierGroupEntity>()));

            var expected = _mapper.Map<List<LoadCarrierGroupCarrierEntity>>(addCarriers);

            IEnumerable<LoadCarrierGroupCarrierEntity> added = null;
            builder.MockLoadCarrierGroupCarriers.Setup(_ => _.AddRange(It.IsAny<List<LoadCarrierGroupCarrierEntity>>())).Callback((IEnumerable<LoadCarrierGroupCarrierEntity> _) => { added = _; });

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();
            _svc.AddLoadCarrierGroupCarriers(addCarriers, "testuser");

            added.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void AddLoadCarrierGroupCarriers_WhenAddSucceeds_ShouldCallSaveChanges()
        {
            var newEntities = new List<LoadCarrierGroupCarrierEntity>
            {
                new LoadCarrierGroupCarrierEntity { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 1" },
                new LoadCarrierGroupCarrierEntity { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 2" }
            };
            var addCarriers = new List<LoadCarrierGroupCarrierData>()
            {
                new LoadCarrierGroupCarrierData { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 1" },
                new LoadCarrierGroupCarrierData { LoadCarrierGroupCarrierId = 0, LoadCarrierGroupId = 200, CarrierId = "New Carrier 2" }

            };

            _db = new MockDbBuilder()
               .WithUser(ADMIN_USER)
               .WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { GROUP_1, GROUP_2, GROUP_3 })
               .WithLoadCarrierGroupCarriers(newEntities)
               .Build();

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();
            _svc.AddLoadCarrierGroupCarriers(addCarriers, "testuser");
            _db.Verify(_ => _.SaveChanges("testuser"), Times.Once());
        }

        [Fact]
        public void AddLoadCarrierGroupCarrier_WhenDatabaseThrowsDuplicate_ShouldThrowException()
        {
            var addCarrier = _mapper.Map<LoadCarrierGroupCarrierData>(GROUP_CARRIER_GROUPS[2]);
            addCarrier.LoadCarrierGroupCarrierId = 0;

            var ex = new DbUpdateException("Testing", new Exception("Violation of UNIQUE KEY constraint"));
            _db.Setup(x => x.LoadCarrierGroupCarriers).Throws(ex);

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();

            Action action = () => _svc.AddLoadCarrierGroupCarriers(new List<LoadCarrierGroupCarrierData> { addCarrier }, "testuser");
            action.Should().Throw<Exception>().WithMessage(
                "A carrier was attempted to be added that already exists for:" + Environment.NewLine
                + $"Load Carrier Group Id - {addCarrier.LoadCarrierGroupId}{Environment.NewLine}"
                );
        }

        [Fact]
        public void AddLoadCarrierGroupCarrier_WhenNoLoadCarrierGroupId_ShouldThrowException()
        {
            var addCarrier = new LoadCarrierGroupCarrierData();

            Action action = () => _svc.AddLoadCarrierGroupCarriers(new List<LoadCarrierGroupCarrierData> { addCarrier }, "testuser");
            action.Should().Throw<Exception>().WithMessage("LoadCarrierGroupCarrier must have a LoadCarrierGroupId.");
        }

        [Fact]
        public void AddLoadCarrierGroupCarrier_WhenLoadCarrierGroupCarrierIdHasValue_ShouldThrowException()
        {
            var addCarrier = new LoadCarrierGroupCarrierData { LoadCarrierGroupCarrierId = 1, LoadCarrierGroupId = 200 };

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();

            Action action = () => _svc.AddLoadCarrierGroupCarriers(new List<LoadCarrierGroupCarrierData> { addCarrier }, "testuser");
            action.Should().Throw<Exception>().WithMessage("LoadCarrierGroupCarrier should not have an LoadCarrierGroupCarrierId assigned when creating.");
        }

        [Fact]
        public void AddLoadCarrierGroupCarrier_WhenMultipleCarriersWithDifferentGroupIds_ShouldThrowException()
        {
            var addCarriers = new List<LoadCarrierGroupCarrierData>
            {
                new LoadCarrierGroupCarrierData { LoadCarrierGroupId = 200, CarrierId = "Carrier 1" },
                new LoadCarrierGroupCarrierData { LoadCarrierGroupId = 300, CarrierId = "Carrier 2" }
            };

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();

            Action action = () => _svc.AddLoadCarrierGroupCarriers(addCarriers, "testuser");
            action.Should().Throw<Exception>().WithMessage("All carriers being added must have the same LoadCarrierGroupId.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void AddLoadCarrierGroupCarrier_WhenNoCarrierId_ShouldThrowException(string carrierId)
        {
            var addCarriers = new List<LoadCarrierGroupCarrierData>
            {
                new LoadCarrierGroupCarrierData { LoadCarrierGroupId = 200, CarrierId = "Carrier 1" },
                new LoadCarrierGroupCarrierData { LoadCarrierGroupId = 200, CarrierId = carrierId }
            };

            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();

            Action action = () => _svc.AddLoadCarrierGroupCarriers(addCarriers, "testuser");
            action.Should().Throw<Exception>().WithMessage("LoadCarrierGroupCarrier must have a CarrierId.");
        }

        [Fact]
        public void DeleteLoadCarrierGroupCarriers_WhenDeleteSucceeds_ShouldCallRemove()
        {
            LoadCarrierGroupCarrierEntity deleted = null;

            var builder = new MockDbBuilder();

            _db = builder
               .WithUser(ADMIN_USER)
               .WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { GROUP_1, GROUP_2, GROUP_3 })
               .WithLoadCarrierGroupCarriers(GROUP_CARRIER_GROUPS)
               .Build();

            builder.MockLoadCarrierGroupCarriers.Setup(_ => _.Remove(It.IsAny<LoadCarrierGroupCarrierEntity>())).Callback((LoadCarrierGroupCarrierEntity _) => { deleted = _; });
            InitService();

            _svc.DeleteLoadCarrierGroupCarrier(1);
            deleted.LoadCarrierGroupCarrierId.Should().Be(1);
        }

        [Fact]
        public void DeleteLoadCarrierGroupCarriers_WhenDeleteSucceeds_ShouldCallSaveChanges()
        {
            _svc.DeleteLoadCarrierGroupCarrier(1);
            _db.Verify(_ => _.SaveChanges(), Times.Once());
        }

        [Fact]
        public void DeleteLoadCarrierGroupCarrier_WhenDatabaseThrows_ShouldThrowException()
        {
            var ex = new Exception("Testing");
            _db.Setup(x => x.LoadCarrierGroupCarriers).Throws(ex);

            Action action = () => _svc.DeleteLoadCarrierGroupCarrier(1);
            action.Should().Throw<Exception>().And.Should().Be(ex);
        }

        [Fact]
        public void DeleteLoadCarrierGroupCarrier_WhenRecordDoesntExists_ShouldThrowException()
        {
            Action action = () => _svc.DeleteLoadCarrierGroupCarrier(99);
            action.Should().Throw<Exception>().WithMessage($"LoadCarrierGroupCarrier not found in the database with LoadCarrierGroupCarrierId: {99}");
        }

        [Fact]
        public void DeleteAllLoadCarrierGroupCarriers_WhenDeleteSucceeds_ShouldCallRemoveRange()
        {
            IEnumerable<LoadCarrierGroupCarrierEntity> deleted = null;
            var expected = GROUP_CARRIER_GROUPS.Where(_ => _.LoadCarrierGroupId == 200).ToList();
            var loadCarrierGroup = _db.Object.LoadCarrierGroups.Single(lcg => lcg.LoadCarrierGroupId == 200);

            loadCarrierGroup.LoadCarrierGroupCarriers = expected;

            var builder = new MockDbBuilder();

            _db = builder
               .WithUser(ADMIN_USER)
               .WithLoadCarrierGroups(new List<LoadCarrierGroupEntity> { GROUP_1, GROUP_2, GROUP_3 })
               .WithLoadCarrierGroupCarriers(GROUP_CARRIER_GROUPS)
               .Build();

            builder.MockLoadCarrierGroupCarriers.Setup(_ => _.RemoveRange(It.IsAny<IEnumerable<LoadCarrierGroupCarrierEntity>>())).Callback((IEnumerable<LoadCarrierGroupCarrierEntity> _) => { deleted = _; });
            
            _security.Reset();
            _security.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData> {
                new CustomerData { CustomerId = CUSTOMER_ID },
                new CustomerData { CustomerId = CUSTOMER_ID_2}
            });
            _security.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);

            InitService();

            _svc.DeleteAllLoadCarrierGroupCarriers(200);

            deleted.Should().HaveCount(2);
            deleted.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DeleteAllLoadCarrierGroupCarriers_WhenDeleteSucceeds_ShouldCallSaveChanges()
        {
            _svc.DeleteAllLoadCarrierGroupCarriers(100);
            _db.Verify(_ => _.SaveChanges(), Times.Once());
        }

        [Fact]
        public void DeleteAllLoadCarrierGroupCarriers_WhenDatabaseThrows_ShouldThrowException()
        {
            var ex = new Exception("Testing");
            _db.Setup(x => x.LoadCarrierGroupCarriers).Throws(ex);

            Action action = () => _svc.DeleteAllLoadCarrierGroupCarriers(100);
            action.Should().Throw<Exception>().And.Should().Be(ex);
        }

        private void InitSeedData()
        {
            GROUP_1 = new LoadCarrierGroupEntity()
            {
                LoadCarrierGroupId = 100,
                GroupName = "Group 1",
                GroupDescription = null,
                CustomerId = CUSTOMER_ID,
                LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>(),
                LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentEntity>()
            };
            GROUP_2 = new LoadCarrierGroupEntity()
            {
                LoadCarrierGroupId = 200,
                GroupName = "Group 2",
                GroupDescription = "",
                CustomerId = CUSTOMER_ID_2,
                LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>(),
                LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentEntity>()
            };
            GROUP_3 = new LoadCarrierGroupEntity()
            {
                LoadCarrierGroupId = 300,
                GroupName = "Group 3",
                GroupDescription = "Group Description 3",
                CustomerId = CUSTOMER_ID,
                OriginAddress1 = "Origin Address 3",
                OriginCity = "Origin City 3",
                OriginState = "Origin State 3",
                OriginPostalCode = "PostalCode",
                OriginCountry = "Origin Country 3",
                DestinationAddress1 = "Destination Address 3",
                DestinationCity = "Destination City 3",
                DestinationState = "Destination State 3",
                DestinationPostalCode = "PostalCode",
                DestinationCountry = "Destination Country 3",
                LoadCarrierGroupEquipment = new List<LoadCarrierGroupEquipmentEntity>{
                    new LoadCarrierGroupEquipmentEntity
                    {
                        EquipmentId = "EQ 1",
                        LoadCarrierGroupId = 300,
                        Equipment = new EquipmentEntity
                        {
                            EquipmentId = "EQ 1",
                            EquipmentDesc = "EQ Desc",
                            Sort = 5
                        }
                    }
                },
                Customer = new CustomerEntity
                {
                    CustomerId = Guid.Parse("594AC743-1025-46C8-B0DC-124DBB52E67D"),
                    Name = "Customer 1"
                },
                LoadCarrierGroupCarriers = new List<LoadCarrierGroupCarrierEntity>
                {
                    new LoadCarrierGroupCarrierEntity()
                    {
                        LoadCarrierGroupCarrierId = 1,
                        CarrierId = "test",
                        LoadCarrierGroupId=300
                    },
                    new LoadCarrierGroupCarrierEntity()
                    {
                        LoadCarrierGroupCarrierId = 2,
                        CarrierId = "test2",
                        LoadCarrierGroupId=300
                        },
                }
            };
            GROUP_CARRIER_GROUPS = new List<LoadCarrierGroupCarrierEntity>
            {
                new LoadCarrierGroupCarrierEntity() { LoadCarrierGroupCarrierId = 1, LoadCarrierGroupId = 100, CarrierId = "Carrier 1", LoadCarrierGroup = GROUP_1 },
                new LoadCarrierGroupCarrierEntity() { LoadCarrierGroupCarrierId = 2, LoadCarrierGroupId = 100, CarrierId = "Carrier 2", LoadCarrierGroup = GROUP_1 },
                new LoadCarrierGroupCarrierEntity() { LoadCarrierGroupCarrierId = 3, LoadCarrierGroupId = 200, CarrierId = "Carrier 3", LoadCarrierGroup = GROUP_2 },
                new LoadCarrierGroupCarrierEntity() { LoadCarrierGroupCarrierId = 4, LoadCarrierGroupId = 200, CarrierId = "Carrier 4", LoadCarrierGroup = GROUP_2 },
                new LoadCarrierGroupCarrierEntity() { LoadCarrierGroupCarrierId = 5, LoadCarrierGroupId = 300, CarrierId = "Carrier 5", LoadCarrierGroup = GROUP_3 }
            };

            
            ADMIN_USER = new UserEntity()
            {
                IdentUserId = ADMIN_IDENT_USER_ID,
                UserId = ADMIN_USER_ID,
                FirstName = "Admin",
                LastName = "User",
                PrimaryScac = "KBXL",
                PrimaryCustomerId = CUSTOMER_ID,
                IsNotificationsEnabled = true,
                UserShippers = new List<UserShipperEntity>(),
                UserCarrierScacs = new List<UserCarrierScacEntity>(),
                SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>()
                {
                    new SecurityUserAccessRoleEntity()
                    {
                        UserId = ADMIN_USER_ID,
                        AccessRoleId = ADMIN_ROLE_ID,
                        SecurityAccessRole = new SecurityAccessRoleEntity
                        {
                            AccessRoleId = ADMIN_ROLE_ID,
                            AccessRoleName = SecurityRoles.SystemAdmin,
                            AccessRoleLevel = 1,
                            SecurityUserAccessRoles = new List<SecurityUserAccessRoleEntity>(),
                            SecurityAccessRoleAppActions = new List<SecurityAccessRoleAppActionEntity>()
                        }
                    }
                }
            };
        }
    }
}
