using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Exceptions;
using Loadshop.Web.Controllers;
using Loadshop.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models.Models;
using Loadshop.DomainServices.Proxy.Tops.Loadshop;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = SecurityRoles.ShipperAdmin + "," + SecurityRoles.LSAdmin + "," + SecurityRoles.SystemAdmin)]
    public class ShipperAdminController : BaseController
    {
        private readonly IShipperAdminService _shipperAdminService;
        private readonly IUserContext _userContext;
        private readonly ITopsLoadshopApiService _topsLoadshopApiService;
        private readonly ISecurityService _securityService;

        public ShipperAdminController(IShipperAdminService shipperAdminService, IUserContext userContext, ITopsLoadshopApiService topsLoadshopApiService,ISecurityService securityService)
        {
            _shipperAdminService = shipperAdminService;            
            _userContext = userContext;
            _topsLoadshopApiService = topsLoadshopApiService;
            _securityService = securityService;
        }

        //Data to populate drop down of shippers
        [HttpGet]
        [Route("GetAllShippers")]
        [ProducesResponseType(typeof(ResponseMessage<List<CustomerData>>), 200)]
        public IActionResult GetAllShippers()
        {
            return Success(_shipperAdminService.GetAllShippers());
        }

        #region Maintain Shipper/Carrier Associations               

        [HttpGet]
        [Route("GetCarriersForShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<CarrierScacData>>), 200)]
        public IActionResult GetAllShipperCarriers(Guid customerId)
        {
            return Success(_shipperAdminService.GetCarriersForShipper(customerId));
        }

        [HttpGet]
        [Route("GetAvailableCarriersForShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<CarrierScacData>>), 200)]
        public IActionResult GetAvailableCarriersForShipper(Guid customerId)
        {
            return Success(_shipperAdminService.GetAvailableCarriersForShipper(customerId));
        }

        [HttpPost]
        [Route("AddCarrierToShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<CustomerCarrierScacContractData>), 200)]
        public IActionResult AddCarrierToShipper(Guid customerId, [FromBody]string scac)
        {
            if (!_userContext.UserId.HasValue)
            {
                throw new EmptyUserIdException("Invalid UserId");
            }

            return Success(_shipperAdminService.AddCarrierToShipper(customerId, scac, _userContext.UserId.Value));
        }

        [HttpPost]
        [Route("AddCarriersToShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<CustomerCarrierScacContractData>>), 200)]
        public IActionResult AddCarriersToShipper(Guid customerId, [FromBody]List<string> scacs)
        {
            if (!_userContext.UserId.HasValue)
            {
                throw new EmptyUserIdException("Invalid UserId");
            }

            return Success(_shipperAdminService.AddCarriersToShipper(customerId, scacs, _userContext.UserId.Value));
        }

        [HttpPost]
        [Route("DeleteCarrierFromShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult DeleteCarrierFromShipper(Guid customerId, Guid customerCarrierContractId)
        {
            return Success(_shipperAdminService.DeleteCarrierFromShipper(customerId, customerCarrierContractId));
        }

        [HttpPost]
        [Route("DeleteCarriersFromShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult DeleteCarriersFromShipper(Guid customerId, [FromBody]List<Guid> customerCarrierContractIds)
        {
            return Success(_shipperAdminService.DeleteCarriersFromShipper(customerId, customerCarrierContractIds));
        }

        [HttpPost]
        [Route("DeleteAllCarriersFromShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult DeleteAllCarriersFromShipper(Guid customerId)
        {
            return Success(_shipperAdminService.DeleteAllCarriersFromShipper(customerId));
        }

        #endregion Maintain Shipper/Carrier Associations


        #region Maintain Shipper/User Associations

        [HttpGet]
        [Route("GetUsersForShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<UserProfileData>>), 200)]
        public IActionResult GetUsersForShipper(Guid customerId)
        {
            return Success(_shipperAdminService.GetUsersForShipper(customerId));
        }

        [HttpGet]
        [Route("GetAvailableUsersForShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<UserProfileData>>), 200)]
        public IActionResult GetAvailableUsersForShipper(Guid customerId)
        {
            return Success(_shipperAdminService.GetAvailableUsersForShipper(customerId));
        }

        [HttpPost]
        [Route("AddUserToShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<UserProfileData>), 200)]
        public IActionResult AddUserToShipper(Guid customerId, [FromBody]Guid userId)
        {
            if (!_userContext.UserId.HasValue)
            {
                throw new EmptyUserIdException("Invalid UserId");
            }

            return Success(_shipperAdminService.AddUserToShipper(customerId, userId, _userContext.UserId.Value));
        }

        [HttpPost]
        [Route("AddUsersToShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<UserProfileData>>), 200)]
        public IActionResult AddUsersToShipper(Guid customerId, [FromBody]List<Guid> userIds)
        {
            if (!_userContext.UserId.HasValue)
            {
                throw new EmptyUserIdException("Invalid UserId");
            }

            return Success(_shipperAdminService.AddUsersToShipper(customerId, userIds, _userContext.UserId.Value));
        }

        [HttpPost]
        [Route("DeleteUserFromShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult DeleteUserFromShipper(Guid customerId, Guid userId)
        {
            return Success(_shipperAdminService.DeleteUserFromShipper(customerId, userId));
        }

        [HttpPost]
        [Route("DeleteUsersFromShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult DeleteUsersFromShipper(Guid customerId, [FromBody]List<Guid> userIds)
        {
            return Success(_shipperAdminService.DeleteUsersFromShipper(customerId, userIds));
        }

        [HttpPost]
        [Route("DeleteAllUsersFromShipper/{customerId}")]
        [ProducesResponseType(typeof(ResponseMessage<bool>), 200)]
        public IActionResult DeleteAllUsersFromShipper(Guid customerId)
        {
            return Success(_shipperAdminService.DeleteAllUsersFromShipper(customerId));
        }

        #endregion Maintain Shipper/User Associations


        #region Maintain Shippers

        [HttpGet]
        [Route("GetShipper")]
        [ProducesResponseType(typeof(ResponseMessage<CustomerProfileData>), 200)]
        public IActionResult GetShipper(Guid? customerId)
        {
            if (!customerId.HasValue || customerId.Value == Guid.Empty)
            {
                throw new EmptyUserIdException("Invalid CustomerId");
            }

            return Success(_shipperAdminService.GetShipper(customerId.Value));
        }

        [HttpPut]
        [ProducesResponseType(typeof(ResponseMessage<CustomerProfileData>), 200)]
        public IActionResult CreateShipper([FromBody]CustomerProfileData customer)
        {
            try
            {
                return Success(_shipperAdminService.AddShipper(customer, _userContext.UserName));
            }
            catch (ValidationException exception)
            {
                return Error<CustomerProfileData>(exception);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage<CustomerProfileData>), 200)]
        public IActionResult UpdateShipper([FromBody]CustomerProfileData customer)
        {
            try
            {
                return Success(_shipperAdminService.UpdateShipper(customer, _userContext.UserName));
            }
            catch (ValidationException exception)
            {
                return Error<CustomerProfileData>(exception);
            }
        }
        #endregion  Maintain Shippers

        #region Maintain Loadshop Shipper Mappings
        [HttpGet]
        [Route("GetSourceSystemOwners")]
        [ProducesResponseType(typeof(ResponseMessage<Dictionary<string, List<string>>>), 200)]
        public async Task<IActionResult> GetSourceSystemOwners(string ownerId = null)
        {
            await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_ShipperProfile_CustomerMapping);
            return Success(await _topsLoadshopApiService.GetSourceSystemOwners(ownerId));
        }

        [HttpGet]
        [Route("GetShipperMappings")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadshopShipperMappingModel>>), 200)]
        public async Task<IActionResult> GetShipperMappings(string ownerId)
        {
            await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_ShipperProfile_CustomerMapping);
            return Success(await _topsLoadshopApiService.GetShipperMappings(ownerId));
        }

        [HttpPost]
        [Route("ShipperMapping")]
        [ProducesResponseType(typeof(ResponseMessage<LoadshopShipperMappingModel>), 200)]
        public async Task<IActionResult> CreateShipperMapping([FromBody]LoadshopShipperMappingModel mapping)
        {
            await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_ShipperProfile_CustomerMapping);

            mapping.CreateBy = _userContext.UserName;
            mapping.CreateDtTm = DateTime.Now;
            mapping.LastChgBy = _userContext.UserName;
            mapping.LastChgDtTm = DateTime.Now;


            return Success(await _topsLoadshopApiService.CreateShipperMapping(mapping));
        }

        [HttpPut]
        [Route("ShipperMapping")]
        [ProducesResponseType(typeof(ResponseMessage<LoadshopShipperMappingModel>), 200)]
        public async Task<IActionResult> UpdateShipperMapping([FromBody]LoadshopShipperMappingModel mapping)
        {
            await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_ShipperProfile_CustomerMapping);
            mapping.LastChgBy = _userContext.UserName;
            mapping.LastChgDtTm = DateTime.Now;

            return Success(await _topsLoadshopApiService.UpdateShipperMapping(mapping));
        }

        [HttpPost]
        [Route("SetupCustomerApi")]
        [ProducesResponseType(typeof(ResponseMessage<CustomerProfileData>), 200)]
        public async Task<IActionResult> SetupCustomerApi([FromBody]CustomerProfileData customer)
        {
            return Success(await _shipperAdminService.CreateCustomerUser(customer.CustomerId.Value, _userContext.UserName));
        }

        #endregion Maintain Loadshop Shipper Mappings
    }
}
