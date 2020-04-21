using System;
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
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [ProducesResponseType(typeof(ResponseMessage<CustomerData>), 200)]
        [Route("{customerId}")]
        public IActionResult Get(Guid customerId)
        {   
            return Success(_customerService.GetCustomer(customerId));
        }
    }
}