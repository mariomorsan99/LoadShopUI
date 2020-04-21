using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserCarrierScacData : BaseData
    {
        [Key]
        public Guid UserCarrierScacId { get; set; }
        public string UserId { get; set; }
        [Required]
        [StringLength(20)]
        public string CarrierId { get; set; }
        [StringLength(4)]
        public string Scac { get; set; }

    }
}
