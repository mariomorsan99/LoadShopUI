using System.ComponentModel.DataAnnotations;


namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class LoadStopContactData
    {
        public long LoadStopContactId { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
        [MaxLength(100)]
        public string PhoneNumber { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
    }
}
