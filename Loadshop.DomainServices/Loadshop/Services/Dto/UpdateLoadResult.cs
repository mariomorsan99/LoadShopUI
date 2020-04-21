namespace Loadshop.DomainServices.Loadshop.Services.Dto
{
    public class UpdateLoadResult
    {
        public bool SavedSuccessfully { get; set; }
        public bool LoadKeptInMarketplace { get; set; }
        public string ErrorMessage { get; set; }
    }
}
