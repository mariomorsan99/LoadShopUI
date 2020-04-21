using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadData : LoadDetailData, ILoadFeeData
    {
        public string EquipmentDesc { get; set; }
        public bool IsEstimatedFSC { get; set; }
        public Guid CustomerId { get; set; }
        public LoadshopFeeData FeeData { get; set; }
    }
}
