using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadStatusInTransitData
    {
        public Guid LoadId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTimeOffset? LocationTime { get; set; }
    }
}
