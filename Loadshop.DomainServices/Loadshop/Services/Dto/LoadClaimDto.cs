using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Dto
{
    class LoadClaimDto
    {
        public Guid LoadClaimId { get; set; }
        public Guid LoadTransactionId { get; set; }
        public decimal LineHaulRate { get; set; }
        public decimal FuelRate { get; set; }
        public string Scac { get; set; }
        public Guid UserId { get; set; }
        public bool IsCounterOffer { get; set; }

        public string BillingLoadId { get; set; }
        public string BillingLoadDisplay { get; set; }
        public string VisibilityPhoneNumber { get; set; }
        public string VisibilityTruckNumber { get; set; }
        public bool MobileExternallyEntered { get; set; }
        public DateTime? VisibilityChgDtTm { get; set; }
    }
}
