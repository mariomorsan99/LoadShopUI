using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Loadshop.API.Models;
using Loadshop.Customer.API.Models.ServiceType;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ServiceTypeController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IServiceTypeService _service;
        private readonly IUserContext _user;

        public ServiceTypeController(IMapper mapper, IServiceTypeService service, IUserContext userContext)
        {
            _mapper = mapper;
            _service = service;
            _user = userContext;
        }

        [HttpGet]
        [Route("GetServiceTypes")]
        [ProducesResponseType(typeof(ResponseMessage<List<ServiceTypeViewModel>>), 200)]
        public IActionResult GetServiceTypes()
        {
            try
            {
                return Success(_mapper.Map<List<ServiceTypeViewModel>>(_service.GetServiceTypes()));
            }
            catch (ValidationException e)
            {
                return Error<List<ServiceTypeViewModel>>(e);
            }
        }
    }
}
