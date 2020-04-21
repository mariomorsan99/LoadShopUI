using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI
{
    public class InTransitLoadData
    {
        public IList<InTransitEventData> Loads { get; set; }
    }
}
