using AutoMapper;
using DocumentService.Domain.Data;
using DocumentService.SDK.Version.V1;
using Loadshop.Data;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Extensions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMS.Infrastructure.Messaging.Client;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class LoadshopDocumentService : ILoadshopDocumentService
    {
        private readonly LoadshopDataContext context;
        private readonly IDocumentApiClient documentApiClient;
        private readonly IUserContext userContext;
        private readonly ILogger<LoadshopDocumentService> logger;
        private readonly IMapper mapper;
        private readonly ISecurityService securityService;

        public LoadshopDocumentService(LoadshopDataContext context,
            IDocumentApiClient documentApiClient,
            IUserContext userContext,
            ILogger<LoadshopDocumentService> logger,
            IMapper mapper,
            ISecurityService securityService)
        {
            this.context = context;
            this.documentApiClient = documentApiClient;
            this.userContext = userContext;
            this.logger = logger;
            this.mapper = mapper;
            this.securityService = securityService;
        }

        public IEnumerable<LoadDocumentTypeData> GetDocumentTypes()
        {

            var docTypes = System.Enum.GetValues(typeof(LoadshopDocumentServiceDocumentTypes)).Cast<LoadshopDocumentServiceDocumentTypes>()
                             .Select(x => new LoadDocumentTypeData()
                             {
                                 Description = x.GetEnumDescription(),
                                 Id = (int)x
                             }).ToList();

            return docTypes;
        }

        public async Task<FileMemoryStream> GetDocument(Guid loadDocumentId)
        {
            securityService.GuardAction(SecurityActions.Loadshop_Ui_Carrier_MyLoads_Documents_View);
            var loadDocument = await context.LoadDocuments.AsNoTracking().FirstOrDefaultAsync(x => x.LoadDocumentId == loadDocumentId);

            if (loadDocument == null)
            {
                throw new ValidationException("Document not found");
            }

            var content = await documentApiClient.GetDocumentContent(loadDocument.DocumentServiceDocHeaderId, loadDocument.DocumentServiceDocContentId);

            return content;
        }

        /// <summary>
        /// Uploads a document to the KBX Document Service and stores metadata in Loadshop
        /// </summary>
        /// <param name="uploadPayload"></param>
        /// <returns></returns>
        public async Task<LoadDocumentMetadata> UploadDocument(LoadDocumentUploadData uploadPayload)
        {
            securityService.GuardAction(SecurityActions.Loadshop_Ui_Carrier_MyLoads_Documents_Attach);

            if (uploadPayload.LoadDocumentType == null || uploadPayload.LoadDocumentType.Id < 1)
            {
                throw new ValidationException("Invalid load document type");
            }

            var load = await context.Loads.AsNoTracking().FirstOrDefaultAsync(x => x.LoadId == uploadPayload.LoadId);

            if (load == null)
            {
                throw new ValidationException("Load not found");
            }

            var billingLoadId = await context.LoadTransactions.AsNoTracking()
                                        .Include(x => x.Claim)
                                        .Where(x => x.TransactionTypeId == TransactionTypes.Accepted)
                                        .Where(x => x.LoadId == load.LoadId)
                                        .Select(x => x.Claim.BillingLoadId)
                                        .FirstOrDefaultAsync();

            // copy stream to request
            var stream = new FileMemoryStream();
            await uploadPayload.File.CopyToAsync(stream);
            // reset position to ensure http request sends payload
            stream.Position = 0;
            stream.FileName = uploadPayload.File.FileName;
            stream.ContentDisposition = uploadPayload.File.ContentDisposition;
            stream.ContentType = uploadPayload.File.ContentType;

            // we hold all metadata in loadshop; however we will include some fields in case TOPS needs to query anything from there in the future
            var request = new DocumentService.SDK.Version.V1.Model.DocumentCreate()
            {
                Properties = new List<DocPropertyData>()
                {
                    new DocPropertyData()
                    {
                        PropertyValue = load.LoadId.ToString(),
                        PropertyName =DocumentServiceConstants.Property_Name_LoadshopLoadId
                    },
                    new DocPropertyData()
                    {
                        PropertyValue = load.PlatformPlusLoadId?.ToString(),
                        PropertyName = DocumentServiceConstants.Property_Name_PlatformPlusLoadId
                    },
                    new DocPropertyData()
                    {
                        PropertyValue = load.ReferenceLoadId?.ToString(),
                        PropertyName = DocumentServiceConstants.Property_Name_ReferenceLoadId
                    },
                    new DocPropertyData()
                    {
                        PropertyValue = billingLoadId,
                        PropertyName = DocumentServiceConstants.Property_Name_BillingLoadId
                    }
                },
                DocTypeId = uploadPayload.LoadDocumentType.Id,
                CreatedBy = userContext.UserName,
                CreatedDateTime = DateTime.Now,
                DocumentFile = stream
            };

            try
            {
                // upload file to document API
                var uploadResult = await documentApiClient.CreateDocument(request);

                if (uploadResult.Status.Equals("Error", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new Exception($"Error while upload document to Document Service: {uploadResult.Result}");
                }

                var entity = new LoadDocumentEntity()
                {
                    LoadDocumentId = Guid.NewGuid(),
                    DocumentServiceDocHeaderId = uploadResult.ResultData.DocHeaderId.Value,
                    DocumentServiceDocContentId = uploadResult.ResultData.DocContentId.Value,
                    DocumentServiceDocumentType = uploadPayload.LoadDocumentType.Id,
                    FileName = uploadPayload.File.FileName,
                    Comment = uploadPayload.Comment,
                    LoadId = load.LoadId,
                    CreateBy = userContext.UserName,
                    CreateDtTm = DateTime.Now,
                    LastChgBy = userContext.UserName,
                    LastChgDtTm = DateTime.Now
                };

                // add the new entity and in order for the DB to generate the doc id, use that to pass to the document service
                context.LoadDocuments.Add(entity);
                await context.SaveChangesAsync();

                return mapper.Map<LoadDocumentMetadata>(entity);
            }
            catch (Exception e)
            {
                // if any errors occured, we dont want to show the record in loadshop, so remove the attachment record and return error
                logger.LogError($"Error while uploading document: {e.Message}", e);
            }
            return null;
        }

        /// <summary>
        /// Removes a document from loadshop and document service
        /// </summary>
        /// <param name="loadDocumentId"></param>
        /// <returns></returns>
        public async Task RemoveDocument(Guid loadDocumentId)
        {
            securityService.GuardAction(SecurityActions.Loadshop_Ui_Carrier_MyLoads_Documents_Remove);

            var loadDocument = await context.LoadDocuments.FirstOrDefaultAsync(x => x.LoadDocumentId == loadDocumentId);

            if (loadDocument == null)
            {
                throw new ValidationException("Document not found");
            }

            var load = await context.Loads.AsNoTracking().FirstOrDefaultAsync(x => x.LoadId == loadDocument.LoadId);

            if (load == null)
            {
                throw new ValidationException("Load not found");
            }

            try
            {
                // remove file from document API
                await documentApiClient.DeleteDocument(loadDocument.DocumentServiceDocHeaderId);

                // add the new entity and in order for the DB to generate the doc id, use that to pass to the document service
                context.LoadDocuments.Remove(loadDocument);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // if any errors occured, we dont want to show the record in loadshop, so remove the attachment record and return error
                logger.LogError($"Error while removing document from Document Service: {e.Message}, removing db record from Loadshop", e);
            }
        }

    }
}
