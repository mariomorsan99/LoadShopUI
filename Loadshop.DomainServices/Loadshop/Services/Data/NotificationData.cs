using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class NotificationData
    {
        public Guid NotificationId { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentDtTm { get; set; }
    }
}
