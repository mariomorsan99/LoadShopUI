using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CustomerProfileData
    {
        public Guid? CustomerId { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string DefaultCommodity { get; set; }
        public bool UseFuelRerating { get; set; }
        [Range(0,int.MaxValue)]
        public int? FuelReratingNumberOfDays { get; set; }
        public bool AllowZeroFuel { get; set; }
        public bool AllowEditingFuel { get; set; }
        [StringLength(20)]
        public string TopsOwnerId { get; set; }
        public Guid? SuccessManagerUserId { get; set; }
        public Guid? SuccessSpecialistUserId { get; set; }
        [StringLength(256)]
        public string Comments { get; set; }
        [StringLength(20)]
        public string DataSource { get; set; }
        public int? CustomerLoadTypeId { get; set; }
        public DateTime? CustomerLoadTypeExpirationDate { get; set; }
        public bool IdentUserSetup { get; set; }

        public List<string> CustomerCarrierScacs { get; set; }
        public List<CustomerContactData> CustomerContacts { get; set; }
        public bool AutoPostLoad { get; set; }
        public bool ValidateUniqueReferenceLoadIds { get; set; }
        public bool AllowManualLoadCreation { get; set; }
        public decimal InNetworkFlatFee { get; set; }
        public decimal InNetworkPercentFee { get; set; }
        public decimal OutNetworkFlatFee { get; set; }
        public decimal OutNetworkPercentFee { get; set; }
        public bool InNetworkFeeAdd { get; set; }
        public bool OutNetworkFeeAdd { get; set; }
        public bool RequireMarginalAnalysis { get; set; }
    }
}
