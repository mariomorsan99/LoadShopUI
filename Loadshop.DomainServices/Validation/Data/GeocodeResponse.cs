using Loadshop.DomainServices.Validation.Data.Address;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Validation.Data
{
    public class GeocodeResponse
    {
        [JsonProperty("results")]
        public virtual IEnumerable<Result> Results { get; set; }
    }
}
