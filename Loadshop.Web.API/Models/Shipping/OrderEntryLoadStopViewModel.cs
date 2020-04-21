using System;
using System.Collections.Generic;

namespace Loadshop.Web.API.Models.Shipping
{
    public class OrderEntryLoadStopViewModel
    {
        public Guid? LoadStopId { get; set; }
        public int StopNbr { get; set; }
        public string StopType { get; set; }
        public int? StopTypeId { get; set; }
        public string LocationName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string StateName { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public DateTime? EarlyDtTm { get; set; }
        public DateTime? EarlyDate { get; set; }
        public string EarlyTime { get; set; }
        public DateTime? LateDtTm { get; set; }
        public DateTime? LateDate { get; set; }
        public string LateTime { get; set; }

        public string ApptType { get; set; }
        public string Instructions { get; set; }

        public string AppointmentSchedulingCode { get; set; }
        public string AppointmentConfirmationCode { get; set; }
        public int? AppointmentSchedulerConfirmationTypeId { get; set; }
        public bool? IsLive { get; set; }

        public List<OrderEntryLoadStopContactViewModel> Contacts { get; set; }
    }
}
