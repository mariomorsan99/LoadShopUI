using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LoadStopController : BaseController
    {
        private readonly ILoadStopService _service;

        public LoadStopController(ILoadStopService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("GetStopTypes")]
        [ProducesResponseType(typeof(ResponseMessage<List<StopTypeData>>), 200)]
        public IActionResult GetStopTypes()
        {
            return Success(_service.GetStopTypes());
        }

        [HttpGet]
        [Route("GetAppointmentCodes")]
        [ProducesResponseType(typeof(ResponseMessage<List<AppointmentSchedulerConfirmationTypeData>>), 200)]
        public IActionResult GetAppointmentCodes()
        {
            return Success(_service.GetAppointmentSchedulerConfirmationTypes());
        }
    }
}
