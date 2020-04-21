using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadUpdateScacData
    {
        [Required]
        public string ReferenceLoadId { get; set; }
        public long? LoadBoardId { get; set; }
        public List<LoadCarrierScacData> CarrierScacs { get; set; } = new List<LoadCarrierScacData>();
    }
}
