using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Dto;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loadshop.Web.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = Security.AuthorizationConstants.HasAnyLoadShopRolePolicy)]
    public class UserCommunicationController : BaseCrudController<UserCommunicationDetailData>
    {
        private readonly IUserCommunicationService _userCommunicationService;
        private readonly IUserContext _userContext;

        public UserCommunicationController(IUserCommunicationService userCommunicationService, IUserContext userContext) : base(userCommunicationService)
        {
            _userCommunicationService = userCommunicationService;
            _userContext = userContext;
        }

        [HttpGet]
        [Authorize(Policy = Security.AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_UserCommunication_Add_Edit)]
        [ProducesResponseType(typeof(ResponseMessage<List<UserCommunicationData>>), 200)]
        public async Task<IActionResult> Get()
        {
            return await base.GetCollection<UserCommunicationData>();
        }

        [HttpGet]
        [Authorize(Policy = Security.AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_UserCommunication_Add_Edit)]
        [Route("{userCommunicationId}")]
        [ProducesResponseType(typeof(ResponseMessage<UserCommunicationDetailData>), 200)]
        public async Task<IActionResult> Get([FromRoute] Guid userCommunicationId)
        {
            return await base.GetByKey(userCommunicationId);
        }

        [HttpPut]
        [Authorize(Policy = Security.AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_UserCommunication_Add_Edit)]
        [ProducesResponseType(typeof(ResponseMessage<UserCommunicationDetailData>), 200)]
        public async Task<IActionResult> UpdateUserCommunication([FromBody] UserCommunicationDetailData userCommunicationData)
        {
            return await base.Update(userCommunicationData, true, userCommunicationData.UserCommunicationId);
        }

        [HttpPost]
        [Authorize(Policy = Security.AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_UserCommunication_Add_Edit)]
        [ProducesResponseType(typeof(ResponseMessage<UserCommunicationDetailData>), 200)]
        public async Task<IActionResult> CreateUserCommunication([FromBody] UserCommunicationDetailData userCommunicationData)
        {
            return await base.Create(userCommunicationData);
        }

        [HttpDelete]
        [Authorize(Policy = Security.AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_UserCommunication_Add_Edit)]
        [Route("{userCommunicationId}")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public async Task<IActionResult> Delete([FromRoute] Guid userCommunicationId)
        {
            return await base.Delete(userCommunicationId);
        }

        [HttpGet(nameof(GetUserCommunicationsForDisplay))]
        [ProducesResponseType(typeof(ResponseMessage<List<UserCommunicationData>>), 200)]
        public async Task<IActionResult> GetUserCommunicationsForDisplay()
        {
            if (_userContext.UserId.HasValue)
            {
                return await ResultHelper(async () =>
                {
                    return await _userCommunicationService.GetUserCommunicationsForDisplay(_userContext.UserId.Value);
                });
            }

            return Error<List<UserCommunicationData>>("Invalid User. User must be logged in");
        }

        [HttpPost(nameof(Acknowledge))]
        [ProducesResponseType(typeof(ResponseMessage<List<UserCommunicationData>>), 200)]
        public async Task<IActionResult> Acknowledge([FromBody]AcknowledgeUserCommunication acknowledgeUserCommunication)
        {
            if (_userContext.UserId.HasValue)
            {
                return await ResultHelper(async () =>
                {
                    return await _userCommunicationService.Acknowledge(_userContext.UserId.Value, acknowledgeUserCommunication);
                });
            }

            return Error<List<UserCommunicationData>>("Invalid User. User must be logged in");
        }
    }
}
