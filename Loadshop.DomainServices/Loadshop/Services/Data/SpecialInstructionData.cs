using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class SpecialInstructionData
    {
        public long SpecialInstructionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public Guid CustomerId { get; set; }
        public string OriginAddress1 { get; set; }
        public string OriginCity { get; set; }
        public string OriginState { get; set; }
        public string OriginPostalCode { get; set; }
        public string OriginCountry { get; set; }
        public string DestinationAddress1 { get; set; }
        public string DestinationCity { get; set; }
        public string DestinationState { get; set; }
        public string DestinationPostalCode { get; set; }
        public string DestinationCountry { get; set; }
        public List<SpecialInstructionEquipmentData> SpecialInstructionEquipment { get; set; } = new List<SpecialInstructionEquipmentData>();
    }
}
