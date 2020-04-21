using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Loadshop.Web.API.Security;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = AuthorizationConstants.IsShipperPolicy)]
    [Route("api/ratings")]
    [ApiController]
    public class RatingController : BaseController
    {
        private readonly IRatingService ratingService;
        private readonly IUserContext userContext;

        public RatingController(
            IRatingService ratingService,
            IUserContext userContext)
        {
            this.ratingService = ratingService;
            this.userContext = userContext;
        }

        [HttpGet("questions")]
        [ProducesResponseType(typeof(ResponseMessage<List<RatingQuestionData>>), 200)]
        public async Task<IActionResult> Get()
        {
            return Success(await ratingService.GetRatingQuestions());
        }
    }
}