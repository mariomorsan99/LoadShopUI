using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI
{
    public class LoadStatusEvent<T>
    {
        public string MessageType { get; set; }
        public Guid MessageId { get; set; }
        public DateTimeOffset MessageTime { get; set; }
        public string ApiVersion { get; set; }
        public T Payload { get; set; }
        public string ServiceName { get; private set; } = "LoadshopManual";
    }
}
