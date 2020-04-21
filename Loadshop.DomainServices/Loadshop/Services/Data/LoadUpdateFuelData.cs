using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadUpdateFuelData
    {
        [Required]
        public string ReferenceLoadId { get; set; }
        public decimal FuelRate { get; set; }
    }
}
