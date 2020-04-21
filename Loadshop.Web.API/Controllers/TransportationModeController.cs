using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    public class TransportationModeController : BaseController
    {
        private readonly ITransportationService _service;

        public TransportationModeController(ITransportationService service)
        {
            _service = service;
        }

        [ProducesResponseType(typeof(ResponseMessage<List<TransportationModeData>>), 200)]
        public IActionResult Get()
        {
            return Success(_service.GetTransportationModes());
        }
    }
}
