using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LocationData
    {
        public long LocationId { get; set; }
        public Guid CustomerId { get; set; }
        [Required]
        [StringLength(50)]
        public string LocationName { get; set; }
        [Required]
        [StringLength(50)]
        public string Address1 { get; set; }
        [StringLength(50)]
        public string Address2 { get; set; }
        [StringLength(50)]
        public string Address3 { get; set; }
        [Required]
        [StringLength(30)]
        public string City { get; set; }
        [Required]
        [StringLength(2)]
        public string State { get; set; }
        [Required]
        [StringLength(30)]
        public string Country { get; set; }
        [StringLength(10)]
        public string PostalCode { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }
        public int AppointmentSchedulerConfirmationTypeId { get; set; }
    }
}
