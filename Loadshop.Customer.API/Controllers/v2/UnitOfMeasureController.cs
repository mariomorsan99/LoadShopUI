using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using System.ComponentModel.DataAnnotations;
using Loadshop.Customer.API.Models.UnitOfMeasure;
using AutoMapper;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UnitOfMeasureController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfMeasureService _service;
        private readonly IUserContext _user;

        public UnitOfMeasureController(IMapper mapper, IUnitOfMeasureService service, IUserContext userContext)
        {
            _mapper = mapper;
            _service = service;
            _user = userContext;
        }

        [HttpGet]
        [Route("GetUnitOfMeasures")]
        [ProducesResponseType(typeof(ResponseMessage<List<UnitOfMeasureViewModel>>), 200)]
        public IActionResult GetUnitOfMeasures()
        {
            try
            {
                return Success(_mapper.Map<List<UnitOfMeasureViewModel>>(_service.GetUnitOfMeasures()));
            }
            catch (ValidationException e)
            {
                return Error<List<UnitOfMeasureViewModel>>(e);
            }
        }
    }
}
