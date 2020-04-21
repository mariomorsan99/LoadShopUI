using Loadshop.DomainServices.Loadshop.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Security
{
    public static class SecurityExtensions
    {
        public static bool IsAuthorizedForCustomer(this ISecurityService securityService, Guid customerId)
        {
            return securityService.GetAuthorizedCustomersforUser().Any(customer => customer.CustomerId == customerId);
        }

        public static async Task<bool> IsAuthorizedForCustomerAsync(this ISecurityService securityService, Guid customerId)
        {
            return (await securityService.GetAuthorizedCustomersforUserAsync()).Any(customer => customer.CustomerId == customerId);
        }

        public static async Task<bool> IsAdminAsync(this ISecurityService securityService)
        {
            return await securityService.UserHasRoleAsync(SecurityRoles.AdminRoles);
        }

        public static async Task<bool> IsDedicatedAsync(this ISecurityService securityService)
        {
            return await securityService.UserHasRoleAsync(SecurityRoles.DedicatedRoles);
        }

        public static bool IsAdmin(this ISecurityService securityService)
        {
            return securityService.UserHasRole(SecurityRoles.AdminRoles);
        }

        public static async Task GuardActionAsync(this ISecurityService securityService, params string[] actions)
        {
            var userHasAtleastOneAction = await securityService.UserHasActionAsync(actions);
            if (!userHasAtleastOneAction)
                throw new UnauthorizedAccessException($"User does not have one of the specified action(s): {string.Join(", ", actions)}");
        }

        public static void GuardAction(this ISecurityService securityService, params string[] actions)
        {
            var userHasAtleastOneAction = securityService.UserHasAction(actions);
            if (!userHasAtleastOneAction)
                throw new UnauthorizedAccessException($"User does not have one of the specified action(s): {string.Join(", ", actions)}");
        }
    }
}
