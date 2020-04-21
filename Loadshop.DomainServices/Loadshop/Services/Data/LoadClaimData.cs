using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadClaimData
    {
        [Key]
        public Guid LoadClaimId { get; set; }
        public Guid LoadTransactionId { get; set; }
        public decimal LineHaulRate { get; set; }
        public decimal FuelRate { get; set; }
        [Required]
        [StringLength(4)]
        public string Scac { get; set; }
        [Required]
        [StringLength(30)]
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
