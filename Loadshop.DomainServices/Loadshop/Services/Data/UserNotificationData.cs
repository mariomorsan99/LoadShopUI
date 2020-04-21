using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserNotificationData
    {
        public Guid UserNotificationId { get; set; }
        public string MessageTypeId { get; set; }
        public string NotificationValue { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public MessageTypeData MessageType { get; set; }
        public bool NotificationEnabled { get; set; }
    }
}
