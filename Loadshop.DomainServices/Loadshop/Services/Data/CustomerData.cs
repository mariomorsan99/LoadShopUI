using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CustomerData
    {
        public Guid CustomerId { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public Guid IdentUserId { get; set; }
        public bool UseContractRates { get; set; }
        public bool AutoAcceptBookedLoads { get; set; }
        [StringLength(30)]
        public string CustomerOfferingLevelId { get; set; }
        public string DefaultCommodity { get; set; }
        public bool AllowZeroFuel { get; set; }
        public bool AllowEditingFuel { get; set; }
        public bool ValidateAddresses { get; set; }
        public bool UseFuelRerating { get; set; }
        public int FuelReratingNumberOfDays { get; set; }
        public int? CustomerLoadTypeId { get; set; }
        public DateTime? CustomerLoadTypeExpirationDate { get; set; }
        public bool RequireMarginalAnalysis { get; set; }
    }
}
