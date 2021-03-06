﻿using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadLineItemData
    {
        public long LoadLineItemId { get; set; }
        public int LoadLineItemNumber { get; set; }
        public string ProductDescription { get; set; }
        public int Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public int? UnitOfMeasureId { get; set; }
        public int Weight { get; set; }
        public int Volume { get; set; }
        public string CustomerPurchaseOrder { get; set; }
        public Guid PickupStopId { get; set; }
        public Guid DeliveryStopId { get; set; }
        public int PickupStopNumber { get; set; }
        public int DeliveryStopNumber { get; set; }
    }
}
