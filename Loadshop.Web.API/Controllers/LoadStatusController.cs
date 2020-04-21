using System;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.API.Security;
using Loadshop.API.Models;
using System.Net;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthorizationConstants.CanAccessLoadStatus)]
    public class LoadStatusController : BaseController
    {
        private readonly ILoadStatusTransactionService _loadStatusTransactionService;

        public LoadStatusController(ILoadStatusTransactionService loadStatusTransactionService)
        {
            _loadStatusTransactionService = loadStatusTransactionService;
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(ResponseMessage<LoadStatusTransactionData>), 200)]
        public async Task<IActionResult> GetLatest([FromQuery] Guid loadId)
        {
            var data = await _loadStatusTransactionService.GetLatestStatus(loadId);
            return Success(data);
        }

        [HttpPost("inTransit")]
        [ProducesResponseType(typeof(ResponseMessage<LoadStatusTransactionData>), 200)]
        public async Task<IActionResult> PostInTransitStatus([FromBody] LoadStatusInTransitData inTransitStatus)
        {
            var response = await _loadStatusTransactionService.AddInTransitStatus(inTransitStatus);
            if (!response.IsSuccess)
            {
                var problemDetails = new ValidationProblemDetails(response.ModelState)
                {
                    Title = "Send \"In Transit\" Status",
                    Detail = "One or more errors occurred when trying to update the In Transit status of the load.  See form for error details",
                    Status = (int)HttpStatusCode.BadRequest,
                    Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                };
                return BadRequest(problemDetails);
            }

            var data = await _loadStatusTransactionService.GetLatestStatus(inTransitStatus.LoadId);
            return Success(data);
        }

        [HttpPost("stopStatuses")]
        [ProducesResponseType(typeof(ResponseMessage<LoadStatusTransactionData>), 200)]
        public async Task<IActionResult> PostStopEventStatus([FromBody] LoadStatusStopData stopData)
        {
            var response = await _loadStatusTransactionService.AddStopStatuses(stopData);
            if (!response.IsSuccess)
            {
                var problemDetails = new ValidationProblemDetails(response.ModelState)
                {
                    Title = "Send Stop Statuses",
                    Detail = "One or more errors occurred when trying to update the Stop statuses of the load.  See form for error details",
                    Status = (int)HttpStatusCode.BadRequest,
                    Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                };
                return BadRequest(problemDetails);
            }

            var data = await _loadStatusTransactionService.GetLatestStatus(stopData.LoadId);
            return Success(data);
        }
    }
}