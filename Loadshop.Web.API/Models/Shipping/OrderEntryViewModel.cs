using Loadshop.DomainServices.Loadshop.Services.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Loadshop.Web.API.Models.Shipping
{
    public class OrderEntryViewModel
    {
        public Guid? LoadId { get; set; }
        public string ReferenceLoadId { get; set; }
        public string ReferenceLoadDisplay { get; set; }
        public string Commodity { get; set; }
        [JsonProperty("equipment")]
        public string EquipmentType { get; set; }
        public string EquipmentDesc { get; set; }
        public string CategoryEquipmentDesc { get; set; }
        public string ShipperPickupNumber { get; set; }
        public string TransportationMode { get; set; }
        [JsonProperty("specialInstructions")]
        public string Comments { get; set; }
        public bool IsHazMat { get; set; }
        public int Miles { get; set; }
        public bool OnLoadshop { get; set; }
        public decimal LineHaulRate { get; set; }
        public decimal FuelRate { get; set; }
        public int Weight { get; set; }
        public int Cube { get; set; }

        [JsonProperty("services")]
        public List<ServiceTypeData> ServiceTypes { get; set; }
        public List<OrderEntryLoadStopViewModel> LoadStops { get; set; }
        public List<OrderEntryLoadContactViewModel> Contacts { get; set; }
        public List<OrderEntryLoadLineItemViewModel> LineItems { get; set; }
    }
}
