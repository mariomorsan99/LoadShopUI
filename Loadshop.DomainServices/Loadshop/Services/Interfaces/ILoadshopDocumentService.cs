using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMS.Infrastructure.Messaging.Client;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ILoadshopDocumentService
    {
        Task<FileMemoryStream> GetDocument(Guid loadDocumentId);
        IEnumerable<LoadDocumentTypeData> GetDocumentTypes();
        Task<LoadDocumentMetadata> UploadDocument(LoadDocumentUploadData uploadPayload);
        Task RemoveDocument(Guid loadDocumentId);
    }
}
