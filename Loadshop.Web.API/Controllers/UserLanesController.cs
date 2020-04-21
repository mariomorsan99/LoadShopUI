using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Exceptions;
using Loadshop.Web.API.Security;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    [Route("api/[controller]")]
    public class UserLanesController : BaseController
    {
        private readonly IUserLaneService _userLaneService;
        private readonly IUserContext _userContext;

        public UserLanesController(IUserLaneService userLaneService, IUserContext userContext)
        {
            _userLaneService = userLaneService;
            _userContext = userContext;
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Profile_Favorites_View)]
        [ProducesResponseType(typeof(ResponseMessage<List<UserLaneData>>), 200)]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Success(await _userLaneService.GetSavedLanesAsync(_userContext.UserId.Value));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<ResponseMessage<List<UserLaneData>>>(ex);
            }
        }

        [HttpPut]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Profile_Favorites_Add_Edit)]
        [ProducesResponseType(typeof(ResponseMessage<UserLaneData>), 200)]
        public async Task<IActionResult> Put([FromBody]UserLaneData lane)
        {
            try
            {
                return Success(await _userLaneService.UpdateLaneAsync(lane, _userContext.UserId.Value, _userContext.UserName));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<ResponseMessage<List<UserLaneData>>>(ex);
            }
            catch (ValidationException valEx)
            {
                return Error<UserLaneData>(valEx);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Profile_Favorites_Add_Edit)]
        [ProducesResponseType(typeof(ResponseMessage<UserLaneData>), 200)]
        public async Task<IActionResult> Post([FromBody]UserLaneData lane)
        {
            try
            {
                return Success(await _userLaneService.CreateLaneAsync(lane, _userContext.UserId.Value, _userContext.UserName));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<ResponseMessage<List<UserLaneData>>>(ex);
            }
            catch (ValidationException valEx)
            {
                return Error<UserLaneData>(valEx);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpDelete]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Profile_Favorites_Add_Edit)]
        [ProducesResponseType(typeof(ResponseMessage<object>), 200)]
        public async Task<IActionResult> Delete([FromQuery]Guid id)
        {
            try
            {
                await _userLaneService.DeleteLaneAsync(id);
                return Success<object>(null);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<ResponseMessage<List<UserLaneData>>>(ex);
            }
        }
    }
}
