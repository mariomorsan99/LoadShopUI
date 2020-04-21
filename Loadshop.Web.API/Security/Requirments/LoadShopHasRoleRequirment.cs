using Microsoft.AspNetCore.Authorization;

namespace Loadshop.Web.API.Security.Requirments
{
    public class LoadShopHasRoleRequirment : IAuthorizationRequirement
    {
        public LoadShopHasRoleRequirment(params string [] roleNames)
        {
            RoleNames = roleNames;
        }

        public string[] RoleNames { get; }
    }
}
