using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public interface ILoadFeeData
    {
        Guid CustomerId { get; set; }
        string Scac { get; set; }
        decimal LineHaulRate { get; set; }
        decimal FuelRate { get; set; }
        LoadshopFeeData FeeData { get; set; }
    }
}
