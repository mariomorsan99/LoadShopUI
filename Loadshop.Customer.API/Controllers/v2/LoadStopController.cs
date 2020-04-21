using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Security;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Loadshop.Customer.API.Models.LoadStop;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Customer.API.Controllers.v2
{
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LoadStopController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ILoadStopService _service;
        private readonly IUserContext _user;

        public LoadStopController(IMapper mapper, ILoadStopService service, IUserContext userContext)
        {
            _mapper = mapper;
            _service = service;
            _user = userContext;
        }

        [HttpGet]
        [Route("GetStopTypes")]
        [ProducesResponseType(typeof(ResponseMessage<List<StopTypeViewModel>>), 200)]
        public IActionResult GetStopTypes()
        {
            try
            {
                return Success(_mapper.Map<List<StopTypeViewModel>>(_service.GetStopTypes()));
            }
            catch (ValidationException e)
            {
                return Error<List<StopTypeViewModel>>(e);
            }
        }

        [HttpGet]
        [Route("GetApptTypes")]
        [ProducesResponseType(typeof(ResponseMessage<List<ApptTypeViewModel>>), 200)]
        public IActionResult GetApptTypes()
        {
            try
            {
                return Success(new List<ApptTypeViewModel>()
                {
                    new ApptTypeViewModel()
                    {
                        ApptType = "AT"
                    },
                    new ApptTypeViewModel()
                    {
                        ApptType = "BY"
                    },
                    new ApptTypeViewModel()
                    {
                        ApptType = "CS"
                    },
                    new ApptTypeViewModel()
                    {
                        ApptType = "ON"
                    }
                });
            }
            catch (ValidationException e)
            {
                return Error<List<ApptTypeViewModel>>(e);
            }
        }

        [HttpGet]
        [Route("GetAppointmentCodes")]
        [ProducesResponseType(typeof(ResponseMessage<List<AppointmentCodeViewModel>>), 200)]
        public IActionResult GetAppointmentCodes()
        {
            try
            {
                return Success(_mapper.Map<List<AppointmentCodeViewModel>>(_service.GetAppointmentSchedulerConfirmationTypes()));
            }
            catch (ValidationException e)
            {
                return Error<List<AppointmentCodeViewModel>>(e);
            }
        }
    }
}
