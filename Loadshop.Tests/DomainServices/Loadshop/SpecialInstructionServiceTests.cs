using AutoMapper;
using FluentAssertions;
using Ganss.XSS;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Common.Services.Data;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class SpecialInstructionServiceTests
    {
        public class GetSpecialInstructionAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly IHtmlSanitizer _htmlSanitizer;

            private ISpecialInstructionsService _svc;

            private List<SpecialInstructionEntity> SPECIAL_INSTRUCTIONS;

            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly long SPECIAL_INSTRUCTIONS_ID = 1;

            public GetSpecialInstructionAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>
                {
                    new CustomerData
                    {
                        CustomerId = CUSTOMER_ID
                    }
                });
                _commonService = new Mock<ICommonService>();
                _htmlSanitizer = new HtmlSanitizer();

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
                    .WithSpecialInstructions(SPECIAL_INSTRUCTIONS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SpecialInstructionsService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _htmlSanitizer);
            }

            [Fact]
            public void NoInstructions_ThrowsException()
            {
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>();
                InitDb();
                InitService();

                var expected = "Special Instructions not found.";
                _svc.Awaiting(x => x.GetSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void InvalidInstructionId_ThrowsException()
            {
                var expected = "Special Instructions not found.";
                _svc.Awaiting(x => x.GetSpecialInstructionAsync(99))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void NotAuthorizedForCustomer_ThrowsException()
            {
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>());
                InitService();

                var expected = $"User is not authorized for customer: {CUSTOMER_ID}";
                _svc.Awaiting(x => x.GetSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public async Task ReturnsInstruction()
            {
                var actual = await _svc.GetSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID);
                actual.Should().NotBeNull();
            }

            [Fact]
            public async Task SanitizesHtmlComments()
            {
                var expected = "<p>some bad comments </p>";
                var actual = await _svc.GetSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID);
                actual.Should().NotBeNull();
                actual.Comments.Should().Be(expected);
            }

            private void InitSeedData()
            {
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>
                {
                    new SpecialInstructionEntity
                    {
                        SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID,
                        CustomerId = CUSTOMER_ID,
                        Comments = "<p>some bad comments <script>alert('xss')</script></p>",
                    }
                };
            }
        }

        public class GetSpecialInstructionsAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly IHtmlSanitizer _htmlSanitizer;

            private ISpecialInstructionsService _svc;

            private List<UserEntity> USERS;
            private List<SpecialInstructionEntity> SPECIAL_INSTRUCTIONS;

            private static readonly Guid USER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");

            public GetSpecialInstructionsAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(x => x.UserId).Returns(USER_ID);
                _securityService = new Mock<ISecurityService>();
                _commonService = new Mock<ICommonService>();
                _htmlSanitizer = new HtmlSanitizer();

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
                    .WithUsers(USERS)
                    .WithSpecialInstructions(SPECIAL_INSTRUCTIONS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SpecialInstructionsService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _htmlSanitizer);
            }

            [Fact]
            public async Task NoUsers_ReturnsEmptyList()
            {
                USERS = new List<UserEntity>();
                InitDb();
                InitService();

                var actual = await _svc.GetSpecialInstructionsAsync();
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task NoUserPrimaryCustomerId_ReturnsEmptyList()
            {
                USERS.First().PrimaryCustomerId = Guid.Empty;
                InitDb();
                InitService();

                var actual = await _svc.GetSpecialInstructionsAsync();
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task NoInstructions_ReturnsEmptyList()
            {
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>();
                InitDb();
                InitService();

                var actual = await _svc.GetSpecialInstructionsAsync();
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task ReturnsOnlySpecialInstructionsForCustomer()
            {
                SPECIAL_INSTRUCTIONS.First().CustomerId = Guid.Empty;
                InitDb();
                InitService();

                var actual = await _svc.GetSpecialInstructionsAsync();
                actual.Should().HaveCount(1);
            }

            [Fact]
            public async Task ReturnsInstructions()
            {
                var actual = await _svc.GetSpecialInstructionsAsync();
                actual.Should().HaveCount(SPECIAL_INSTRUCTIONS.Count);
            }

            [Fact]
            public async Task SanitizesHtmlComments()
            {
                var expected = "<p>some bad comments </p>";
                var actual = await _svc.GetSpecialInstructionsAsync();
                actual.ForEach(x => x.Comments.Should().Be(expected));
            }

            private void InitSeedData()
            {
                USERS = new List<UserEntity>
                {
                    new UserEntity
                    {
                        IdentUserId = USER_ID,
                        PrimaryCustomerId = CUSTOMER_ID
                    }
                };
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>
                {
                    new SpecialInstructionEntity
                    {
                        CustomerId = CUSTOMER_ID,
                        Comments = "<p>some bad comments <script>alert('xss')</script></p>",
                    },
                    new SpecialInstructionEntity
                    {
                        CustomerId = CUSTOMER_ID,
                        Comments = "<p>some bad comments <script>alert('xss')</script></p>",
                    }
                };
            }
        }

        public class DeleteSpecialInstructionAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly IHtmlSanitizer _htmlSanitizer;

            private ISpecialInstructionsService _svc;

            private List<SpecialInstructionEntity> SPECIAL_INSTRUCTIONS;
            private List<SpecialInstructionEquipmentEntity> SPECIAL_INSTRUCTION_EQUIPMENT;

            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly long SPECIAL_INSTRUCTIONS_ID = 1;

            public DeleteSpecialInstructionAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>
                {
                    new CustomerData
                    {
                        CustomerId = CUSTOMER_ID
                    }
                });
                _commonService = new Mock<ICommonService>();
                _htmlSanitizer = new HtmlSanitizer();

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
                    .WithSpecialInstructions(SPECIAL_INSTRUCTIONS)
                    .WithSpecialInstructionEquipment(SPECIAL_INSTRUCTION_EQUIPMENT)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SpecialInstructionsService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _htmlSanitizer);
            }

            [Fact]
            public void NoInstructions_ThrowsException()
            {
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>();
                InitDb();
                InitService();

                var expected = $"Special Instruction not found in the database with Id: {SPECIAL_INSTRUCTIONS_ID}";
                _svc.Awaiting(x => x.DeleteSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void InvalidInstructionId_ThrowsException()
            {
                var expected = $"Special Instruction not found in the database with Id: 99";
                _svc.Awaiting(x => x.DeleteSpecialInstructionAsync(99))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void NotAuthorizedForCustomer_ThrowsException()
            {
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>());
                InitService();

                var expected = $"User is not authorized for customer: {CUSTOMER_ID}";
                _svc.Awaiting(x => x.DeleteSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public async Task Delete_RemovesSpecialInstructionEquipment()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.DeleteSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID);
                builder.MockSpecialInstructionEquipment.Verify(x => x.RemoveRange(It.IsAny<List<SpecialInstructionEquipmentEntity>>()), Times.Once);
            }

            [Fact]
            public async Task Delete_RemovesSpecialInstruction()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                await _svc.DeleteSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID);
                builder.MockSpecialInstructions.Verify(x => x.Remove(It.IsAny<SpecialInstructionEntity>()), Times.Once);
            }

            [Fact]
            public async Task Delete_RemovesAreSaved()
            {
                await _svc.DeleteSpecialInstructionAsync(SPECIAL_INSTRUCTIONS_ID);
                _db.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
            }

            private void InitSeedData()
            {
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>
                {
                    new SpecialInstructionEntity
                    {
                        SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID,
                        CustomerId = CUSTOMER_ID,
                        Comments = "<p>some bad comments <script>alert('xss')</script></p>",
                    }
                };
                SPECIAL_INSTRUCTION_EQUIPMENT = new List<SpecialInstructionEquipmentEntity>
                {
                    new SpecialInstructionEquipmentEntity
                    {
                        SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID
                    }
                };
            }
        }

        public class UpdateSpecialInstructionAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly IHtmlSanitizer _htmlSanitizer;

            private ISpecialInstructionsService _svc;

            private List<SpecialInstructionEntity> SPECIAL_INSTRUCTIONS;
            private List<SpecialInstructionEquipmentEntity> SPECIAL_INSTRUCTION_EQUIPMENT;

            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly long SPECIAL_INSTRUCTIONS_ID = 1;
            private static readonly long DUPE_SPECIAL_INSTRUCTIONS_ID = 2;
            private static readonly string USERNAME = "username";
            private static readonly string ErrorPrefix = "urn:SpecialInstruction";

            private SpecialInstructionData SPECIAL_INSTRUCTION_DATA;

            public UpdateSpecialInstructionAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser())
                    .Returns(new List<CustomerData>
                    {
                        new CustomerData
                        {
                            CustomerId = CUSTOMER_ID
                        }
                    });
                _commonService = new Mock<ICommonService>();
                _commonService.Setup(x => x.GetUSCANStateProvince(It.IsAny<string>()))
                    .Returns(new StateData
                    { 
                        Name = "State",
                        Abbreviation = "ST"
                    });
                _htmlSanitizer = new HtmlSanitizer();

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
                    .WithSpecialInstructions(SPECIAL_INSTRUCTIONS)
                    .WithSpecialInstructionEquipment(SPECIAL_INSTRUCTION_EQUIPMENT)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SpecialInstructionsService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _htmlSanitizer);
            }

            [Fact]
            public async Task NullInstructionData_ReturnsError()
            {
                var actual = await _svc.UpdateSpecialInstructionAsync(null, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage
                    .Should().Be("Special Instruction should have an Id assigned when updating.");
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task InvalidId_ReturnsError(long id)
            {
                SPECIAL_INSTRUCTION_DATA.SpecialInstructionId = id;
                var actual = await _svc.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage
                    .Should().Be("Special Instruction should have an Id assigned when updating.");
            }

            [Fact]
            public async Task MissingCustomerId_ReturnsError()
            {
                SPECIAL_INSTRUCTION_DATA.CustomerId = default(Guid);
                var actual = await _svc.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have a Customer");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public async Task MissingName_ReturnsError(string input)
            {
                SPECIAL_INSTRUCTION_DATA.Name = input;
                var actual = await _svc.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have a name");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public async Task MissingOriginDestAndEquipment_ReturnsError(string input)
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = input;
                SPECIAL_INSTRUCTION_DATA.OriginState = input;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = input;
                SPECIAL_INSTRUCTION_DATA.DestinationCity = input;
                SPECIAL_INSTRUCTION_DATA.DestinationState = input;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = input;
                SPECIAL_INSTRUCTION_DATA.SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>();
                var actual = await _svc.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have an Origin, Destination, or Equipment");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("<p></p>")]
            public async Task MissingComments_ReturnsError(string input)
            {
                SPECIAL_INSTRUCTION_DATA.Comments = input;
                var actual = await _svc.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have an instruction");
            }

            [Fact]
            public async Task IdNotFound_ReturnsError()
            {
                SPECIAL_INSTRUCTION_DATA.SpecialInstructionId = 99;
                var actual = await _svc.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Special Instruction not found");
            }

            [Fact]
            public void NotAuthorizedForCustomer_ThrowsException()
            {
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser())
                    .Returns(new List<CustomerData>());
                InitService();

                var expected = $"User is not authorized for customer: {CUSTOMER_ID}";
                _svc.Awaiting(x => x.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Theory]
            [InlineData("City", "", "")]
            [InlineData("", "State", "")]
            [InlineData("", "", "Country")]
            public void UpdateSpecialInstruction_WhenAtLeastOnePieceOfOriginSet_ShouldNotThrowException(string city, string state, string country)
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = city;
                SPECIAL_INSTRUCTION_DATA.OriginState = state;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = country;

                SPECIAL_INSTRUCTION_DATA.DestinationCity = null;
                SPECIAL_INSTRUCTION_DATA.DestinationState = null;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = null;

                SPECIAL_INSTRUCTION_DATA.SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>();

                _svc.Awaiting(x => x.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().NotThrow();
            }

            [Theory]
            [InlineData("City", "", "")]
            [InlineData("", "State", "")]
            [InlineData("", "", "Country")]
            public void UpdateSpecialInstruction_WhenAtLeastOnePieceOfDestinationSet_ShouldNotThrowException(string city, string state, string country)
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = null;
                SPECIAL_INSTRUCTION_DATA.OriginState = null;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = null;

                SPECIAL_INSTRUCTION_DATA.DestinationCity = city;
                SPECIAL_INSTRUCTION_DATA.DestinationState = state;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = country;

                SPECIAL_INSTRUCTION_DATA.SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>();

                _svc.Awaiting(x => x.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().NotThrow();
            }

            [Fact]
            public void UpdateSpecialInstruction_WhenAtLeastEquipmentIsSet_ShouldNotThrowException()
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = null;
                SPECIAL_INSTRUCTION_DATA.OriginState = null;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = null;

                SPECIAL_INSTRUCTION_DATA.DestinationCity = null;
                SPECIAL_INSTRUCTION_DATA.DestinationState = null;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = null;

                _svc.Awaiting(x => x.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().NotThrow();
            }

            [Fact]
            public async Task DuplicateExists_ReturnsError()
            {
                var dupe = SPECIAL_INSTRUCTIONS.First(x => x.SpecialInstructionId == DUPE_SPECIAL_INSTRUCTIONS_ID);
                var input = _mapper.Map<SpecialInstructionData>(dupe);
                input.SpecialInstructionId += 1;

                var expectedErrorMessage = "A special instruction already exists for:" + Environment.NewLine
                    + $"Origin Address1 - {input.OriginAddress1}{Environment.NewLine}"
                    + $"Origin City - {input.OriginCity}{Environment.NewLine}"
                    + $"Origin State - {input.OriginState}{Environment.NewLine}"
                    + $"Origin Postal Code - {input.OriginPostalCode}{Environment.NewLine}"
                    + $"Origin Country - {input.OriginCountry}{Environment.NewLine}"
                    + $"Destination Address1 - {input.DestinationAddress1}{Environment.NewLine}"
                    + $"Destination City - {input.DestinationCity}{Environment.NewLine}"
                    + $"Destination State - {input.DestinationState}{Environment.NewLine}"
                    + $"Destination Postal Code - {input.DestinationPostalCode}{Environment.NewLine}"
                    + $"Destination Country - {input.DestinationCountry}{Environment.NewLine}"
                    + $"Equipment Type(s) - {string.Join(", ", input.SpecialInstructionEquipment.Select(x => x.EquipmentId))}{Environment.NewLine}";

                var actual = await _svc.UpdateSpecialInstructionAsync(input, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be(expectedErrorMessage.Trim());
            }

            [Fact]
            public void SavesUpdate()
            {
                var actual = _svc.UpdateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                _db.Verify(x => x.SaveChangesAsync(USERNAME, default), Times.Once);
            }

            private void InitSeedData()
            {
                SPECIAL_INSTRUCTION_DATA = new SpecialInstructionData
                {
                    SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID,
                    CustomerId = CUSTOMER_ID,
                    Name = "Name",
                    Description = "Special Instructions",
                    OriginCity = "Origin City",
                    OriginAddress1 = "123 Fake St.",
                    OriginState = "ST",
                    OriginPostalCode = "12345",
                    OriginCountry = "USA",
                    DestinationCity = "Dest City",
                    DestinationAddress1 = "123 Fake St.",
                    DestinationState = "ST",
                    DestinationPostalCode = "12345",
                    DestinationCountry = "USA",
                    SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>
                    {
                        new SpecialInstructionEquipmentData
                        {
                            SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID,
                            EquipmentId = "Equipment"
                        }
                    },
                    Comments = "<p>some bad comments <script>alert('xss')</script></p>"
                };
                SPECIAL_INSTRUCTION_EQUIPMENT = new List<SpecialInstructionEquipmentEntity>
                {
                    new SpecialInstructionEquipmentEntity
                    {
                        SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID
                    }
                };
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>
                {
                    new SpecialInstructionEntity
                    {
                        SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID,
                        CustomerId = CUSTOMER_ID,
                        Comments = "<p>some bad comments <script>alert('xss')</script></p>",
                        SpecialInstructionEquipment = SPECIAL_INSTRUCTION_EQUIPMENT
                    },
                    new SpecialInstructionEntity
                    {
                        SpecialInstructionId = DUPE_SPECIAL_INSTRUCTIONS_ID,
                        CustomerId = SPECIAL_INSTRUCTION_DATA.CustomerId,
                        Comments = SPECIAL_INSTRUCTION_DATA.Comments,
                        Name = SPECIAL_INSTRUCTION_DATA.Name,
                        Description = SPECIAL_INSTRUCTION_DATA.Description,
                        OriginCity = SPECIAL_INSTRUCTION_DATA.OriginCity,
                        OriginAddress1 = SPECIAL_INSTRUCTION_DATA.OriginAddress1,
                        OriginState = SPECIAL_INSTRUCTION_DATA.OriginState,
                        OriginPostalCode = SPECIAL_INSTRUCTION_DATA.OriginPostalCode,
                        OriginCountry = SPECIAL_INSTRUCTION_DATA.OriginCountry,
                        DestinationCity = SPECIAL_INSTRUCTION_DATA.DestinationCity,
                        DestinationAddress1 = SPECIAL_INSTRUCTION_DATA.DestinationAddress1,
                        DestinationState = SPECIAL_INSTRUCTION_DATA.DestinationState,
                        DestinationPostalCode = SPECIAL_INSTRUCTION_DATA.DestinationPostalCode,
                        DestinationCountry = SPECIAL_INSTRUCTION_DATA.DestinationCountry,
                        SpecialInstructionEquipment = new List<SpecialInstructionEquipmentEntity>
                        {
                            new SpecialInstructionEquipmentEntity
                            {
                                SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID,
                                EquipmentId = "Equipment"
                            }
                        }
                    },
                    new SpecialInstructionEntity
                    {
                        SpecialInstructionId = DUPE_SPECIAL_INSTRUCTIONS_ID + 1,
                        CustomerId = SPECIAL_INSTRUCTION_DATA.CustomerId,
                        Comments = SPECIAL_INSTRUCTION_DATA.Comments,
                        Name = SPECIAL_INSTRUCTION_DATA.Name,
                        Description = SPECIAL_INSTRUCTION_DATA.Description,
                        OriginCity = SPECIAL_INSTRUCTION_DATA.OriginCity,
                        OriginAddress1 = SPECIAL_INSTRUCTION_DATA.OriginAddress1,
                        OriginState = SPECIAL_INSTRUCTION_DATA.OriginState,
                        OriginPostalCode = SPECIAL_INSTRUCTION_DATA.OriginPostalCode,
                        OriginCountry = SPECIAL_INSTRUCTION_DATA.OriginCountry,
                        DestinationCity = SPECIAL_INSTRUCTION_DATA.DestinationCity,
                        DestinationAddress1 = SPECIAL_INSTRUCTION_DATA.DestinationAddress1,
                        DestinationState = SPECIAL_INSTRUCTION_DATA.DestinationState,
                        DestinationPostalCode = SPECIAL_INSTRUCTION_DATA.DestinationPostalCode,
                        DestinationCountry = SPECIAL_INSTRUCTION_DATA.DestinationCountry,
                        SpecialInstructionEquipment = new List<SpecialInstructionEquipmentEntity>
                        {
                            new SpecialInstructionEquipmentEntity
                            {
                                SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID,
                                EquipmentId = "Equipment"
                            }
                        }
                    }
                };
            }
        }

        public class CreateSpecialInstructionAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly IHtmlSanitizer _htmlSanitizer;

            private ISpecialInstructionsService _svc;

            private List<SpecialInstructionEntity> SPECIAL_INSTRUCTIONS;
            private List<SpecialInstructionEquipmentEntity> SPECIAL_INSTRUCTION_EQUIPMENT;

            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly long SPECIAL_INSTRUCTIONS_ID = 1;
            private static readonly long DUPE_SPECIAL_INSTRUCTIONS_ID = 2;
            private static readonly string USERNAME = "username";
            private static readonly string ErrorPrefix = "urn:SpecialInstruction";

            private SpecialInstructionData SPECIAL_INSTRUCTION_DATA;
            private SpecialInstructionEntity DUPLICATE;

            public CreateSpecialInstructionAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser())
                    .Returns(new List<CustomerData>
                    {
                        new CustomerData
                        {
                            CustomerId = CUSTOMER_ID
                        }
                    });
                _commonService = new Mock<ICommonService>();
                _commonService.Setup(x => x.GetUSCANStateProvince(It.IsAny<string>()))
                    .Returns(new StateData
                    {
                        Name = "State",
                        Abbreviation = "ST"
                    });
                _htmlSanitizer = new HtmlSanitizer();

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
                    .WithSpecialInstructions(SPECIAL_INSTRUCTIONS)
                    .WithSpecialInstructionEquipment(SPECIAL_INSTRUCTION_EQUIPMENT)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SpecialInstructionsService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _htmlSanitizer);
            }

            [Fact]
            public async Task NullInstructionData_ReturnsError()
            {
                var actual = await _svc.CreateSpecialInstructionAsync(null, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage
                    .Should().Be("Special Instruction is requred");
            }

            [Fact]
            public async Task MissingCustomerId_ReturnsError()
            {
                SPECIAL_INSTRUCTION_DATA.CustomerId = default(Guid);
                var actual = await _svc.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have a Customer");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public async Task MissingName_ReturnsError(string input)
            {
                SPECIAL_INSTRUCTION_DATA.Name = input;
                var actual = await _svc.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have a name");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public async Task MissingOriginDestAndEquipment_ReturnsError(string input)
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = input;
                SPECIAL_INSTRUCTION_DATA.OriginState = input;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = input;
                SPECIAL_INSTRUCTION_DATA.DestinationCity = input;
                SPECIAL_INSTRUCTION_DATA.DestinationState = input;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = input;
                SPECIAL_INSTRUCTION_DATA.SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>();
                var actual = await _svc.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have an Origin, Destination, or Equipment");
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("<p></p>")]
            public async Task MissingComments_ReturnsError(string input)
            {
                SPECIAL_INSTRUCTION_DATA.Comments = input;
                var actual = await _svc.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be("Must have an instruction");
            }

            [Fact]
            public void NotAuthorizedForCustomer_ThrowsException()
            {
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser())
                    .Returns(new List<CustomerData>());
                InitService();

                var expected = $"User is not authorized for customer: {CUSTOMER_ID}";
                _svc.Awaiting(x => x.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Theory]
            [InlineData("City", "", "")]
            [InlineData("", "State", "")]
            [InlineData("", "", "Country")]
            public void UpdateSpecialInstruction_WhenAtLeastOnePieceOfOriginSet_ShouldNotThrowException(string city, string state, string country)
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = city;
                SPECIAL_INSTRUCTION_DATA.OriginState = state;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = country;

                SPECIAL_INSTRUCTION_DATA.DestinationCity = null;
                SPECIAL_INSTRUCTION_DATA.DestinationState = null;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = null;

                SPECIAL_INSTRUCTION_DATA.SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>();

                _svc.Awaiting(x => x.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().NotThrow();
            }

            [Theory]
            [InlineData("City", "", "")]
            [InlineData("", "State", "")]
            [InlineData("", "", "Country")]
            public void UpdateSpecialInstruction_WhenAtLeastOnePieceOfDestinationSet_ShouldNotThrowException(string city, string state, string country)
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = null;
                SPECIAL_INSTRUCTION_DATA.OriginState = null;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = null;

                SPECIAL_INSTRUCTION_DATA.DestinationCity = city;
                SPECIAL_INSTRUCTION_DATA.DestinationState = state;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = country;

                SPECIAL_INSTRUCTION_DATA.SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>();

                _svc.Awaiting(x => x.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().NotThrow();
            }

            [Fact]
            public void UpdateSpecialInstruction_WhenAtLeastEquipmentIsSet_ShouldNotThrowException()
            {
                SPECIAL_INSTRUCTION_DATA.OriginCity = null;
                SPECIAL_INSTRUCTION_DATA.OriginState = null;
                SPECIAL_INSTRUCTION_DATA.OriginCountry = null;

                SPECIAL_INSTRUCTION_DATA.DestinationCity = null;
                SPECIAL_INSTRUCTION_DATA.DestinationState = null;
                SPECIAL_INSTRUCTION_DATA.DestinationCountry = null;

                _svc.Awaiting(x => x.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME))
                    .Should().NotThrow();
            }

            [Fact]
            public async Task DuplicateExists_ReturnsError()
            {
                var input = _mapper.Map<SpecialInstructionData>(DUPLICATE);
                input.SpecialInstructionId = 0;

                SPECIAL_INSTRUCTIONS.Add(DUPLICATE);
                InitDb();
                InitService();

                var expectedErrorMessage = "A special instruction already exists for:" + Environment.NewLine
                    + $"Origin Address1 - {input.OriginAddress1}{Environment.NewLine}"
                    + $"Origin City - {input.OriginCity}{Environment.NewLine}"
                    + $"Origin State - {input.OriginState}{Environment.NewLine}"
                    + $"Origin Postal Code - {input.OriginPostalCode}{Environment.NewLine}"
                    + $"Origin Country - {input.OriginCountry}{Environment.NewLine}"
                    + $"Destination Address1 - {input.DestinationAddress1}{Environment.NewLine}"
                    + $"Destination City - {input.DestinationCity}{Environment.NewLine}"
                    + $"Destination State - {input.DestinationState}{Environment.NewLine}"
                    + $"Destination Postal Code - {input.DestinationPostalCode}{Environment.NewLine}"
                    + $"Destination Country - {input.DestinationCountry}{Environment.NewLine}"
                    + $"Equipment Type(s) - {string.Join(", ", input.SpecialInstructionEquipment.Select(x => x.EquipmentId))}{Environment.NewLine}";

                var actual = await _svc.CreateSpecialInstructionAsync(input, USERNAME);
                actual.IsSuccess.Should().BeFalse();
                actual.ModelState[ErrorPrefix].Errors.Should().HaveCount(1);
                actual.ModelState[ErrorPrefix].Errors.First().ErrorMessage.Trim()
                    .Should().Be(expectedErrorMessage.Trim());
            }

            [Fact]
            public void SavesUpdate()
            {
                var actual = _svc.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                _db.Verify(x => x.SaveChangesAsync(USERNAME, default), Times.Once);
            }

            [Fact]
            public void InsertsNewSpecialInstruction()
            {
                var builder = new MockDbBuilder();
                InitDb(builder);
                InitService();

                var actual = _svc.CreateSpecialInstructionAsync(SPECIAL_INSTRUCTION_DATA, USERNAME);
                builder.MockSpecialInstructions.Verify(x => x.Add(It.IsAny<SpecialInstructionEntity>()), Times.Once);
            }

            private void InitSeedData()
            {
                SPECIAL_INSTRUCTION_DATA = new SpecialInstructionData
                {
                    CustomerId = CUSTOMER_ID,
                    Name = "Name",
                    Description = "Special Instructions",
                    OriginCity = "Origin City",
                    OriginAddress1 = "123 Fake St.",
                    OriginState = "ST",
                    OriginPostalCode = "12345",
                    OriginCountry = "USA",
                    DestinationCity = "Dest City",
                    DestinationAddress1 = "123 Fake St.",
                    DestinationState = "ST",
                    DestinationPostalCode = "12345",
                    DestinationCountry = "USA",
                    SpecialInstructionEquipment = new List<SpecialInstructionEquipmentData>
                    {
                        new SpecialInstructionEquipmentData
                        {
                            EquipmentId = "Equipment"
                        }
                    },
                    Comments = "<p>some bad comments <script>alert('xss')</script></p>"
                };
                DUPLICATE = new SpecialInstructionEntity
                {
                    SpecialInstructionId = DUPE_SPECIAL_INSTRUCTIONS_ID,
                    CustomerId = SPECIAL_INSTRUCTION_DATA.CustomerId,
                    Comments = SPECIAL_INSTRUCTION_DATA.Comments,
                    Name = SPECIAL_INSTRUCTION_DATA.Name,
                    Description = SPECIAL_INSTRUCTION_DATA.Description,
                    OriginCity = SPECIAL_INSTRUCTION_DATA.OriginCity,
                    OriginAddress1 = SPECIAL_INSTRUCTION_DATA.OriginAddress1,
                    OriginState = SPECIAL_INSTRUCTION_DATA.OriginState,
                    OriginPostalCode = SPECIAL_INSTRUCTION_DATA.OriginPostalCode,
                    OriginCountry = SPECIAL_INSTRUCTION_DATA.OriginCountry,
                    DestinationCity = SPECIAL_INSTRUCTION_DATA.DestinationCity,
                    DestinationAddress1 = SPECIAL_INSTRUCTION_DATA.DestinationAddress1,
                    DestinationState = SPECIAL_INSTRUCTION_DATA.DestinationState,
                    DestinationPostalCode = SPECIAL_INSTRUCTION_DATA.DestinationPostalCode,
                    DestinationCountry = SPECIAL_INSTRUCTION_DATA.DestinationCountry,
                    SpecialInstructionEquipment = new List<SpecialInstructionEquipmentEntity>
                    {
                        new SpecialInstructionEquipmentEntity
                        {
                            SpecialInstructionId = DUPE_SPECIAL_INSTRUCTIONS_ID,
                            EquipmentId = "Equipment"
                        }
                    }
                };
                SPECIAL_INSTRUCTION_EQUIPMENT = new List<SpecialInstructionEquipmentEntity>
                {
                    new SpecialInstructionEquipmentEntity
                    {
                        SpecialInstructionId = SPECIAL_INSTRUCTIONS_ID
                    }
                };
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>
                {
                    new SpecialInstructionEntity
                    {
                        SpecialInstructionId = 0,
                        CustomerId = CUSTOMER_ID,
                        Name = "Dummy for Create"
                    }
                };
            }
        }

        public class GetSpecialInstructionsForLoadAsyncTests : IClassFixture<TestFixture>
        {
            private Mock<LoadshopDataContext> _db;
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private readonly Mock<ISecurityService> _securityService;
            private readonly Mock<ICommonService> _commonService;
            private readonly IHtmlSanitizer _htmlSanitizer;

            private ISpecialInstructionsService _svc;

            private List<SpecialInstructionEntity> SPECIAL_INSTRUCTIONS;
            private LoadEntity LOAD;

            private static readonly Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
            private static readonly string EQUIPMENT_ID = "EquipmentId";

            public GetSpecialInstructionsForLoadAsyncTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser())
                    .Returns(new List<CustomerData>
                    {
                        new CustomerData
                        {
                            CustomerId = CUSTOMER_ID
                        }
                    });
                _commonService = new Mock<ICommonService>();
                _htmlSanitizer = new HtmlSanitizer();

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
                    .WithSpecialInstructions(SPECIAL_INSTRUCTIONS)
                    .Build();
            }

            private void InitService()
            {
                _svc = new SpecialInstructionsService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _htmlSanitizer);
            }

            [Fact]
            public void NullLoad_ThrowsException()
            {
                var expected = "Load cannot be null";
                _svc.Awaiting(x => x.GetSpecialInstructionsForLoadAsync(null))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public void NotAuthorizedForCustomer_ThrowsException()
            {
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser())
                    .Returns(new List<CustomerData>());
                InitService();

                var expected = $"User is not authorized for customer: {CUSTOMER_ID}";
                _svc.Awaiting(x => x.GetSpecialInstructionsForLoadAsync(LOAD))
                    .Should().Throw<Exception>()
                    .WithMessage(expected);
            }

            [Fact]
            public async Task NoOrigin_ReturnsEmptyList()
            {
                LOAD.LoadStops.RemoveAt(0);
                var actual = await _svc.GetSpecialInstructionsForLoadAsync(LOAD);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task NoDestination_ReturnsEmptyList()
            {
                LOAD.LoadStops.RemoveAt(1);
                var actual = await _svc.GetSpecialInstructionsForLoadAsync(LOAD);
                actual.Should().BeEmpty();
            }

            [Fact]
            public async Task ReturnsInstructions()
            {
                var actual = await _svc.GetSpecialInstructionsForLoadAsync(LOAD);
                actual.Should().NotBeEmpty();
            }

            private void InitSeedData()
            {
                SPECIAL_INSTRUCTIONS = new List<SpecialInstructionEntity>
                {
                    new SpecialInstructionEntity
                    {
                        CustomerId = CUSTOMER_ID,
                        Comments = "<p>some bad comments <script>alert('xss')</script></p>",
                        Name = "Name",
                        Description = "Special Instructions",
                        OriginCity = "Origin City",
                        OriginAddress1 = "123 Fake St.",
                        OriginState = "State",
                        OriginPostalCode = "12345",
                        OriginCountry = "USA",
                        DestinationCity = "Dest City",
                        DestinationAddress1 = "123 Fake St.",
                        DestinationState = "State",
                        DestinationPostalCode = "12345",
                        DestinationCountry = "USA",
                        SpecialInstructionEquipment = new List<SpecialInstructionEquipmentEntity>
                        {
                            new SpecialInstructionEquipmentEntity
                            {
                                EquipmentId = EQUIPMENT_ID
                            }
                        }
                    },
                    new SpecialInstructionEntity
                    {
                        CustomerId = CUSTOMER_ID,
                        Comments = "<p>some bad comments <script>alert('xss')</script></p>",
                        Name = "Name",
                        Description = "Special Instructions",
                        OriginCity = "Origin City",
                        OriginAddress1 = "123 Fake St.",
                        OriginState = "State",
                        OriginPostalCode = "12345",
                        OriginCountry = "USA",
                        DestinationCity = "Dest City",
                        DestinationAddress1 = "123 Fake St.",
                        DestinationState = "State",
                        DestinationPostalCode = "12345",
                        DestinationCountry = "USA",
                        SpecialInstructionEquipment = new List<SpecialInstructionEquipmentEntity>
                        {
                            new SpecialInstructionEquipmentEntity
                            {
                                EquipmentId = EQUIPMENT_ID
                            }
                        }
                    }
                };
                LOAD = new LoadEntity
                {
                    CustomerId = CUSTOMER_ID,
                    LoadStops = new List<LoadStopEntity>
                    {
                        new LoadStopEntity
                        {
                            StopNbr = 1,
                            Address1 = "123 Fake St.",
                            City = "Origin City",
                            State = "State",
                            PostalCode = "12345",
                            Country = "USA"
                        },
                        new LoadStopEntity
                        {
                            StopNbr = 2,
                            Address1 = "123 Fake St.",
                            City = "Dest City",
                            State = "State",
                            PostalCode = "12345",
                            Country = "USA"
                        }
                    },
                    EquipmentId = EQUIPMENT_ID
                };
            }
        }
    }
}
