using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = Security.AuthorizationConstants.ActionPolicyPrefix + DomainServices.Security.SecurityActions.Loadshop_Ui_System_Carrier_Add_Edit)]
    public class CarrierAdminController : BaseCrudController<CarrierProfileData>
    {
        public CarrierAdminController(ICarrierAdminService carrierAdminService) : base(carrierAdminService)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<List<CarrierProfileData>>), 200)]
        public async Task<IActionResult> Get()
        {
            return await base.GetCollection<CarrierData>();
        }

        [HttpGet]
        [Route("{carrierId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<CarrierProfileData>>), 200)]
        public async Task<IActionResult> Get([FromRoute] string carrierId)
        {
            return await base.GetByKey(carrierId);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ResponseMessage<List<CarrierProfileData>>), 200)]
        public async Task<IActionResult> UpdateCarrier([FromBody] CarrierProfileData carrierProfileData)
        {
            return await base.Update(carrierProfileData, true, carrierProfileData.CarrierId);
        }
    }
}
