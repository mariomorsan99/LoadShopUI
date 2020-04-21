using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using Microsoft.AspNetCore.Authorization;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.API.Security;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = AuthorizationConstants.IsCarrierOrShipperPolicy)]
    public class CarrierController : BaseController
    {
        private readonly ICarrierService _carrierService;
        private readonly IUserProfileService _userProfileService;
        private readonly ISecurityService _securityService;
        private readonly IUserContext _userContext;

        public CarrierController(ICarrierService carrierService, IUserProfileService userProfileService, ISecurityService securityService, IUserContext userContext)
        {
            _carrierService = carrierService;
            _userProfileService = userProfileService;
            _securityService = securityService;
            _userContext = userContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<List<CarrierData>>), 200)]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Success(await _securityService.GetContractedCarriersByPrimaryCustomerIdAsync());
            }
            catch (Exception ex)
            {
                return Error<List<CarrierData>>(ex);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseMessage<CarrierCarrierScacGroupData>), 200)]
        [Route("All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Success(await _carrierService.GetAllCarrierScacsAsync());
            }
            catch (Exception ex)
            {
                return Error<List<CarrierData>>(ex);
            }

        }
    }
}