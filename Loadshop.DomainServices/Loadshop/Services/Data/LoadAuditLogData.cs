using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadAuditLogData
    {
        public long LoadAuditLogId { get; set; }
        public string AuditTypeId { get; set; }
        public Guid LoadId { get; set; }
        public string ReferenceLoadId { get; set; }
        public Guid CustomerId { get; set; }
        public short Stops { get; set; }
        public DateTime PickupDtTm { get; set; }
        public DateTime DeliveryDtTm { get; set; }
        public int Miles { get; set; }
        public decimal LineHaulRate { get; set; }
        public decimal FuelRate { get; set; }
        public int Weight { get; set; }
        public int Cube { get; set; }
        public string Commodity { get; set; }
        public string EquipmentId { get; set; }
        public bool IsHazMat { get; set; }
        public string Comments { get; set; }
        public Guid UserId { get; set; }
        public string Scac { get; set; }
        public decimal? ContractRate { get; set; }
        public string CarrierName { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreateDtTm { get; set; }
    }
}
