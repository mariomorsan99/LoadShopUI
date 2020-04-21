using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Common.Services.Data
{
    public class LoadLaneScacHistoryData
    {
        public string LoadId { get; set; }
        public string RateType { get; set; }
        public string RateName { get; set; }
        public decimal? RateValue { get; set; }
    }
}
