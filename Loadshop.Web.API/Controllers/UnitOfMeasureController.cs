using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Mvc;
using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Loadshop.Web.API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    public class UnitOfMeasureController : BaseController
    {
        private readonly IUnitOfMeasureService _svc;

        public UnitOfMeasureController(IUnitOfMeasureService svc)
        {
            _svc = svc;
        }

        [ProducesResponseType(typeof(ResponseMessage<List<UnitOfMeasure>>), 200)]
        public IActionResult Get()
        {   
            return Success(_svc.GetUnitOfMeasures());
        }
    }
}