using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class PostLoadsResponse : BaseServiceResponse
    {
        /// <summary>
        /// The LoadIds of all the loads that were Posted successfully
        /// </summary>
        public List<ShippingLoadData> PostedLoads { get; set; } = new List<ShippingLoadData>();
        /// <summary>
        /// All of the LoadCarrierScac entities created
        /// </summary>
        public List<LoadCarrierScacEntity> LoadCarrierScacs { get; set; } = new List<LoadCarrierScacEntity>();
    }
}
