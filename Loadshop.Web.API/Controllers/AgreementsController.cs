using System;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loadshop.Web.API.Controllers
{
    [Authorize]
    [Route("api/agreements")]
    [ApiController]
    public class AgreementsController : BaseController
    {
        private readonly IAgreementDocumentService agreementDocumentService;

        public AgreementsController(IAgreementDocumentService agreementDocumentService)
        {
            this.agreementDocumentService = agreementDocumentService;
        }
               
        [HttpPost("")]
        [ProducesResponseType(typeof(ResponseMessage<AgreementDocumentData>), 200)]
        public async Task<IActionResult> AgreeToDocument()
        {
            await agreementDocumentService.UserAgreement();
            return Success<object>(null);
        }
    }
}