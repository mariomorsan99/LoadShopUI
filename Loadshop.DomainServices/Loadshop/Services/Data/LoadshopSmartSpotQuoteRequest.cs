using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadshopSmartSpotQuoteRequest
    {
        public string OriginCity { get; set; }
        public string OriginState { get; set; }
        public string OriginPostalCode { get; set; }
        public string OriginCountry { get; set; }
        public string DestinationCity { get; set; }
        public string DestinationState { get; set; }
        public string DestinationPostalCode { get; set; }
        public string DestinationCountry { get; set; }
        public string EquipmentId { get; set; }
        public int? Weight { get; set; }
        public DateTimeOffset? PickupDate { get; set; }
    }
}
