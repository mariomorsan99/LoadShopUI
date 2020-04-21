using System;

namespace Loadshop.DomainServices.Utilities.Models.Visibility
{
    public class TopsToGoDetail
    {
        public string LoadId { get; set; }
        public DateTime SendUtc { get; set; }
        public DateTime LinkExpiredUtc { get; set; }
        public DateTime AccessExpiredUtc { get; set; }
    }
}
