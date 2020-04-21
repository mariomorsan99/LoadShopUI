using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadCarrierGroupEquipmentData
    {
        public Guid LoadCarrierGroupEquipmentId { get; set; }
        public long LoadCarrierGroupId { get; set; }
        public string EquipmentId { get; set; }
    }
}