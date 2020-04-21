using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{   

    public class UserShipperData : BaseData
    {
        [Key]
        public Guid UserShipperId { get; set; }
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Guid UserId { get; set; }

        public UserProfileData User { get; set; }
        public virtual CustomerData Customer { get; set; }
    }
}
