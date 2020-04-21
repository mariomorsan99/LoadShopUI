using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.Web.API.Security;
using Loadshop.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Loadshop.Web.API.Controllers
{
    [Authorize(Policy = AuthorizationConstants.IsCarrierOrShipperPolicy)]
    [Route("api/loaddocuments")]
    [ApiController]
    public class LoadDocumentsController : BaseController
    {
        private readonly ILoadshopDocumentService loadshopDocumentService;

        public LoadDocumentsController(ILoadshopDocumentService loadshopDocumentService)
        {
            this.loadshopDocumentService = loadshopDocumentService;
        }

        [HttpGet("types")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadDocumentTypeData>>), 200)]
        public IActionResult GetDocumentTypes()
        {
            var documentTypes = loadshopDocumentService.GetDocumentTypes();
            return Success(documentTypes);
        }

        [HttpPost("")]
        [RequestSizeLimit(1000000000)]
        [ProducesResponseType(typeof(ResponseMessage<LoadDocumentTypeData>), 200)]
        public async Task<IActionResult> Upload([FromForm] IFormCollection formData)
        {
            var file = formData.Files.FirstOrDefault();

            var json = formData.FirstOrDefault(x => x.Key.Equals("loaddocumentform", StringComparison.CurrentCultureIgnoreCase)).Value;

            if (string.IsNullOrEmpty(json))
            {
                return Error<string>("Invalid document metadata");
            }

            var dto = JsonConvert.DeserializeObject<LoadDocumentUploadData>(json);
            dto.File = file;

            var result = await loadshopDocumentService.UploadDocument(dto);

            return Success(result);
        }

        [HttpDelete("{documentId}")]
        [RequestSizeLimit(1000000000)]
        [ProducesResponseType(typeof(ResponseMessage<LoadDocumentTypeData>), 200)]
        public async Task<IActionResult> Remove([FromRoute] Guid documentId)
        {
            await loadshopDocumentService.RemoveDocument(documentId);

            return Success<object>(null);
        }

        [HttpGet("{documentId}/download")]
        [ProducesResponseType(typeof(ResponseMessage<List<LoadDocumentTypeData>>), 200)]
        public async Task<IActionResult> DownloadDocument([FromRoute] Guid documentId)
        {
            var fileContentStream =  await loadshopDocumentService.GetDocument(documentId);

            // reset the position in order to read stream for response
            fileContentStream.Position = 0;
            return File(fileContentStream, fileContentStream.ContentType, fileContentStream.FileName);
        }
    }
}