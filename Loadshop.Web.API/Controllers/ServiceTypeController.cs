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
    public class ServiceTypeController : BaseController
    {
        private readonly IServiceTypeService _service;

        public ServiceTypeController(IServiceTypeService service)
        {
            _service = service;
        }

        [ProducesResponseType(typeof(ResponseMessage<List<ServiceTypeData>>), 200)]
        public IActionResult Get()
        {
            return Success(_service.GetServiceTypes());
        }
    }
}
