using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadStatusStopData
    {
        public Guid LoadId { get; set; }
        public List<LoadStatusStopEventData> Events { get; set; }
    }
}
