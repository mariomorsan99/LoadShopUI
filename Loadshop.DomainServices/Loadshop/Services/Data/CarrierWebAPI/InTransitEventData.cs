namespace Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI
{
    public class InTransitEventData
    {
        public string LoadNumber { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string LocationTime { get; set; }
        public string Scac { get; set; }
        public bool IsLocalTime { get; set; }
    }
}
