using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadStatusNotificationsData
    {
        public bool TextMessageEnabled { get; set; }
        public string TextMessageNumber { get; set; }
        public bool EmailEnabled { get; set; }
        public string Email { get; set; }
        public bool DepartedEnabled { get; set; }
        public bool ArrivedEnabled { get; set; }
        public bool DeliveredEnabled { get; set; }
    }
}
