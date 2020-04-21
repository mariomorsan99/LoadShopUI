using Newtonsoft.Json;

namespace Loadshop.Customer.API.Models.LoadStop
{
    public class StopTypeViewModel
    {
        [JsonProperty("stopType")]
        public string Name { get; set; }
    }
}
