using System;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.Web.API.Security;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    [Route("api/feedback")]
    [ApiController]
    public class FeedbackController : BaseController
    {
        private readonly IFeedbackIntegrationService _feedbackService;

        public FeedbackController(IFeedbackIntegrationService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet("{feedbackQuestionCode}")]
        [ProducesResponseType(typeof(ResponseMessage<QuestionData>), 200)]
        public async Task<IActionResult> Get(FeedbackQuestionCodeEnum feedbackQuestionCode)
        {
            return Success(await _feedbackService.GetQuestionAsync(feedbackQuestionCode));
        }

        [HttpGet("{feedbackQuestionCode}/{loadId}")]
        [ProducesResponseType(typeof(ResponseMessage<QuestionData>), 200)]
        public async Task<IActionResult> GetResponse(FeedbackQuestionCodeEnum feedbackQuestionCode, Guid? loadId)
        {
            return Success(await _feedbackService.GetResponseAsync(feedbackQuestionCode, loadId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<QuestionData>), 200)]
        public async Task<IActionResult> Post([FromBody] QuestionResponseData data)
        {
            return Success(await _feedbackService.SaveResponseAsync(data));
        }
    }
}