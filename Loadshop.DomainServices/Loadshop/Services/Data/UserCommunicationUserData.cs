using System;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class UserCommunicationUserData : BaseData
    {
        public Guid? UserCommunicationUserId { get; set; }
        public Guid? UserCommunicationId { get; set; }
        public Guid UserId { get; set; }
    }
}
