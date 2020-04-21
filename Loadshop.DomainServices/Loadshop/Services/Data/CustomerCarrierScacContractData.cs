using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CustomerCarrierScacContractData : BaseData
    {
        [Key]
        public Guid CustomerCarrierContractId { get; set; }
        [Required]
        [StringLength(4)]
        public string Scac { get; set; }
        [Required]
        public Guid CustomerId { get; set; }

        public CarrierScacData CarrierScac { get; set; }
        public CustomerData Customer { get; set; }
    }
}
