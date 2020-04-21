using Newtonsoft.Json;

namespace Loadshop.DomainServices.Validation.Data.Address
{
    public class ViewPort
    {
        [JsonProperty("southwest")]
        public virtual Location SouthWest { get; set; }

        [JsonProperty("northeast")]
        public virtual Location NorthEast { get; set; }
    }
}