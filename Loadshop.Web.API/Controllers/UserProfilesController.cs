using System;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Exceptions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Proxy.Visibility.Interfaces;
using Loadshop.Web.API.Security;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    [Route("api/[controller]")]
    public class UserProfilesController : BaseController
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IUserContext _userContext;
        private readonly IVisibilityProxyService visibilityProxyService;
        private readonly ISecurityService securityService;

        public UserProfilesController(IUserProfileService userProfileService, IUserContext userContext, IVisibilityProxyService visibilityProxyService,
            ISecurityService securityService)
        {
            _userProfileService = userProfileService;
            _userContext = userContext;
            this.visibilityProxyService = visibilityProxyService;
            this.securityService = securityService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(ResponseMessage<UserProfileData>), 200)]
        public async Task<IActionResult> Get()
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("UserId is null");
                }

                var identUserId = _userContext.UserId.Value;
                _userProfileService.UpdateUserData(identUserId, _userContext.UserName, _userContext.FirstName, _userContext.LastName);

                return Success(await _userProfileService.GetUserProfileAsync(identUserId));
            }
            catch (EntityNotFoundException)
            {
                return Success(await _userProfileService.CreateUserProfileAsync(_userContext.UserId.Value, _userContext.Company, _userContext.Email, _userContext.UserName,
                    firstName: _userContext.FirstName, lastName: _userContext.LastName));
            }
            catch (Exception e)
            {
                throw e;
            }
        }
               
        [HttpPut("")]
        [ProducesResponseType(typeof(ResponseMessage<UserProfileData>), 200)]
        public async Task<IActionResult> Put([FromBody]UserProfileData user)
        {
            var response = await _userProfileService.SaveUserProfileAsync(user, _userContext.UserName);
            if (response.IsSuccess)
            {
                return Success(response.UserProfile);
            }
            else
            {
                var problemDetails = new ValidationProblemDetails(response.ModelState)
                {
                    Title = "Error updating User Profile",
                    Detail = "One or more errors occurred when updating the User Profile.  See form for error details",
                    Status = (int)HttpStatusCode.BadRequest,
                    Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                };
                return BadRequest(problemDetails);
            }

        }

        [HttpGet(nameof(GetAllMyAuthorizedEntities))]
        [ProducesResponseType(typeof(ResponseMessage<List<UserFocusEntityData>>), 200)]
        public async Task<IActionResult> GetAllMyAuthorizedEntities([FromServices] ISecurityService securityService)
        {
            try
            {
                return Success(await securityService.GetAllMyAuthorizedEntitiesAsync());
            }
            catch (Exception ex)
            {
                return Error<ResponseMessage<List<CarrierScacData>>>(ex);
            }
        }

        [HttpPut(nameof(PutFocusEntity))]
        [ProducesResponseType(typeof(ResponseMessage<UserProfileData>), 200)]
        public async Task<IActionResult> PutFocusEntity([FromBody] UserFocusEntityData userFocusEntityData)
        {
            try
            {

                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("UserId is null");
                }

                return Success(await _userProfileService.UpdateFocusEntityAsync(_userContext.UserId.Value, userFocusEntityData, _userContext.UserName));
            }
            catch (Exception ex)
            {
                return Error<ResponseMessage<List<CarrierScacData>>>(ex);
            }
        }



        [HttpGet("loadstatusnotifications")]
        [ProducesResponseType(typeof(ResponseMessage<LoadStatusNotificationsData>), 200)]
        [Authorize(Policy = AuthorizationConstants.IsShipperPolicy)]
        public async Task<IActionResult> GetLoadStatusNotifications()
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("UserId is null");
                }

                await securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_Profile_LoadStatusNotifications);

                var response = await visibilityProxyService.GetLoadStatusNotificationsAsync();

                return Success(response);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPut("loadstatusnotifications")]
        [ProducesResponseType(typeof(ResponseMessage<LoadStatusNotificationsData>), 200)]
        [Authorize(Policy = AuthorizationConstants.IsShipperPolicy)]
        public async Task<IActionResult> UpdateLoadStatusNotifications([FromBody]LoadStatusNotificationsData notificationsData)
        {
            await securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_Profile_LoadStatusNotifications);

            var response = await visibilityProxyService.UpdateLoadStatusNotificationsAsync(notificationsData);
            if (response.Successful)
            {
                return Success(notificationsData);
            }
            else
            {
                var problemDetails = new ValidationProblemDetails(response.ModelState)
                {
                    Title = "Error updating Load status notifications",
                    Detail = "One or more errors occurred when updating the load status notifications.  See form for error details",
                    Status = (int)HttpStatusCode.BadRequest,
                    Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                };
                return BadRequest(problemDetails);
            }

        }
    }
}
