using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadVisibilityData
    {
        public Guid LoadClaimId { get; set; }
        public string LoadNbr { get; set; }
        public string PhoneNumber { get; set; }
        public string TruckNumber { get; set; }

    }
}
