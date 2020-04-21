using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Common.Services.Data;
using Loadshop.DomainServices.Security;
using Microsoft.AspNetCore.Authorization;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    [Route("api/[controller]")]
    public class StatesController : BaseController
    {
        private readonly ICommonService _commonService;
        private readonly IUserContext _userContext;

        public StatesController(ICommonService commonService, IUserContext userContext)
        {
            _commonService = commonService;
            _userContext = userContext;
        }

        [ProducesResponseType(typeof(ResponseMessage<List<StateData>>), 200)]
        public IActionResult Get()
        {
            return Success(_commonService.GetUSCANStateProvinces());
        }
    }
}
