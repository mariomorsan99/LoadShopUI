using Microsoft.AspNetCore.Authorization;

namespace Loadshop.Web.API.Security.Requirments
{
    public class LoadShopHasActionRequirment : IAuthorizationRequirement
    {
        public LoadShopHasActionRequirment(params string[] actionNames)
        {
            ActionNames = actionNames;
        }
        public string[] ActionNames { get; }
    }
}
