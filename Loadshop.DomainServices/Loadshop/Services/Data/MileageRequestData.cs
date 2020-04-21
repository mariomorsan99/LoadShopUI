namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class MileageRequestData
    {
        public string OriginCity { get; set; }
        public string OriginState { get; set; }
        public string OriginPostalCode { get; set; }
        public string OriginCountry { get; set; }

        public string DestinationCity { get; set; }
        public string DestinationState { get; set; }
        public string DestinationPostalCode { get; set; }
        public string DestinationCountry { get; set; }

        public int DefaultMiles { get; set; }
    }
}
