using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Dto
{
    public class LoadStopDto
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
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime? EarlyDtTm { get; set; }
        public string EarlyTime { get; set; }
        public DateTime LateDtTm { get; set; }
        public string LateTime { get; set; }
        public string ApptType { get; set; }
        public string Instructions { get; set; }

        public string AppointmentSchedulingCode { get; set; }
        public string AppointmentConfirmationCode { get; set; }
        public int? AppointmentSchedulerConfirmationTypeId { get; set; }

        /// <summary>
        /// Indicates whether a pickup stop is live loading (vs preload) or a delivery stop is live unloading (vs drop trailer)
        /// </summary>
        public bool? IsLive { get; set; }
    }
}
