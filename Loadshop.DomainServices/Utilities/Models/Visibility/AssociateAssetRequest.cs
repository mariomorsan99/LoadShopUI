namespace Loadshop.DomainServices.Utilities.Models.Visibility
{
    public class AssociateAssetRequest
    {
        public string LoadId { get; set; }
        public string AssetId { get; set; }        
        public string LoadNumber { get; set; }
        public string ShipmentIdNumber { get; set; }
        public bool UpdateSuccess { get; set; }
    }
}
