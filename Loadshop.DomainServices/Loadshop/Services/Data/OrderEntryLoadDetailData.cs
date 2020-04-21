using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class OrderEntryLoadDetailData : LoadDetailData
    {
        public string EquipmentDesc { get; set; }
        public string CategoryEquipmentDesc { get; set; }
        public bool OnLoadshop { get; set; }

        public new List<OrderEntryLoadStopData> LoadStops { get; set; }
    }
}
