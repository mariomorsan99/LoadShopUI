using System;
using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Common.Cache.Models
{
    public class UserCache
    {
        public Guid UserId { get; set; }
        public List<SecurityAccessRoleData> UserSecurityAccessRoles { get; set; }
        public List<string> UserSecurityAppActions { get; set; }
        public IReadOnlyCollection<UserFocusEntityData> AuthorizedEntities { get; set; }
    }
}
