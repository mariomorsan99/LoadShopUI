using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.Web.API.Security;

namespace Loadshop.Web.API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Policy = AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    public class CustomerLoadTypeController : BaseController
    {
        private readonly ICustomerLoadTypeService _customerLoadTypeService;

        public CustomerLoadTypeController(ICustomerLoadTypeService customerLoadTypeService)
        {
            _customerLoadTypeService = customerLoadTypeService;
        }

        [ProducesResponseType(typeof(ResponseMessage<List<CustomerLoadTypeData>>), 200)]
        public IActionResult Get()
        {   
            return Success(_customerLoadTypeService.GetCustomerLoadTypes());
        }
    }
}