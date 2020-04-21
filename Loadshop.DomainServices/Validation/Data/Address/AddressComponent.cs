using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Validation.Data.Address
{
    public class AddressComponent
    {
        [JsonProperty("short_name")]
        public virtual string ShortName { get; set; }

        [JsonProperty("long_name")]
        public virtual string LongName { get; set; }

        [JsonProperty("types")]
        public virtual IEnumerable<string> Types { get; set; }
    }
}
