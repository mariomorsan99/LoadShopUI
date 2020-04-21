using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Validation.Data.Address
{
    public class Result
    {
        [JsonProperty("place_id")]
        public virtual string PlaceId { get; set; }

        [JsonProperty("geometry")]
        public virtual Geometry Geometry { get; set; }

        [JsonProperty("formatted_address")]
        public virtual string FormattedAddress { get; set; }

        [JsonProperty("partial_match")]
        public virtual bool PartialMatch { get; set; }

        [JsonProperty("plus_code")]
        public virtual PlusCode PlusCode { get; set; }

        [JsonProperty("postcode_localities")]
        public virtual IEnumerable<string> PostcodeLocalities { get; set; }

        [JsonProperty("types")]
        public virtual IEnumerable<string> Types { get; set; }

        [JsonProperty("address_components")]
        public virtual IEnumerable<AddressComponent> AddressComponents { get; set; }
    }
}
