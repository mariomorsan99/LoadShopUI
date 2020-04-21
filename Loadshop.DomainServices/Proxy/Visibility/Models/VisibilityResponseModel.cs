using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Proxy.Visibility.Models
{
    public class VisibilityResponseModel<T>
    {
        [JsonProperty("e")]
        public IEnumerable<VisibilityError> Errors { get; set; }
        [JsonProperty("d")]
        public T ResponsePayload { get; set; }
        [JsonProperty("s")]
        public bool Successful { get; set; }
    }

    public class VisibilityError
    {
        [JsonProperty("m")]
        public string Message { get; set; }
    }
}
