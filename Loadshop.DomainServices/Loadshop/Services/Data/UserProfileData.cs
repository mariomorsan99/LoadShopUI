using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserProfileData
    {
        public Guid UserId { get; set; }
        public Guid IdentUserId { get; set; }
        public string Name { get; set; }
        public string CarrierName { get; set; }
        [Obsolete("CarrierScac is obsolete PrimaryScac should be used instead")]
        public string CarrierScac {
            get { return PrimaryScac; }
            set { PrimaryScac = value; }
        }

        public string PrimaryScac { get; set; }
        public Guid? PrimaryCustomerId { get; set; }
        public bool IsNotificationsEnabled { get; set; }
        public List<UserNotificationData> UserNotifications { get; set; }

        public List<string> AvailableCarrierScacs { get; set; } = new List<string>();

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool IsShipper { get; set; }
        public bool IsCarrier { get; set; }
        public bool IsAdmin { get; set; }

        public UserFocusEntityData FocusEntity { get; set; }

        public List<string> CarrierVisibilityTypes { get; set; }

        public List<SecurityAccessRoleData> SecurityAccessRoles { get; set; } = new List<SecurityAccessRoleData>();

        public List<CustomerData> AuthorizedShippers { get; set; } = new List<CustomerData>();

        public List<CustomerData> AuthorizedShippersForMyPrimaryScac { get; set; } = new List<CustomerData>();

        public List<string> MyCustomerContractedScacs { get; set; } = new List<string>();

        public string DefaultCommodity { get; set; }

        public bool CanSetDefaultCommodity { get; set; } = false;

        /// <summary>
        /// Flag to indicate if user agree to both terms and privacy
        /// </summary>
        public bool HasAgreedToTerms { get; set; }
        public bool AllowManualLoadCreation { get; set; }
    }
}
