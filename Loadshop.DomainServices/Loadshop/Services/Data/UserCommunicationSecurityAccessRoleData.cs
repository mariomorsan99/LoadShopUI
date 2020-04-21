using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserCommunicationSecurityAccessRoleData : BaseData
    {
        public Guid? UserCommunicationSecurityAccessRoleId { get; set; }
        public Guid? UserCommunicationId { get; set; }
        public Guid AccessRoleId { get; set; }
    }
}
