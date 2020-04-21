using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadCarrierGroupData
    {
        public long LoadCarrierGroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
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
        public int LoadCarrierGroupTypeId { get; set; }
        public string LoadCarrierGroupTypeName { get; set; }
        //public CustomerData Customer { get; set; }
        public int CarrierCount { get; set; }
    }
}
