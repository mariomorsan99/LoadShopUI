using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserCommunicationDetailData : UserCommunicationData
    {
        public List<UserCommunicationUserData> UserCommunicationUsers { get; set; }
        public List<UserCommunicationShipperData> UserCommunicationShippers { get; set; }
        public List<UserCommunicationCarrierData> UserCommunicationCarriers { get; set; }
        public List<UserCommunicationSecurityAccessRoleData> UserCommunicationSecurityAccessRoles { get; set; }
    }
}
