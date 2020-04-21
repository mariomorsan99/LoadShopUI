namespace Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI
{
    public class StopEventData
    {
        public string LoadNumber { get; set; }
        public string Scac { get; set; }
        public int StopNbr { get; set; }
        public string StatusType { get; set; }
        public string StatusDateTime { get; set; }
        public bool IsLocalTime { get; set; }
    }
}
