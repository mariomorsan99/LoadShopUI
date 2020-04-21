using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadCarrierScacData
    {
        [Required]
        public string Scac { get; set; }
        public decimal? ContractRate { get; set; }
    }
}

