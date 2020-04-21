using Newtonsoft.Json;

namespace Loadshop.Customer.API.Models.ServiceType
{
    public class ServiceTypeViewModel
    {
        [JsonProperty("serviceType")]
        public string Name { get; set; }
    }
}
