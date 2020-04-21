using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class SpecialInstructionEquipmentData
    {
        public Guid SpecialInstructionEquipmentId { get; set; }
        public long SpecialInstructionId { get; set; }
        public string EquipmentId { get; set; }
    }
}