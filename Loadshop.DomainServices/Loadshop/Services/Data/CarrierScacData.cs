using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CarrierScacData : BaseData
    {
        [StringLength(4)]
        public string Scac { get; set; }
        [Required]
        [StringLength(50)]
        public string ScacName { get; set; }
        [Required]
        [StringLength(20)]
        public string CarrierId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDedicated { get; set; }
        public bool IsBookingEligible { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        [Required]
        [StringLength(20)]
        public string DataSource { get; set; }
    }
}
