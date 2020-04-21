using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserData : BaseData
    {
        public Guid? UserId { get; set; }
        public Guid IdentUserId { get; set; }
        [StringLength(4)]
        public bool IsNotificationsEnabled { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [StringLength(4)]
        public string PrimaryScac { get; set; }
        public Guid? PrimaryCustomerId { get; set; }

        //Identity Properties
        public string Email { get; set; }
        public string CompanyName { get; set; }

        public List<Guid> SecurityRoleIds { get; set; }
        public List<SecurityAccessRoleData> SecurityRoles { get; set; }
        public List<Guid> ShipperIds { get; set; }
        public List<string> CarrierScacs { get; set; }
        public List<UserNotificationData> UserNotifications { get; set; }
    }
}
