using Newtonsoft.Json;

namespace Loadshop.Customer.API.Models.Equipment
{
    public class EquipmentViewModel
    {
        [JsonProperty("equipmentType")]
        public string EquipmentId { get; set; }
        public string EquipmentDesc { get; set; }
        public string CategoryId { get; set; }
        public string CategoryEquipmentDesc { get; set; }
        public string CategoryName { get; set; }
    }
}
