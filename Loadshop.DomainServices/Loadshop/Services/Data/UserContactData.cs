using System;
using System.Collections.Generic;
using System.Text;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserContactData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        /// <summary>
        /// Users can only have 1 email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Users can have 1... many phone numberss
        /// </summary>
        public IEnumerable<string> PhoneNumbers { get; set; }
        /// <summary>
        /// Users can have 1... many cell phone numbers 
        /// </summary>
        public IEnumerable<string> CellPhoneNumbers { get; set; }
    }
}
