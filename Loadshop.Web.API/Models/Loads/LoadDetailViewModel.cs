using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.Web.API.Models.Loads
{
    public class LoadDetailViewModel : LoadData
    {
        public bool ViewOnly { get; set; }
    }
}
