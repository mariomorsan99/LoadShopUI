using Newtonsoft.Json;

namespace Loadshop.Customer.API.Models.Commodity
{
    public class CommodityViewModel
    {
        [JsonProperty("commodity")]
        public string CommodityName { get; set; }
    }
}
