using Loadshop.DomainServices.Constants;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadDocumentUploadData
    {
        public Guid LoadId { get; set; }
        public LoadDocumentTypeData LoadDocumentType { get; set; }
        public string Comment { get; set; }

        /// <summary>
        /// Set internally on the API side
        /// </summary>
        public IFormFile File { get; set; }
    }
}
