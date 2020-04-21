using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Loadshop.Web.API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    public class EquipmentController : BaseController
    {
        private readonly IEquipmentService _commonService;
        private readonly IUserContext _userContext;

        public EquipmentController(IEquipmentService commonService, IUserContext userContext)
        {
            _commonService = commonService;
            _userContext = userContext;
        }

        [ProducesResponseType(typeof(ResponseMessage<List<Equipment>>), 200)]
        public IActionResult Get()
        {   
            return Success(_commonService.GetEquipment());
        }
    }
}