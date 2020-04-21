using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    /// <summary>
    /// A load sent to Loadshop from a Shipper is only allowed to modify certain fields on the
    /// load from the Shop It Home tab.  That load (or many loads like it) can then be posted 
    /// to the Loadshop Marketplace for carriers to book.  When posting a load from Shop It Home
    /// any modifications to it must be saved as part of the posting process.  This model defines
    /// those fields that the UI must send to the API as part of a PostLoads request
    /// </summary>
    public class PostingLoad
    {
        public Guid LoadId { get; set; }
        public string Commodity { get; set; }
        public decimal ShippersFSC { get; set; }
        public decimal LineHaulRate { get; set; }
        public decimal? SmartSpotRate { get; set; }
        public decimal? DATGuardRate { get; set; }
        public decimal? MachineLearningRate { get; set; }
        public string Comments { get; set; }
        public bool AllCarriersPosted { get; set; }
        public List<string> CarrierIds { get; set; } = new List<string>();
        public List<long> CarrierGroupIds { get; set; } = new List<long>();
        public List<int> ServiceTypeIds { get; set; } = new List<int>();
    }
}
