using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Loadshop.DomainServices.Security;
using Loadshop.Web.API.Security;
using System.Net;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = AuthorizationConstants.IsShipperPolicy)]
    [Route("api/[controller]")]
    public class LoadCarrierGroupController : BaseController
    {
        private readonly ILoadCarrierGroupService _loadCarrierGroupService;
        private readonly IUserContext _userContext;

        public LoadCarrierGroupController(ILoadCarrierGroupService loadCarrierGroupService, IUserContext userContext)
        {
            _loadCarrierGroupService = loadCarrierGroupService;
            _userContext = userContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadCarrierGroupDetailData>>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit)]

        public IActionResult Get()
        {
            return Success(_loadCarrierGroupService.GetLoadCarrierGroups());
        }

        [HttpGet]
        [Route("load/{loadId}")]
        [ProducesResponseType(typeof(ResponseMessage<IList<ShippingLoadCarrierGroupData>>), 200)]
        public IActionResult GetLoadCarrierGroupsForLoadId(Guid loadId)
        {
            try
            {
                return Success(_loadCarrierGroupService.GetLoadCarrierGroupsForLoad(loadId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<IList<ShippingLoadCarrierGroupData>>(ex);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ResponseMessage<LoadCarrierGroupDetailData>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit)]
        public IActionResult GetGroup(long id)
        {
            try
            {
                return Success(_loadCarrierGroupService.GetLoadCarrierGroup(id));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<LoadCarrierGroupDetailData>(ex);
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ResponseMessage<LoadCarrierGroupDetailData>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit)]
        public IActionResult Put([FromBody]LoadCarrierGroupDetailData group)
        {
            try
            {
                var response = _loadCarrierGroupService.UpdateLoadCarrierGroup(group, _userContext.UserName);
                if (response.IsSuccess)
                {
                    return Success(response.LoadCarrierGroupData);
                }
                else
                {
                    var problemDetails = new ValidationProblemDetails(response.ModelState)
                    {
                        Title = "Error updating LoadCarrierGroup",
                        Detail = "One or more errors occurred when updating the LoadCarrierGroup.  See form for error details",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                    };
                    return BadRequest(problemDetails);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<LoadCarrierGroupDetailData>(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<LoadCarrierGroupDetailData>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit)]
        public IActionResult Post([FromBody]LoadCarrierGroupDetailData group)
        {
            try
            {
                var response = _loadCarrierGroupService.CreateLoadCarrierGroup(group, _userContext.UserName);
                if (response.IsSuccess)
                {
                    return Success(response.LoadCarrierGroupData);
                }
                else
                {
                    var problemDetails = new ValidationProblemDetails(response.ModelState)
                    {
                        Title = "Error creating LoadCarrierGroup",
                        Detail = "One or more errors occurred when creating the LoadCarrierGroup.  See form for error details",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                    };
                    return BadRequest(problemDetails);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<LoadCarrierGroupDetailData>(ex);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(typeof(ResponseMessage<object>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit)]
        public IActionResult Delete(long id)
        {
            try
            {
                _loadCarrierGroupService.DeleteLoadCarrierGroup(id);
                return Success<object>(null);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<object>(ex);
            }
        }

        [HttpGet]
        [Route("{loadCarrierGroupId}/carrier")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadCarrierGroupCarrierData>>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit)]
        public IActionResult Get(long loadCarrierGroupId)
        {
            try
            {
                return Success(_loadCarrierGroupService.GetLoadCarrierGroupCarriers(loadCarrierGroupId));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<List<LoadCarrierGroupCarrierData>>(ex);
            }
        }

        [HttpDelete]
        [Route("{loadCarrierGroupId}/carrier")]
        [ProducesResponseType(typeof(ResponseMessage<object>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_System_Shipper_Carrier_Groups_Add_Edit)]
        public IActionResult DeleteAllCarriers(long loadCarrierGroupId)
        {
            try
            {
                _loadCarrierGroupService.DeleteAllLoadCarrierGroupCarriers(loadCarrierGroupId);
                return Success<object>(null);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<object>(ex);
            }
        }

        [HttpGet]
        [Route("LoadCarrierGroupTypes")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadCarrierGroupTypeData>>), 200)]
        public IActionResult GetLoadCarrierGroupTypes()
        {
            return Success(_loadCarrierGroupService.GetLoadCarrierGroupTypes());
        }
    }
}
