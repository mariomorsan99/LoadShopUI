using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadViewData : ILoadFeeData
    {
        public Guid LoadId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? UserLaneId { get; set; }
        public string ReferenceLoadId { get; set; }
        public string ReferenceLoadDisplay { get; set; }
        public double OriginLat { get; set; }
        public double OriginLng { get; set; }
        public string OriginCity { get; set; }
        public string OriginState { get; set; }
        public DateTime? OriginEarlyDtTm { get; set; }
        public DateTime OriginLateDtTm { get; set; }
        public double DestLat { get; set; }
        public double DestLng { get; set; }
        public string DestCity { get; set; }
        public string DestState { get; set; }
        public DateTime? DestEarlyDtTm { get; set; }
        public DateTime DestLateDtTm { get; set; }
        public string EquipmentId { get; set; }
        public string EquipmentType { get; set; }
        public string EquipmentCategoryId { get; set; }
        public string EquipmentCategoryDesc { get; set; }
        public string EquipmentTypeDisplay { get; set; }
        public short Stops { get; set; }
        public int Miles { get; set; }
        public decimal LineHaulRate { get; set; }
        public decimal FuelRate { get; set; }
        public string Commodity { get; set; }
        public int Weight { get; set; }
        public bool IsHazMat { get; set; }
        public string TransactionTypeId { get; set; }
        public int? DistanceFromOrig { get; set; }
        public int? DistanceFromDest { get; set; }
        public string Scac { get; set; }
        public string BookedUser { get; set; }
        public string BookedUserCarrierName { get; set; }
        public string BillingLoadId { get; set; }
        public string BillingLoadDisplay { get; set; }
        public string VisibilityPhoneNumber { get; set; }
        public string VisibilityTruckNumber { get; set; }        
        public bool MobileExternallyEntered { get; set; }
        public DateTime? VisibilityChgDtTm { get; set; }
        public bool? ShowVisibilityWarning { get; set; }
        public bool IsEstimatedFSC { get; set; }
        public int? CustomerLoadTypeId { get; set; }
        public bool IsPlatformPlus { get; set; }
        public bool HasContractedCarrierScac { get; set; }

        public LoadshopFeeData FeeData { get; set; }
        /// <summary>
        /// Return just the service type id from the LoadServiceType entity
        /// </summary>
        public IEnumerable<int> ServiceTypeIds { get { return this.LoadServiceTypes.Select(x => x.ServiceTypeId).ToList(); } }
        [JsonIgnore]
        public IEnumerable<LoadServiceTypeEntity> LoadServiceTypes { get; set; }
    }
}
