using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadStatusStopEventData
    {
        public int StopNumber { get; set; }
        public StopEventTypeEnum EventType { get; set; }
        public DateTimeOffset? EventTime { get; set; }
    }
}
