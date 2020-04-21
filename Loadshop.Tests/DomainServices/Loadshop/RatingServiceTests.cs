using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class RatingServiceTests
    {
        public class GetRatingQuestionTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;

            private RatingService _svc;
            private static Guid VALID_QUESTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid VALID_LOAD_ID = Guid.Parse("11111111-2222-1111-1111-111111111111");
            private static Guid INVALID_QUESTION_ID = Guid.Parse("22222222-2222-2222-2222-222222222222");

            private List<RatingQuestionEntity> RATINGQUESTIONS = new List<RatingQuestionEntity>()
            {
                new RatingQuestionEntity()
                {
                    RatingQuestionId = VALID_QUESTION_ID,
                    Question="Remove due to Shipper",
                    DisplayText="shipper"
                }
            };

            public GetRatingQuestionTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder()
                    .WithRatingQuestions(RATINGQUESTIONS)
                    .Build();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });

                _commonService = new Mock<ICommonService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);

            }

            [Fact]
            public void RatingQuestion_Should_ThrowNotFound()
            {
                _svc = CreateService();

                _svc.Awaiting(x => x.GetRatingQuestion(INVALID_QUESTION_ID)).Should()
                    .Throw<Exception>()
                    .WithMessage("Question was not found with id");
            }

            [Fact]
            public async Task RatingQuestion_Should_ReturnQuestion()
            {
                _svc = CreateService();

                var question = await _svc.GetRatingQuestion(VALID_QUESTION_ID);

                question.Should().NotBeNull();
                question.RatingQuestionId.Should().Be(VALID_QUESTION_ID);
                question.Question.Should().Be("Remove due to Shipper");
            }

            [Fact]
            public async Task RatingQuestions_Should_ReturnQuestions()
            {
                _svc = CreateService();

                var questions = await _svc.GetRatingQuestions();

                questions.Should().NotBeNull();
                questions.Count.Should().Be(1);
            }

            [Fact]
            public void GetLatestRatingQuestionAnswer_Should_ThrowNotFound()
            {
                _svc = CreateService();

                _svc.Awaiting(x => x.GetLatestRatingQuestionAnswer(Guid.Empty)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Question was not found with id");
            }


            [Fact]
            public async Task GetLatestRatingQuestionAnswer_Should_ReturnAnswer()
            {
                var questionId = Guid.NewGuid();
                var answers = new List<RatingQuestionAnswerEntity>()
                {
                    new RatingQuestionAnswerEntity()
                    {
                        LoadId = VALID_LOAD_ID,
                        AnswerYN = true,
                        AdditionalComment = "test",
                        RatingQuestionId = questionId,
                        RatingQuestion = new RatingQuestionEntity()
                        {
                            RatingQuestionId = questionId,
                            DisplayText ="carrier"
                        }
                    }
                };

                _db = new MockDbBuilder()
                        .WithRatingQuestionAnswers(answers)
                       .Build();
                _svc = CreateService();

                var question = await _svc.GetLatestRatingQuestionAnswer(VALID_LOAD_ID);

                question.Should().NotBeNull();
                question.RatingQuestionId.Should().Be(questionId);
                question.RatingQuestion.DisplayText.Should().Be("carrier");
            }

            private RatingService CreateService()
            {
                return new RatingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _serviceUtilities);
            }
        }

        public class AddRatingQuestionTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;

            private RatingService _svc;
            private static Guid VALID_QUESTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");

            private List<RatingQuestionEntity> RATINGQUESTIONS = new List<RatingQuestionEntity>()
            {
                new RatingQuestionEntity()
                {
                    RatingQuestionId = VALID_QUESTION_ID,
                    Question="Remove due to Shipper",
                    DisplayText="shipper"
                }
            };

            public AddRatingQuestionTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder()
                    .WithRatingQuestions(RATINGQUESTIONS)
                    .Build();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });

                _commonService = new Mock<ICommonService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public void AddRatingQuestion_Should_ThrowAnswerRequired()
            {
                _svc = CreateService();

                _svc.Awaiting(x => x.AddRatingQuestionAnswer(new RatingQuestionAnswerData())).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Answer must have a question");
            }

            [Fact]
            public void AddRatingQuestion_Should_ThrowLoadRequired()
            {
                _svc = CreateService();

                var add = new RatingQuestionAnswerData()
                {
                    RatingQuestionId = VALID_QUESTION_ID
                };

                _svc.Awaiting(x => x.AddRatingQuestionAnswer(add)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Answer must have a load attached");
            }

            [Fact]
            public void AddRatingQuestion_Should_ThrowLoadClaimRequired()
            {
                _svc = CreateService();

                var add = new RatingQuestionAnswerData()
                {
                    RatingQuestionId = VALID_QUESTION_ID,
                    LoadId = Guid.NewGuid()
                };

                _svc.Awaiting(x => x.AddRatingQuestionAnswer(add)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Answer must be tied to a load claim");
            }

            [Fact]
            public async Task AddRatingQuestion_Should_Save()
            {
                _svc = CreateService();

                var add = new RatingQuestionAnswerData()
                {
                    RatingQuestionId = VALID_QUESTION_ID,
                    LoadId = Guid.NewGuid(),
                    LoadClaimId = Guid.NewGuid()
                };

                await _svc.AddRatingQuestionAnswer(add, true);
                //string userId, System.Threading.CancellationToken
                _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task AddRatingQuestion_Should_NotSave()
            {
                _svc = CreateService();

                var add = new RatingQuestionAnswerData()
                {
                    RatingQuestionId = VALID_QUESTION_ID,
                    LoadId = Guid.NewGuid(),
                    LoadClaimId = Guid.NewGuid()
                };

                await _svc.AddRatingQuestionAnswer(add);
                //string userId, System.Threading.CancellationToken
                _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            }

            private RatingService CreateService()
            {
                return new RatingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _serviceUtilities);
            }
        }

        public class GetRatingReasonTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private readonly Mock<IUserContext> _userContext;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private Mock<ICommonService> _commonService;
            private Mock<IDateTimeProvider> _dateTime;
            private ServiceUtilities _serviceUtilities;

            private RatingService _svc;
            private static Guid VALID_QUESTION_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
            private static Guid VALID_LOAD_ID = Guid.Parse("11111111-2222-1111-1111-111111111111");

            private List<RatingQuestionEntity> RATINGQUESTIONS = new List<RatingQuestionEntity>()
            {
                new RatingQuestionEntity()
                {
                    RatingQuestionId = VALID_QUESTION_ID,
                    Question="Remove due to Shipper",
                    DisplayText="shipper"
                }
            };

            public GetRatingReasonTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _userContext = new Mock<IUserContext>();
                _db = new MockDbBuilder()
                    .WithRatingQuestions(RATINGQUESTIONS)
                    .Build();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });

                _commonService = new Mock<ICommonService>();
                _dateTime = new Mock<IDateTimeProvider>();
                _serviceUtilities = new ServiceUtilities(_dateTime.Object);
            }

            [Fact]
            public async Task RatingReason_NotFound()
            {
                _svc = CreateService();

                (await _svc.GetRatingReason(Guid.Empty)).Should().BeEmpty();
            }

            [Fact]
            public async Task RatingReason_Success()
            {
                var questionId = Guid.NewGuid();
                var answers = new List<RatingQuestionAnswerEntity>()
                {
                    new RatingQuestionAnswerEntity()
                    {
                        LoadId = VALID_LOAD_ID,
                        AnswerYN = true,
                        AdditionalComment = "test",
                        RatingQuestionId = questionId,
                        RatingQuestion = new RatingQuestionEntity()
                        {
                            RatingQuestionId = questionId,
                            DisplayText ="carrier"
                        }
                    }
                };

                _db = new MockDbBuilder()
                        .WithRatingQuestionAnswers(answers)
                       .Build();
                _svc = CreateService();

                (await _svc.GetRatingReason(VALID_LOAD_ID)).Should().Be("carrier");
            }

            private RatingService CreateService()
            {
                return new RatingService(_db.Object, _mapper, _userContext.Object, _securityService.Object, _commonService.Object, _serviceUtilities);
            }
        }
    }
}
