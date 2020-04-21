using System;
using System.ComponentModel.DataAnnotations;


namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadContactData
    {
        public Guid? LoadContactId { get; set; }
        [Required]
        public string Display { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
    }
}
