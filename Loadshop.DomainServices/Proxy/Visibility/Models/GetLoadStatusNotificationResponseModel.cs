using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Proxy.Visibility.Models
{
   public class GetLoadStatusNotificationResponseModel
    {
        public int StatusFlag { get; set; }
        public int ResponseFlag { get; set; }
        public int EventType { get; set; }
        public DateTime CreateDateUTC { get; set; }
        public DateTime ModifiedDateUTC { get; set; }
        public string SenderCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool StatusIsArrived { get; set; }
        public bool StatusIsDeparted { get; set; }
        public bool StatusIsDelivered { get; set; }
        public bool ResponseIsEmail { get; set; }
        public bool ResponseIsSMS { get; set; }
        public bool ResponseIsMobilePush { get; set; }
        public bool ResponseIsWebPush { get; set; }
        public string Est_email { get; set; }
        public string Est_phone { get; set; }
    }
}
