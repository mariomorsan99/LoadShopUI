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
    public class CommodityController : BaseController
    {
        private readonly ICommodityService _commodityService;

        public CommodityController(ICommodityService commodityService)
        {
            _commodityService = commodityService;
        }

        [ProducesResponseType(typeof(ResponseMessage<List<CommodityData>>), 200)]
        public IActionResult Get()
        {   
            return Success(_commodityService.GetCommodities());
        }
    }
}