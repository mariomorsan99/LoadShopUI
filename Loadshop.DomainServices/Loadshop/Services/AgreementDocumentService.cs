using AutoMapper;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class AgreementDocumentService : IAgreementDocumentService
    {
        private readonly LoadshopDataContext context;
        private readonly IMapper mapper;
        private readonly IUserContext userContext;
        private readonly ISecurityService securityService;

        public AgreementDocumentService(LoadshopDataContext context,
            IMapper mapper,
            IUserContext userContext,
            ISecurityService securityService)
        {
            this.context = context;
            this.mapper = mapper;
            this.userContext = userContext;
            this.securityService = securityService;
        }

        public async Task UserAgreement()
        {
            // check if its the latest agreement
            var latest = await GetLatestAgreementDocument(AgreementDocumentTypes.TermsAndPrivacy);

            var user = await context.Users.FirstOrDefaultAsync(x => x.IdentUserId == userContext.UserId);

            // record user agreement
            var entity = new UserAgreementDocumentEntity()
            {
                UserId = user.UserId,
                AgreementDocumentId = latest.AgreementDocumentId,
                AgreementDtTm = DateTime.Now,
                CreateBy = userContext.UserName,
                LastChgBy = userContext.UserName,
                CreateDtTm = DateTime.Now,
                LastChgDtTm = DateTime.Now
            };

            context.UserAgreements.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task<AgreementDocumentData> GetLatestAgreementDocument(string documentType)
        {
            var latest = await context.AgreementDocuments.AsNoTracking()
                                .Where(x => x.AgreementType == documentType)
                                .Where(x => x.AgreementActiveDtTm <= DateTime.Now)
                                .OrderByDescending(x => x.AgreementActiveDtTm)
                                .FirstOrDefaultAsync();

            return mapper.Map<AgreementDocumentData>(latest);
        }

        public async Task<bool> HasUserAgreedToLatestTermsAndPrivacy(Guid userId)
        {
            var latestTerms = await GetLatestAgreementDocument(AgreementDocumentTypes.TermsAndPrivacy);

            if (latestTerms == null)
            {
                // there is no agreement saved in the db, return true
                return true;
            }

            var acceptance = await context.UserAgreements.AsNoTracking()
                                .Where(x => x.UserId == userId)
                                .AnyAsync(x => x.AgreementDocumentId == latestTerms.AgreementDocumentId);

            return acceptance;
        }
    }
}
