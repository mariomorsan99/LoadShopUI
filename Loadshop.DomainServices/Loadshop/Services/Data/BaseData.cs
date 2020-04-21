using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class BaseData
    {
        public string CreateBy { get; set; }
        public DateTime CreateDtTm { get; set; }
        public string LastChgBy { get; set; }
        public DateTime LastChgDtTm { get; set; }
    }
}
