using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class SmartSpotPrice
    {
        public Guid LoadId { get; set; }
        public decimal Price { get; set; }
        public decimal? DATGuardRate { get; set; }
        public decimal? MachineLearningRate { get; set; }
    }
}
