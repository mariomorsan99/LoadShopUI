using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CarrierProfileData : CarrierData
    {

        public Guid? CarrierSuccessTeamLeadId { get; set; }
        public Guid? CarrierSuccessSpecialistId { get; set; }
        public string Comments { get; set; }
        public List<CarrierScacData> Scacs { get; set; }
    }
}
