using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IAgreementDocumentService
    {
        Task UserAgreement();
        Task<AgreementDocumentData> GetLatestAgreementDocument(string documentType);
        Task<bool> HasUserAgreedToLatestTermsAndPrivacy(Guid userId);
    }
}
