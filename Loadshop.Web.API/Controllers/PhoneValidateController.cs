using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Loadshop.Web.API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    public class PhoneValidateController : BaseController
    {
        private readonly ISMSService _smsService;

        public PhoneValidateController(ISMSService smsService)
        {
            _smsService = smsService;
        }

        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        [HttpPost]
        public IActionResult Post([FromQuery]string phoneNumber)
        {
            return Success(_smsService.ValidateNumber(phoneNumber));
        }
        
    }
}