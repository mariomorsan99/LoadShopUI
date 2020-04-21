using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CarrierCarrierScacGroupData
    {
        public CarrierData Carrier { get; set; }
        public List<CarrierScacData> CarrierScacs { get; set; }
    }
}
