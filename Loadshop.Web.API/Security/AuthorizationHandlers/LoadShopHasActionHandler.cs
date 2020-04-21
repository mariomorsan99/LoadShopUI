using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.Web.API.Security.Exceptions;
using Loadshop.Web.API.Security.Requirments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Security.AuthorizationHandlers
{
    public class LoadShopHasActionHandler : AuthorizationHandler<LoadShopHasActionRequirment>
    {
        private readonly ISecurityService securityService;
        private readonly ILogger<LoadShopHasActionHandler> logger;

        public LoadShopHasActionHandler(ISecurityService securityService, ILogger<LoadShopHasActionHandler> logger)
        {
            this.securityService = securityService;
            this.logger = logger;
        }
        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, LoadShopHasActionRequirment requirement)
        {
            try
            {
                var actionsAsString = string.Join(",", requirement.ActionNames);

                logger.LogInformation($"Validating user {context.User.Identity.Name} has one of the following actions: {actionsAsString}");
                if (await securityService.UserHasActionAsync(requirement.ActionNames))
                {
                    logger.LogInformation($"Validating user has one of the following actions: {actionsAsString} - Successful");
                    context.Succeed(requirement);
                }
                else
                {
                    logger.LogInformation($"Validating user {context.User.Identity.Name} has one of the following actions: {actionsAsString} - Fail");
                    context.Fail();
                }
            }
            catch (SecurityFetchException ex)
            {
                var message = $"Error fetching user {context.User.Identity.Name} security from: {nameof(LoadShopHasActionHandler)}";
                logger.LogError(ex, message);
                throw new LoadShopAuthorizationException(message, ex);
            }
        }
    }
}
