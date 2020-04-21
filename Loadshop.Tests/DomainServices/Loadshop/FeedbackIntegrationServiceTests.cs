using AutoMapper;
using FeedbackService.Models.V1;
using FeedbackService.SDK.V1;
using FluentAssertions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class FeedbackIntegrationServiceTests
    {
        public abstract class BaseFeedbackIntegrationServiceTests
        {
            protected readonly IMapper _mapper;
            protected readonly Mock<IUserContext> _userContext;
            protected MockDbBuilder _dbBuilder;
            protected Mock<LoadshopDataContext> _db;
            protected Mock<ISecurityService> _securityService;
            protected Mock<IFeedbackClient> _feedbackClient;
            protected Mock<IConfigurationRoot> _config;
            protected Mock<IConfigurationSection> _configSection;
            protected FeedbackIntegrationService _svc;

            protected static Guid VALID_USER_ID = Guid.Parse("99999999-9999-9999-9999-999999999999");
            protected static string VALID_USER_NAME = "valid user";
            protected static Guid INVALID_USER_ID = Guid.Parse("88888888-8888-8888-8888-888888888888");
            protected static Guid VALID_LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            protected static Guid INVALID_LOAD_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");
            protected static Guid UNBOOKED_LOAD_ID = Guid.Parse("33333333-3333-3333-3333-333333333333");
            protected static Guid VALID_CUSTOMER_ID = Guid.Parse("12345678-1234-1234-1234-123456789012");
            protected static Guid INVALID_CUSTOMER_ID = Guid.Parse("12345678-1234-1234-1234-000000000000");
            protected static string VALID_REF_LOAD_ID = "TEST-12345678";

            protected static UserEntity VALID_USER => new UserEntity
            {
                IdentUserId = VALID_USER_ID,
                Username = VALID_USER_NAME,
                PrimaryCustomerId = VALID_CUSTOMER_ID
            };

            protected static UserEntity INVALID_USER => new UserEntity
            {
                IdentUserId = INVALID_USER_ID,
                Username = "INVALID",
                PrimaryCustomerId = INVALID_CUSTOMER_ID
            };

            protected static LoadEntity VALID_LOAD => new LoadEntity
            {
                LoadId = VALID_LOAD_ID,
                CustomerId = VALID_CUSTOMER_ID,
                ReferenceLoadId = VALID_REF_LOAD_ID
            };

            protected List<LoadEntity> LOADS = new List<LoadEntity> { VALID_LOAD };

            protected BaseFeedbackIntegrationServiceTests()
            {
                _mapper = new TestFixture().Mapper;
                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(LOADS)
                    .WithUsers(new List<UserEntity> { VALID_USER, INVALID_USER })
                    .Build();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(_ => _.GetAuthorizedCustomersforUserAsync()).ReturnsAsync((new List<CustomerData>()
                {
                    new CustomerData
                    {
                        CustomerId = VALID_CUSTOMER_ID
                    }
                }).AsReadOnly());

                _feedbackClient = new Mock<IFeedbackClient>();

                _userContext = new Mock<IUserContext>();
                _userContext.SetupGet(_ => _.UserId).Returns(VALID_USER_ID);
                _userContext.SetupGet(_ => _.UserName).Returns(VALID_USER_NAME);

                _config = new Mock<IConfigurationRoot>();
                _configSection = new Mock<IConfigurationSection>();

                _svc = CreateService();
            }

            protected FeedbackIntegrationService CreateService()
            {
                return new FeedbackIntegrationService(_feedbackClient.Object, _config.Object, _mapper, _db.Object, _userContext.Object, _securityService.Object);
            }

            protected void AssertHasError(BaseServiceResponse response, string urn, string message)
            {
                response.ModelState.Should().Contain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
            }

            protected void AssertNoError(BaseServiceResponse response, string urn, string message)
            {
                response.ModelState.Should().NotContain(s => s.Key == urn && s.Value.Errors.Any(e => e.ErrorMessage == message));
            }
        }

        public class GetQuestionTests : BaseFeedbackIntegrationServiceTests
        {
            public GetQuestionTests() : base()
            {
            }

            [Fact]
            public async Task QuestionIdNotFound()
            {
                Func<Task> action = async () => await _svc.GetQuestionAsync(FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId);
                await action.Should().ThrowAsync<Exception>();
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            public async Task BadQuestionId(int questionId)
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns(questionId.ToString());

                var response = await _svc.GetQuestionAsync(FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId);
                response.Should().BeNull();
            }

            [Fact]
            public async Task CallsGetQuestionWithExpectedId()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                await _svc.GetQuestionAsync(FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId);
                _feedbackClient.Verify(_ => _.GetQuestionAsync(10));
            }

            [Fact]
            public async Task GetFeedbackClientThrowsException()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");
                _feedbackClient.Setup(_ => _.GetQuestionAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Testing Exception"));

                Func<Task> action = async () => await _svc.GetQuestionAsync(FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Testing Exception");
            }

            [Fact]
            public async Task GetFeedbackClientReturnsNull()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");
                _feedbackClient.Setup(_ => _.GetQuestionAsync(It.IsAny<int>())).ReturnsAsync((Question)null);

                var question = await _svc.GetQuestionAsync(FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId);
                question.Should().BeNull();
            }

            [Fact]
            public async Task ReturnsExpectedQuestion()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");
                _feedbackClient.Setup(_ => _.GetQuestionAsync(It.IsAny<int>())).ReturnsAsync(new Question()
                {
                    QuestionId = 99,
                    ApplicationCode = "AC",
                    Category = "CAT",
                    Description = "DESC",
                    IsActive = true,
                    QuestionText = "QT",
                    QuestionReasons = new List<QuestionReason>
                    {
                        new QuestionReason { QuestionReasonId = 1, Description = "QR Desc", DisplayedNegative = true, DisplayedPositive = true, IsActive = true, Reason = "QR Reason" },
                        new QuestionReason { QuestionReasonId = 5, Description = "QR Desc 5", DisplayedNegative = false, DisplayedPositive = true, IsActive = false, Reason = "QR Reason 5" }
                    },
                    UpdateUser = "UU"
                });

                var question = await _svc.GetQuestionAsync(FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId);
                question.Should().BeEquivalentTo(new QuestionData
                {
                    QuestionId = 99,
                    ApplicationCode = "AC",
                    Category = "CAT",
                    Description = "DESC",
                    IsActive = true,
                    QuestionText = "QT",
                    QuestionReasons = new List<QuestionReasonData>
                    {
                        new QuestionReasonData { QuestionReasonId = 1, Description = "QR Desc", DisplayedNegative = true, DisplayedPositive = true, IsActive = true, Reason = "QR Reason" },
                        new QuestionReasonData { QuestionReasonId = 5, Description = "QR Desc 5", DisplayedNegative = false, DisplayedPositive = true, IsActive = false, Reason = "QR Reason 5" }
                    },
                    UpdateUser = "UU"
                });
            }
        }

        public class GetResponseTests : BaseFeedbackIntegrationServiceTests
        {
            public GetResponseTests() : base()
            {
            }

            [Fact]
            public async Task QuestionIdNotFound()
            {
                Func<Task> action = async () => await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, VALID_LOAD_ID);
                await action.Should().ThrowAsync<Exception>();
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            public async Task InvalidQuestionId(int questionId)
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns(questionId.ToString());

                Func<Task> action = async () => await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, VALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Invalid question id");
            }

            [Fact]
            public async Task NullLoadId()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                Func<Task> action = async () => await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, null);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load Id is required");
            }

            [Fact]
            public async Task CallsSearchResponsesWithExpectedValues()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, VALID_LOAD_ID);
                _feedbackClient.Verify(_ => _.SearchResponsesAsync(It.Is<ResponseSearchCriteria>(x =>
                    x.QuestionId == 10
                    && x.Attributes["LoadshopLoadId"] == VALID_LOAD_ID.ToString()
                )));
            }

            [Fact]
            public async Task SearchResponsesThrowsException()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _feedbackClient.Setup(_ => _.SearchResponsesAsync(It.IsAny<ResponseSearchCriteria>())).ThrowsAsync(new Exception("Testing Exception"));

                Func<Task> action = async () => await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, VALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Testing Exception");
            }

            [Fact]
            public async Task MissingLoad()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _dbBuilder = new MockDbBuilder();
                _dbBuilder.WithUser(VALID_USER);
                _db = _dbBuilder.Build();
                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, VALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task UserNotFound()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(LOADS)
                    .Build();

                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, VALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task UnauthorizedUser()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _securityService.Setup(_ => _.GetAuthorizedCustomersforUserAsync()).ReturnsAsync((new List<CustomerData>()).AsReadOnly());

                _svc = CreateService();

                Func<Task> action = async () => await _svc.GetResponseAsync(
                    FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId, VALID_LOAD_ID);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }
        }

        public class SaveResponseTests : BaseFeedbackIntegrationServiceTests
        {
            public SaveResponseTests() : base()
            {
            }

            [Fact]
            public async Task QuestionIdNotFound()
            {
                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                Func<Task> action = async () => await _svc.SaveResponseAsync(data);
                await action.Should().ThrowAsync<Exception>();
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            public async Task InvalidQuestionId(int questionId)
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns(questionId.ToString());

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                Func<Task> action = async () => await _svc.SaveResponseAsync(data);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Invalid question id");
            }

            [Fact]
            public async Task NullResponseData()
            {
                Func<Task> action = async () => await _svc.SaveResponseAsync(null);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Response must be provided");
            }

            [Fact]
            public async Task NullLoadId()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = null
                };

                Func<Task> action = async () => await _svc.SaveResponseAsync(data);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load Id is required");
            }

            [Fact]
            public async Task CallsSearchResponsesWithExpectedValues()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                await _svc.SaveResponseAsync(data);
                _feedbackClient.Verify(_ => _.SearchResponsesAsync(It.Is<ResponseSearchCriteria>(x =>
                    x.QuestionId == 10
                    && x.Attributes["LoadshopLoadId"] == VALID_LOAD_ID.ToString()
                )));
            }

            [Fact]
            public async Task SearchResponsesThrowsException()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _feedbackClient.Setup(_ => _.SearchResponsesAsync(It.IsAny<ResponseSearchCriteria>())).ThrowsAsync(new Exception("Testing Exception"));

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                Func<Task> action = async () => await _svc.SaveResponseAsync(data);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Testing Exception");
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task CreatesNewResponse(bool answer)
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");
                _feedbackClient.Setup(_ => _.SaveResponseAsync(It.IsAny<Response>())).ReturnsAsync(new Response() { ResponseId = 15 });

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = answer,
                    LoadId = VALID_LOAD_ID
                };

                await _svc.SaveResponseAsync(data);

                _feedbackClient.Verify(_ => _.SaveResponseAsync(It.Is<Response>(x =>
                    x.QuestionId == 10
                    && x.Attributes["LoadshopLoadId"] == VALID_LOAD_ID.ToString()
                    && x.Answer == answer
                )));
            }

            [Fact]
            public async Task UpdatesExistingResponse()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");
                var latestResponse = new Response
                {
                    ResponseId = 15,
                    ResponseDateUtc = new DateTime(2020, 3, 18, 1, 0, 1),
                    Responder = "old user",
                    Answer = false,
                    QuestionId = 10,
                    Attributes = new Dictionary<string, string> { { "LoadshopLoadId", VALID_LOAD_ID.ToString() } }
                };
                _feedbackClient.Setup(_ => _.SearchResponsesAsync(It.IsAny<ResponseSearchCriteria>())).ReturnsAsync(
                    new List<Response>
                    {
                        new Response() { ResponseId = 1, ResponseDateUtc = new DateTime(2020, 3, 18, 1, 0, 0 )},
                        latestResponse,
                        new Response() { ResponseId = 2, ResponseDateUtc = new DateTime(2020, 3, 18, 1, 0, 0 )}
                    });
                _feedbackClient.Setup(_ => _.SaveResponseAsync(It.IsAny<Response>())).ReturnsAsync(new Response() { ResponseId = 15 });

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                await _svc.SaveResponseAsync(data);

                _feedbackClient.Verify(_ => _.SaveResponseAsync(It.Is<Response>(x =>
                    x.QuestionId == 10
                    && x.Attributes["LoadshopLoadId"] == VALID_LOAD_ID.ToString()
                    && x.Answer == true
                    && x.ResponseDateUtc != new DateTime(2020, 3, 18, 1, 0, 1)//date should be updated
                    && x.Responder == VALID_USER_NAME
                )));
            }

            [Fact]
            public async Task MissingLoad()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _dbBuilder = new MockDbBuilder();
                _dbBuilder.WithUser(VALID_USER);
                _db = _dbBuilder.Build();
                _svc = CreateService();

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                Func<Task> action = async () => await _svc.SaveResponseAsync(data);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task UserNotFound()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _dbBuilder = new MockDbBuilder();
                _db = _dbBuilder
                    .WithLoads(LOADS)
                    .Build();

                _svc = CreateService();

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                Func<Task> action = async () => await _svc.SaveResponseAsync(data);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }

            [Fact]
            public async Task UnauthorizedUser()
            {
                _config.Setup(_ => _.GetSection(It.Is<string>(s => s == FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId.ToString()))).Returns(_configSection.Object);
                _configSection.Setup(a => a.Value).Returns("10");

                _securityService.Setup(_ => _.GetAuthorizedCustomersforUserAsync()).ReturnsAsync((new List<CustomerData>()).AsReadOnly());

                _svc = CreateService();

                var data = new QuestionResponseData
                {
                    FeedbackQuestionCode = FeedbackQuestionCodeEnum.LB_ShipperReuseCarrierQuestionId,
                    Answer = true,
                    LoadId = VALID_LOAD_ID
                };

                Func<Task> action = async () => await _svc.SaveResponseAsync(data);
                (await action.Should().ThrowAsync<Exception>()).WithMessage("Load not found");
            }
        }
    }
}
