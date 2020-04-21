using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using System.ComponentModel.DataAnnotations;
using Loadshop.Customer.API.Models.Transportation;
using AutoMapper;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TransportationController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ITransportationService _service;
        private readonly IUserContext _user;

        public TransportationController(IMapper mapper, ITransportationService service, IUserContext userContext)
        {
            _mapper = mapper;
            _service = service;
            _user = userContext;
        }

        [HttpGet]
        [Route("GetTransportationModes")]
        [ProducesResponseType(typeof(ResponseMessage<List<TransportationModeViewModel>>), 200)]
        public IActionResult GetTransportationModes()
        {
            try
            {
                return Success(_mapper.Map<List<TransportationModeViewModel>>(_service.GetTransportationModes()));
            }
            catch (ValidationException e)
            {
                return Error<List<TransportationModeViewModel>>(e);
            }
        }
    }
}
