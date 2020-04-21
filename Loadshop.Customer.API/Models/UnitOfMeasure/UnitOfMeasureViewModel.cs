using Newtonsoft.Json;

namespace Loadshop.Customer.API.Models.UnitOfMeasure
{
    public class UnitOfMeasureViewModel
    {
        [JsonProperty("unitOfMeasure")]
        public string Name { get; set; }
    }
}
