using Loadshop.API.Models;
using Loadshop.DomainServices.Common.Services.Crud;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.API.Models.DataModels;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = Security.AuthorizationConstants.IsCarrierOrShipperAdmin)]
    [Route("api/[controller]")]
    public class UserAdminController : BaseCrudController<UserData>
    {
        private readonly IUserAdminService _userAdminService;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;


        public UserAdminController(IUserAdminService userAdminService, IUserContext userContext, ISecurityService securityService) : base(userAdminService)
        {
            _userAdminService = userAdminService;
            _userContext = userContext;
            _securityService = securityService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get([FromRoute] Guid userId)
        {
            return await GetByKey(userId);
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            return await GetCollection<UserData>();
        }

        protected override Task<CrudResult<IEnumerable<TListData>>> GetCollectionServiceCall<TListData>(int? take, int? skip)
        {
            string query = Request.Query["query"];

            if (query != null)
                return _userAdminService.GetCollection<TListData>(query);

            return base.GetCollectionServiceCall<TListData>(take, skip);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserData data)
        {
            return await base.Create(data);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update([FromBody] UserData data, [FromRoute]Guid userId)
        {
            return await base.Update(data, true, userId);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete([FromRoute]Guid userId)
        {
            return await base.Delete(userId);
        }

        [HttpGet(nameof(GetAllMyAuthorizedShippers))]
        public async Task<IActionResult> GetAllMyAuthorizedShippers()
        {
            var authorizedShippers = await _userAdminService.GetAuthorizedCustomers();

            return Success(authorizedShippers);
        }

        [HttpGet(nameof(GetAllMyAuthorizedCarrierScacs))]
        public async Task<IActionResult> GetAllMyAuthorizedCarrierScacs()
        {
            var authorizedCarrierScacs = await _userAdminService.GetAuthorizedCarrierScacs();

            return Success(authorizedCarrierScacs);
        }

        [HttpGet(nameof(GetAllMyAuthorizedSecurityRoles))]
        public async Task<IActionResult> GetAllMyAuthorizedSecurityRoles()
        {
            _userContext.UserId.NullArgumentCheck(nameof(_userContext.UserId));

            var authorizedCarrierScacs = await _userAdminService.GetAvailableSecurityRoles(_userContext.UserId.Value);

            return Success(authorizedCarrierScacs);
        }

        [HttpGet(nameof(GetAllAdminUsers))]
        public async Task<IActionResult> GetAllAdminUsers()
        {
            var users = await _userAdminService.GetAllAdminUsers();

            return Success(users);
        }

        [HttpGet(nameof(GetIdentityUserForCreate) +"/{userName}")]
        public async Task<IActionResult> GetIdentityUserForCreate([FromRoute]string userName)
        {
            try
            {
                var identityUser = await _userAdminService.GetIdentityUserForCreate(userName);

                return Success(identityUser);
            }catch(Exception ex)
            {
                return Error<IdentityUserData>(ex);
            }
        }

        [HttpGet]
        [Route("GetCarrierUsers/{carrierId}")]
        [ProducesResponseType(typeof(ResponseMessage<List<UserData>>), 200)]
        public async Task<IActionResult> GetCarrierUsers(string carrierId)
        {
            try
            {
                if (!_userContext.UserId.HasValue)
                {
                    throw new EmptyUserIdException("UserId is null");
                }

                await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_System_Carrier_User_Add_Edit);

                // If the user is not an admin user, then we should exclude admin users from the results
                var excludeAdminUsers = !await _securityService.IsAdminAsync();
                var response = await _userAdminService.GetCarrierUsersAsync(carrierId, excludeAdminUsers);

                return Success(response);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet(nameof(GetAllMyAuthorizedCarriers))]
        [ProducesResponseType(typeof(ResponseMessage<List<CarrierData>>), 200)]
        public async Task<IActionResult> GetAllMyAuthorizedCarriers()
        {
            var authorizedCarriers = await _userAdminService.GetAllMyAuthorizedCarriersAsync();
            return Success(authorizedCarriers);
        }
    }
}
