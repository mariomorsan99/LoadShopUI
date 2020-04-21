using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadshopFeeData
    {
        public decimal LoadshopPercentFee { get; set; }
        public decimal LoadshopFlatFee { get; set; }
        public bool LoadshopFeeAdd { get; set; }
        public decimal LoadshopFee { get; set; }
    }
}
