using Loadshop.DomainServices.Loadshop.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.API.Security
{
    public static class LoadBoardSecurityExtensions
    {
        public static IMvcCoreBuilder AddLoadBoardAuthorization(this IMvcCoreBuilder coreBuilder)
        {

            //Use reflection to grab roles and actions
            //Don't need role policies as we can use the Roles property on the Authorize Attribute
            //var roleConstants = typeof(DomainServices.Security.SecurityRoles)
            //                                .GetFields(System.Reflection.BindingFlags.Public
            //                                    | System.Reflection.BindingFlags.Static
            //                                    | System.Reflection.BindingFlags.FlattenHierarchy)
            //                                .Select(field => field.GetValue(null).ToString()).ToList();
            var actionConstants = typeof(DomainServices.Security.SecurityActions)
                                            .GetFields(System.Reflection.BindingFlags.Public
                                                | System.Reflection.BindingFlags.Static
                                                | System.Reflection.BindingFlags.FlattenHierarchy)
                                            .Select(field => field.GetValue(null).ToString()).ToList();

            //Loop through Roles and Actions to create policy for each
            coreBuilder.AddAuthorization(options =>
            {
                //Add Policy for all roles
                //foreach (var role in roleConstants)
                //    options.AddPolicy($"{AuthorizationConstants.RolePolicyPrefix}{role}", policy => policy.AddRequirements(new Requirments.LoadShopHasRoleRequirment(role)));

                //Add Policy for all actions
                foreach (var action in actionConstants)
                    options.AddPolicy($"{AuthorizationConstants.ActionPolicyPrefix}{action}", policy => policy.AddRequirements(new Requirments.LoadShopHasActionRequirment(action)));

                //Add policy to check if user has any load shop role
                options.AddPolicy(AuthorizationConstants.HasAnyLoadShopRolePolicy, policy => policy.AddRequirements(new Requirments.LoadShopHasRoleRequirment()));

                //Add legacy Polcies to LoadShop still works until replace with
                //options.AddPolicy("TOPSRoleAdmin", policy =>
                //{
                //    policy.Requirements.Add(new TrnWebApiCore.TopsAuth.Security.TOPSRoleRequirement("LB", new[] { "SYSADMIN" }));
                //});

                options.AddPolicy(AuthorizationConstants.IsCarrierPolicy, policy =>
                {
                    policy.AddRequirements(new Requirments.LoadShopHasRoleRequirment(DomainServices.Security.SecurityRoles.CarrierRoles));
                });

                options.AddPolicy(AuthorizationConstants.IsShipperPolicy, policy =>
                {
                    policy.AddRequirements(new Requirments.LoadShopHasRoleRequirment(DomainServices.Security.SecurityRoles.ShipperRoles));
                });

                options.AddPolicy(AuthorizationConstants.IsCarrierOrShipperPolicy, policy =>
                {
                    policy.AddRequirements(new Requirments.LoadShopHasRoleRequirment(
                       DomainServices.Security.SecurityRoles.CarrierRoles
                        .Union(DomainServices.Security.SecurityRoles.ShipperRoles)
                        .ToArray()));
                });

                options.AddPolicy(AuthorizationConstants.IsCarrierOrShipperAdmin, policy =>
                {
                    policy.AddRequirements(new Requirments.LoadShopHasRoleRequirment(
                        DomainServices.Security.SecurityRoles.LSAdmin,
                        DomainServices.Security.SecurityRoles.SystemAdmin,
                        DomainServices.Security.SecurityRoles.CarrierAdmin,
                        DomainServices.Security.SecurityRoles.ShipperAdmin));
                });

                options.AddPolicy(AuthorizationConstants.CanAccessCarrierLoadsPolicy, policy =>
                {
                    policy.AddRequirements(new Requirments.LoadShopHasActionRequirment(
                        DomainServices.Security.SecurityActions.Loadshop_Ui_Marketplace_Loads_View,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_My_Loads_View_Booked));
                });

                options.AddPolicy(AuthorizationConstants.CanAccessLoadDetail, policy =>
                {
                    policy.AddRequirements(new Requirments.LoadShopHasActionRequirment(
                        DomainServices.Security.SecurityActions.Loadshop_Ui_Marketplace_Loads_View_Detail,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_My_Loads_View_Booked_Detail,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_Carrier_My_Loads_View_Delivered_Detail,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_Shopit_Load_View_Posted_Detail,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_Shopit_Load_View_Booked_Detail,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_Shopit_Load_View_Delivered_Detail));
                });

                options.AddPolicy(AuthorizationConstants.CanAccessLoadStatus, policy =>
                {
                    policy.AddRequirements(new Requirments.LoadShopHasActionRequirment(
                        DomainServices.Security.SecurityActions.Loadshop_Ui_My_Loads_Status_View,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_My_Loads_Status_Update,
                        DomainServices.Security.SecurityActions.Loadshop_Ui_Shopit_Load_View_Booked_Detail));
                });
            });

            coreBuilder.Services.AddTransient<IAuthorizationHandler, AuthorizationHandlers.LoadShopHasRoleHandler>();
            coreBuilder.Services.AddTransient<IAuthorizationHandler, AuthorizationHandlers.LoadShopHasActionHandler>();

            //Register legacy TopsRoleHandler
            //coreBuilder.Services.AddTransient<IAuthorizationHandler, TrnWebApiCore.TopsAuth.Security.TOPSRoleAuthorizationHandler>();

            return coreBuilder;
        }

        public static AuthenticationBuilder AddLoadBoardAuthentication(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            return serviceCollection.AddAuthentication(o =>
             {
                 o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             })
             .AddJwtBearer(o =>
             {
                 o.Audience = "visibilityApi";
                 o.Authority = configuration["IdentityServerUrl"];
                 o.RequireHttpsMetadata = configuration.GetValue("RequireHttpsMetadata", true);
                 o.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateAudience = true,
                     ValidAudiences = new List<string> { "visibilityApi" },
                     ValidateIssuer = true,
                     ValidateIssuerSigningKey = false
                 };
                 o.Events = new JwtBearerEvents()
                 {
                     OnTokenValidated = async context =>
                     {
                         var securityService = context.HttpContext.RequestServices.GetRequiredService<ISecurityService>();

                         var userIdent = context.Principal.Claims
                                                             .Where(c => c.Type == ClaimTypes.NameIdentifier && !string.IsNullOrEmpty(c.Value))
                                                             .Select(c => Guid.Parse(c.Value))
                                                             .FirstOrDefault();

                         securityService.OverrideUserIdentId = userIdent;


                         var roles = await securityService.GetUserRolesAsync();
                         var loadShopIdentity = new ClaimsIdentity();

                         foreach (var role in roles)
                         {
                             loadShopIdentity.AddClaim(new Claim(ClaimTypes.Role, role.AccessRoleName));
                         }

                         context.Principal.AddIdentity(loadShopIdentity);

                         //Reset sense this is a scoped service and we want to use the user context for the service in the domain layer
                         securityService.ResetInit();
                     }
                 };
             });
        }
    }
}
