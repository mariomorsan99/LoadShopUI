using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Loadshop.Customer.API.Models.Commodity;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CommodityController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ICommodityService _commodityService;
        private readonly IUserContext _user;

        public CommodityController(IMapper mapper, ICommodityService commodityService, IUserContext userContext)
        {
            _mapper = mapper;
            _commodityService = commodityService;
            _user = userContext;
        }

        [HttpGet]
        [Route("GetCommodities")]
        [ProducesResponseType(typeof(ResponseMessage<List<CommodityViewModel>>), 200)]
        public IActionResult GetCommodities()
        {
            try
            {
                return Success(_mapper.Map<List<CommodityViewModel>>(_commodityService.GetCommodities()));
            }
            catch (ValidationException e)
            {
                return Error<List<CommodityViewModel>>(e);
            }
        }
    }
}
