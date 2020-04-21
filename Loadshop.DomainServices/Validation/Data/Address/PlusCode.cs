using Newtonsoft.Json;

namespace Loadshop.DomainServices.Validation.Data.Address
{
    public class PlusCode
    {
        [JsonProperty("global_code")]
        public virtual string GlobalCode { get; set; }

        [JsonProperty("compound_code")]
        public virtual string LocalCode { get; set; }
    }
}