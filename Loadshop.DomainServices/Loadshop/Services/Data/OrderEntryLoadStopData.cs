using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class OrderEntryLoadStopData : LoadStopData
    {
        public string StateName { get; set; }
        public new DateTime? LateDtTm { get; set; }
    }
}
