using Loadshop.DomainServices.Validation.Data.Address.Enums;
using Newtonsoft.Json;

namespace Loadshop.DomainServices.Validation.Data.Address
{
    public class Geometry
    {
        [JsonProperty("location")]
        public virtual Location Location { get; set; }
        [JsonProperty("bounds")]
        public virtual ViewPort Bounds { get; set; }

        [JsonProperty("viewport")]
        public virtual ViewPort ViewPort { get; set; }

        [JsonProperty("location_type")]
        public virtual GeometryLocationType LocationType { get; set; }
    }
}
