using Newtonsoft.Json;

namespace Loadshop.Customer.API.Models.Transportation
{
    public class TransportationModeViewModel
    {
        [JsonProperty("transportationMode")]
        public string Name { get; set; }
    }
}
