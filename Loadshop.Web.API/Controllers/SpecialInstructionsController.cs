using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Loadshop.Web.API.Security;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = AuthorizationConstants.IsShipperPolicy)]
    [Route("api/SpecialInstructions")]
    [ApiController]
    public class SpecialInstructionsController : BaseController
    {
        private readonly ISpecialInstructionsService specialInstructionsService;
        private readonly IUserContext userContext;

        public SpecialInstructionsController(
            ISpecialInstructionsService specialInstructionsService,
            IUserContext userContext)
        {
            this.specialInstructionsService = specialInstructionsService;
            this.userContext = userContext;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(ResponseMessage<List<SpecialInstructionData>>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shipper_Comments_Add_Edit)]
        public async Task<IActionResult> Get([FromQuery]Guid customerId)
        {
            return Success(await specialInstructionsService.GetSpecialInstructionsAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseMessage<List<SpecialInstructionData>>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shipper_Comments_Add_Edit)]
        public async Task<IActionResult> Get([FromRoute] long id)
        {
            if (id < 0)
            {
                return Error<List<SpecialInstructionData>>("Invalid id");
            }
            try
            {
                return Success(await specialInstructionsService.GetSpecialInstructionAsync(id));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<IList<ShippingLoadCarrierGroupData>>(ex);
            }
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(ResponseMessage<LoadCarrierGroupData>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shipper_Comments_Add_Edit)]
        public async Task<IActionResult> CreateInstruction([FromBody]SpecialInstructionData instruction)
        {
            try
            {
                var response = await specialInstructionsService.CreateSpecialInstructionAsync(instruction, userContext.UserName);
                if (response.IsSuccess)
                {
                    return Success(response.SpecialInstructionData);
                }
                else
                {
                    var problemDetails = new ValidationProblemDetails(response.ModelState)
                    {
                        Title = "Error creating Special Instructions",
                        Detail = "One or more errors occurred when creating the Special Instruction.  See form for error details",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                    };
                    return BadRequest(problemDetails);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<LoadCarrierGroupData>(ex);
            }
        }
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(typeof(ResponseMessage<object>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shipper_Comments_Add_Edit)]
        public async Task<IActionResult> DeleteInstruction(long id)
        {
            try
            {
                await specialInstructionsService.DeleteSpecialInstructionAsync(id);
                return Success<object>(null);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<object>(ex);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseMessage<LoadCarrierGroupData>), 200)]
        [Authorize(Policy = AuthorizationConstants.ActionPolicyPrefix + SecurityActions.Loadshop_Ui_Shipper_Comments_Add_Edit)]
        public async Task<IActionResult> UpdateInstruction([FromRoute]long id, [FromBody]SpecialInstructionData instruction)
        {
            try
            {
                var response = await specialInstructionsService.UpdateSpecialInstructionAsync(instruction, userContext.UserName);
                if (response.IsSuccess)
                {
                    return Success(response.SpecialInstructionData);
                }
                else
                {
                    var problemDetails = new ValidationProblemDetails(response.ModelState)
                    {
                        Title = "Error updating Special Instructions",
                        Detail = "One or more errors occurred when updating the LoadCarrierGroup.  See form for error details",
                        Status = (int)HttpStatusCode.BadRequest,
                        Instance = $"urn:kbxl:error:{Guid.NewGuid()}"
                    };
                    return BadRequest(problemDetails);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden<LoadCarrierGroupData>(ex);
            }
        }

    }
}