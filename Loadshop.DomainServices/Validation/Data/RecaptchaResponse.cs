using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Validation.Data
{
    public class RecaptchaResponse
    {
        public bool Success { get; set; }
        [JsonProperty("challenge_ts")]
        public DateTimeOffset? ChallengeTimestamp { get; set; }
        [JsonProperty("hostname")]
        public string HostName { get; set; }
        public decimal Score { get; set; }
        public string Action { get; set; }
        public List<string> ErrorCodes { get; set; }
    }
}
