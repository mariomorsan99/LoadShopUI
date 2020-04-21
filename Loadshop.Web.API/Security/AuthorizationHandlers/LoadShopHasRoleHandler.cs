using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.API.Security.Exceptions;
using Loadshop.Web.API.Security.Requirments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Security.AuthorizationHandlers
{
    public class LoadShopHasRoleHandler : AuthorizationHandler<LoadShopHasRoleRequirment>
    {
        private readonly ISecurityService securityService;
        private readonly ILogger<LoadShopHasRoleHandler> logger;

        public LoadShopHasRoleHandler(ISecurityService securityService, ILogger<LoadShopHasRoleHandler> logger)
        {
            this.securityService = securityService;
            this.logger = logger;
        }
        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, LoadShopHasRoleRequirment requirement)
        {
            try
            {
                var rolesAsString = string.Join(",", requirement.RoleNames);

                logger.LogInformation($"Validating user {context.User.Identity.Name} has one of the following roles: {rolesAsString}");
                if (requirement.RoleNames.Any())
                {
                    if (await securityService.UserHasRoleAsync(requirement.RoleNames))
                    {
                        logger.LogInformation($"Validating user has one of the following roles: {rolesAsString} - Success");
                        context.Succeed(requirement);
                    }
                    else
                    {
                        logger.LogInformation($"Validating user {context.User.Identity.Name} has one of the following roles: {rolesAsString} - Fail");
                        context.Fail();
                    }
                }
                else
                {
                    logger.LogInformation($"Validating user {context.User.Identity.Name} has any role");
                    if ((await securityService.GetUserRolesAsync()).Any())
                    {
                        logger.LogInformation($"Validating user {context.User.Identity.Name} has any role - Success");
                        context.Succeed(requirement);
                    }
                    else
                    {
                        logger.LogInformation($"Validating user {context.User.Identity.Name} has any role - Fail");
                        context.Fail();
                    }
                }

            }
            catch (SecurityFetchException ex)
            {
                var message = $"Error fetching user {context.User.Identity.Name} security from: {nameof(LoadShopHasRoleHandler)}";
                logger.LogError(ex, message);
                throw new LoadShopAuthorizationException(message, ex);
            }
        }
    }
}
