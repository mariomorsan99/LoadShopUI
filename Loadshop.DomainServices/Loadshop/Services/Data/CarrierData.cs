using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CarrierData : BaseData
    {
        [StringLength(20)]
        public string CarrierId { get; set; }
        [Required]
        [StringLength(50)]
        public string CarrierName { get; set; }
        public bool IsSourceActive { get; set; }
        public bool IsLoadshopActive { get; set; }
        [StringLength(50)]
        public string Address { get; set; }
        [StringLength(30)]
        public string City { get; set; }
        [StringLength(2)]
        public string State { get; set; }
        [StringLength(10)]
        public string Zip { get; set; }
        [StringLength(30)]
        public string Country { get; set; }
        [StringLength(20)]
        public string PHoneNumber { get; set; }
        [StringLength(20)]
        public string OperatingAuthNbr { get; set; }
        [StringLength(50)]
        public string OperatingAuthType { get; set; }
        [Required]
        [StringLength(20)]
        public string USDOTNbr { get; set; }
        public bool HazMatCertified { get; set; }
        public bool HasMexAuth { get; set; }
        public bool HasCanAuth { get; set; }
        [Required]
        [StringLength(20)]
        public string RMISCertification { get; set; }
        [Required]
        [StringLength(20)]
        public string DataSource { get; set; }
        public bool KBXLContracted { get; set; }

        public List<string> CarrierScacs { get; set; }
    }
}
