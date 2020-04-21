using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserCommunicationData : BaseData
    {
        public Guid? UserCommunicationId { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public Guid? OwnerId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool AllUsers { get; set; }
        public bool AcknowledgementRequired { get; set; }

        public int AcknowledgementCount { get; set; }
    }
}
