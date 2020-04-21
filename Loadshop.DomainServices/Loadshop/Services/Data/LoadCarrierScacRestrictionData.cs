using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadCarrierScacRestrictionData
    {
        public long LoadCarrierScacRestrictionId { get; set; }
        [Required]
        public string Scac { get; set; }
        public int LoadCarrierScacRestrictionTypeId { get; set; }
    }
}

