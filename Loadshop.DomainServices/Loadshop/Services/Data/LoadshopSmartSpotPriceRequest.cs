using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadshopSmartSpotPriceRequest
    {
        public Guid LoadId { get; set; }
        public int Weight { get; set; }
        public string Commodity { get; set; }
        public string EquipmentId { get; set; }
        public List<string> CarrierIds { get; set; }
    }
}
