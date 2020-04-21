using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    [Route("api/[controller]")]
    public class SecurityController : BaseController
    {
        ISecurityService _securityService;

        public SecurityController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        [HttpGet]
        [Route("api/[controller]/[action]/{carrierId}")]
        [ProducesResponseType(typeof(ResponseMessage<IEnumerable<CarrierScacData>>), 200)]
        public async Task<IActionResult> GetAuthorizedCarrierScacs([FromRoute]string carrierId)
        {
            try
            {
                var authorizedCarrierScacs = await _securityService.GetCurrentUserAuthorizedScacsForCarrierAsync(carrierId);

                return Success(authorizedCarrierScacs);
            }
            catch (Exception ex)
            {
                return Error<IEnumerable<CarrierScacData>>(ex);
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]/{carrierId}")]
        [ProducesResponseType(typeof(ResponseMessage<IEnumerable<CarrierScacData>>), 200)]
        public async Task<IActionResult> GetAuthorizedShippers()
        {
            try
            {
                var authorizedShippers = await _securityService.GetAuthorizedCustomersforUserAsync();

                return Success(authorizedShippers);
            }
            catch (Exception ex)
            {
                return Error<IEnumerable<CustomerData>>(ex);
            }
        }
    }
}
