using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadCarrierGroupDetailData : LoadCarrierGroupData
    {
        public List<LoadCarrierGroupEquipmentData> LoadCarrierGroupEquipment { get; set; } = new List<LoadCarrierGroupEquipmentData>();
        public List<LoadCarrierGroupCarrierData> Carriers { get; set; } = new List<LoadCarrierGroupCarrierData>();
    }
}
