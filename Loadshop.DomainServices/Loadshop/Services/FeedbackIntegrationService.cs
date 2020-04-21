using AutoMapper;
using FeedbackService.Models.V1;
using FeedbackService.SDK.V1;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class FeedbackIntegrationService : IFeedbackIntegrationService
    {
        private readonly IFeedbackClient _client;
        private readonly IConfigurationRoot _config;
        private readonly IMapper _mapper;
        private readonly LoadshopDataContext _context;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;

        private const string LOADSHOP_LOAD_ID = "LoadshopLoadId";

        public FeedbackIntegrationService(IFeedbackClient client, IConfigurationRoot config, IMapper mapper,
            LoadshopDataContext context, IUserContext userContext, ISecurityService securityService)
        {
            _client = client;
            _config = config;
            _mapper = mapper;
            _context = context;
            _userContext = userContext;
            _securityService = securityService;
        }

        public async Task<QuestionData> GetQuestionAsync(FeedbackQuestionCodeEnum feedbackQuestionCode)
        {
            var questionId = _config.GetValue<int>(feedbackQuestionCode.ToString());
            if (questionId > 0)
            {
                var question = await _client.GetQuestionAsync(questionId);
                var data = _mapper.Map<QuestionData>(question);
                if (data != null)
                    data.FeedbackQuestionCode = feedbackQuestionCode;
                return data;
            }
            return null;
        }

        public async Task<QuestionResponseData> GetResponseAsync(FeedbackQuestionCodeEnum feedbackQuestionCode, Guid? loadId)
        {
            var (questionId, response) = await GetLatestResponseAsync(feedbackQuestionCode, loadId);
            if (response == null)
                return null;

            string loadshopLoadId = null;
            if (response.Attributes?.TryGetValue(LOADSHOP_LOAD_ID, out loadshopLoadId) != true)
                throw new Exception($"Feedback response did not return the {LOADSHOP_LOAD_ID}");

            return new QuestionResponseData
            {
                FeedbackQuestionCode = feedbackQuestionCode,
                Answer = response.Answer,
                LoadId = new Guid(loadshopLoadId)
            };
        }

        public async Task<QuestionResponseData> SaveResponseAsync(QuestionResponseData data)
        {
            if (data == null)
                throw new ValidationException("Response must be provided");

            var (questionId, response) = await GetLatestResponseAsync(data.FeedbackQuestionCode, data.LoadId);
            if (response == null)
            {
                response = new Response
                {
                    QuestionId = questionId,
                    Attributes = new Dictionary<string, string> { { LOADSHOP_LOAD_ID, data.LoadId.ToString() } }
                };
            }
            response.Answer = data.Answer;
            response.ResponseDateUtc = DateTime.UtcNow;
            response.Responder = _userContext.UserName;
            response = await _client.SaveResponseAsync(response);

            return data;
        }

        private async Task<(int, Response)> GetLatestResponseAsync(FeedbackQuestionCodeEnum feedbackQuestionCode, Guid? loadId)
        {
            var questionId = _config.GetValue<int>(feedbackQuestionCode.ToString());
            if (questionId <= 0)
                throw new ValidationException("Invalid question id");

            if (loadId == null)
                throw new ValidationException("Load Id is required");

            //verify the user has access to this load
            var load = await GetLoad(loadId.Value);

            var responses = await _client.SearchResponsesAsync(new ResponseSearchCriteria
            {
                QuestionId = questionId,
                Attributes = new Dictionary<string, string>
                {
                    { "LoadshopLoadId", loadId.ToString() }
                }
            });

            return (questionId, responses?.OrderByDescending(_ => _.ResponseDateUtc).FirstOrDefault());
        }

        private async Task<LoadEntity> GetLoad(Guid loadId)
        {
            var load = await _context.Loads.SingleOrDefaultAsync(x => x.LoadId == loadId);
            if (load == null)
                throw new ValidationException("Load not found");

            var user = await _context.Users.FirstOrDefaultAsync(_ => _.IdentUserId == _userContext.UserId);
            if (user == null)
                throw new ValidationException("Load not found");

            if (!(await _securityService.IsAuthorizedForCustomerAsync(load.CustomerId)))
                throw new ValidationException("Load not found");

            return load;
        }
    }
}
