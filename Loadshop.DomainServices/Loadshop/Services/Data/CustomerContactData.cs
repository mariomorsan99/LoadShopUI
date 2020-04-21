using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class CustomerContactData
    {
        public Guid? CustomerContactId { get; set; }

        public Guid? CustomerId { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Position { get; set; }

        [MaxLength(100)]
        public string PhoneNumber { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }
    }
}
