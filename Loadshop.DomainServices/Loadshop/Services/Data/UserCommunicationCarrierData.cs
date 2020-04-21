using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserCommunicationCarrierData : BaseData
    {
        public Guid? UserCommunicationCarrierId { get; set; }
        public Guid? UserCommunicationId { get; set; }
        [Required]
        [StringLength(20)]
        public string CarrierId { get; set; }
    }
}
