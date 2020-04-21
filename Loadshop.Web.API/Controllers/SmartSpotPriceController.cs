using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    public class SmartSpotPriceController : BaseController
    {
        private readonly ISmartSpotPriceService _svc;

        public SmartSpotPriceController(ISmartSpotPriceService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<List<SmartSpotPrice>>), 200)]
        public async Task<IActionResult> PostAsync([FromBody]List<LoadshopSmartSpotPriceRequest> requests)
        {
            var prices = await _svc.GetSmartSpotPricesAsync(requests);
            return Success(prices);
        }

        [HttpPost]
        [Route("quote")]
        [Authorize(Policy = Security.AuthorizationConstants.ActionPolicyPrefix + DomainServices.Security.SecurityActions.Loadshop_Ui_Shopit_SmartSpotPrice_Quote)]
        [ProducesResponseType(typeof(ResponseMessage<decimal>), 200)]
        public async Task<IActionResult> QuoteAsync([FromBody] RecaptchaRequest<LoadshopSmartSpotQuoteRequest> request)
        {
            var response = await _svc.GetSmartSpotQuoteAsync(request);
            if (!response.IsSuccess)
            {
                var problemDetails = new ValidationProblemDetails(response.ModelState)
                {
                    Title = "Send \"In Transit\" Status",
                    Detail = "One or more errors occurred when trying to retrieve the Smart Spot Quote.  See form for error details",
                    Status = (int)HttpStatusCode.BadRequest,
                    Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                };
                return BadRequest(problemDetails);
            }
            return Success(response.Data);
        }
    }
}
