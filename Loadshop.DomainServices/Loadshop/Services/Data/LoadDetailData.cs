using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadDetailData
    {
        public Guid? LoadId { get; set; }
        public Guid? UserLaneId { get; set; }
        [Required]
        public string ReferenceLoadId { get; set; }
        [Required]
        public string ReferenceLoadDisplay { get; set; }
        [ReadOnly(true)]
        public int Stops { get; set; }
        [Required]
        public int Miles { get; set; }
        [Required]
        public decimal LineHaulRate { get; set; }
        [Required]
        public decimal FuelRate { get; set; }
        [Required]
        public int Weight { get; set; }
        public int Cube { get; set; }
        public string Commodity { get; set; }
        [Required]
        public string EquipmentType { get; set; }
        public string EquipmentCategoryId { get; set; }
        public string EquipmentCategoryDesc { get; set; }
        public string EquipmentTypeDisplay { get; set; }
        public bool IsHazMat { get; set; }
        [ReadOnly(true)]
        public bool IsAccepted { get; set; }
        public string Comments { get; set; }
        [Required]
        public List<LoadContactData> Contacts { get; set; }
        [Required]
        public List<LoadStopData> LoadStops { get; set; }
        [Required]
        public List<LoadDocumentMetadata> LoadDocuments { get; set; }
        [ReadOnly(true)]
        [Display(Name = "TransactionData")]
        public LoadTransactionData LoadTransaction { get; set; }

        public List<LoadCarrierScacData> CarrierScacs { get; set; }
        public List<LoadCarrierScacRestrictionData> CarrierScacRestrictions { get; set; }

        [ReadOnly(true)]
        public int? DistanceFromOrig { get; set; }
        [ReadOnly(true)]
        public int? DistanceFromDest { get; set; }
        [ReadOnly(true)]
        public string Scac { get; set; }
        [ReadOnly(true)]
        public UserContactData BookedUser { get; set; }
        [ReadOnly(true)]
        public string BookedUserCarrierName { get; set; }
        public string BillingLoadId { get; set; }
        public string BillingLoadDisplay { get; set; }
        public long? LoadBoardId { get; set; }
        public string VisibilityPhoneNumber { get; set; }
        public string VisibilityTruckNumber { get; set; }
        public bool MobileExternallyEntered { get; set; }
        public DateTime? VisibilityChgDtTm { get; set; }
        public string TransportationMode { get; set; }
        public int? TransportationModeId { get; set; }
        public string ShipperPickupNumber { get; set; }
        public int? CustomerLoadTypeId { get; set; }
        public string PlatformPlusLoadId { get; set; }
        public bool? IsPlatformPlus { get; set; }
        public bool UsesAllInRates { get; set; }
        public bool HasScacRestrictions { get; set; }

        public List<LoadLineItemData> LineItems { get; set; } = new List<LoadLineItemData>();
        public List<ServiceTypeData> ServiceTypes { get; set; } = new List<ServiceTypeData>();
        public DateTime? DeliveredDate { get; set; }
    }
}
