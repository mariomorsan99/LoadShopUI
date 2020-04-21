using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class AgreementDocumentData
    {
        public Guid AgreementDocumentId { get; set; }
        /// <summary>
        /// PrivacyAndTerms
        /// </summary>
        public string AgreementType { get; set; }
    }
}
