using System;

namespace Loadshop.Web.API.Models.Shipping
{
    public class LocationViewModel
    {
        public long LocationId { get; set; }
        public Guid CustomerId { get; set; }
        public string LocationName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int AppointmentSchedulerConfirmationTypeId { get; set; }
    }
}
