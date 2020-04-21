using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using System.ComponentModel.DataAnnotations;
using Loadshop.Customer.API.Models.Equipment;
using AutoMapper;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EquipmentController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IEquipmentService _commonService;
        private readonly IUserContext _user;

        public EquipmentController(IMapper mapper, IEquipmentService commonService, IUserContext userContext)
        {
            _mapper = mapper;
            _commonService = commonService;
            _user = userContext;
        }

        [HttpGet]
        [Route("GetEquipmentTypes")]
        [ProducesResponseType(typeof(ResponseMessage<List<EquipmentViewModel>>), 200)]
        public IActionResult GetEquipmentTypes()
        {
            try
            {
                return Success(_mapper.Map<List<EquipmentViewModel>>(_commonService.GetEquipment()));
            }
            catch (ValidationException e)
            {
                return Error<List<EquipmentViewModel>>(e);
            }
        }
    }
}
