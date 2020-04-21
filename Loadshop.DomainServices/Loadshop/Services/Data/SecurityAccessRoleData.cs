using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class SecurityAccessRoleData : SecurityAccessRoleListData
    {
        public List<SecurityAppActionData> AppActions { get; set; }
    }
}
