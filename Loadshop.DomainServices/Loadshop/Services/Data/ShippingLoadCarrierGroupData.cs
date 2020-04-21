using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class ShippingLoadCarrierGroupData : LoadCarrierGroupData
    {
        public string ShippingLoadCarrierGroupDisplay { get; set; }

        public List<LoadCarrierGroupEquipmentData> LoadCarrierGroupEquipment { get; set; } = new List<LoadCarrierGroupEquipmentData>();
        public List<CarrierData> Carriers { get; set; }
    }
}
