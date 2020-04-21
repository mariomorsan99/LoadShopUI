using System;
using System.Net.Http;
using DocumentService.SDK;
using FeedbackService.SDK;
using Ganss.XSS;
using Loadshop.DomainServices.Common;
using Loadshop.DomainServices.Common.Cache;
using Loadshop.DomainServices.Common.DataProvider;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Constants;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Proxy.Visibility;
using Loadshop.DomainServices.Proxy.Visibility.Interfaces;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Validation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.Infrastructure.Common.Configuration;
using TMS.Infrastructure.Messaging.Client;
using Loadshop.DomainServices.Loadshop.Services.Utility;
using Loadshop.DomainServices.Proxy.Tops.Loadshop;
using TMS.Infrastructure.Messaging.Client.HttpAuthentication;
using Loadshop.DomainServices.Loadshop.Services.Repositories;

namespace Loadshop.DomainServices.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureLoadshopServices(this IServiceCollection container, IConfiguration configuration)
        {
            //register db context
            container.AddDbContext<LoadshopDataContext>(options => options.UseSqlServer(configuration.GetValue<string>("LoadBoardSQLServer"), opts => opts.EnableRetryOnFailure()));
            container.AddDbContext<TopsDataContext>(options => options.UseSqlServer(configuration.CommonSettings().TopsDbConnection, opts => opts.EnableRetryOnFailure()));
            container.AddDbContext<StateDataContext>(options => options.UseSqlServer(configuration.CommonSettings().TopsDbConnection, opts => opts.EnableRetryOnFailure()));

            //container.AddTransient<Authentication.Domaincontainer.Security.container.ITopsUserService, Authentication.Domaincontainer.Security.container.TopsUserService>();
            container.AddTransient<INotificationService, NotificationService>();
            container.AddTransient<IUserProfileService, UserProfileService>();
            container.AddTransient<IUserLaneService, UserLaneService>();
            container.AddTransient<ILoadService, LoadService>();
            container.AddTransient<ICommonService, CommonService>();
            container.AddTransient<ICarrierService, CarrierService>();
            container.AddTransient<IEquipmentService, EquipmentService>();
            container.AddTransient<ISMSService, SMSService>();
            container.AddTransient<IEmailService, EmailService>();
            container.AddTransient<ILoadCarrierGroupService, LoadCarrierGroupService>();
            container.AddTransient<ILoadValidationService, LoadValidationService>();
            container.AddTransient<ICommodityService, CommodityService>();
            container.AddTransient<ICustomerService, CustomerService>();
            container.AddTransient<IShippingService, ShippingService>();
            container.AddTransient<ISecurityService, SecurityService>();
            container.AddTransient<IShipperAdminService, ShipperAdminService>();
            container.AddTransient<IUserAdminService, UserAdminService>();
            container.AddTransient<ILoadStopService, LoadStopService>();
            container.AddTransient<ILocationService, LocationService>();
            container.AddTransient<ITransportationService, TransportationService>();
            container.AddTransient<IUnitOfMeasureService, UnitOfMeasureService>();
            container.AddTransient<IServiceTypeService, ServiceTypeService>();
            container.AddTransient<ISpecialInstructionsService, SpecialInstructionsService>();
            container.AddTransient<ICustomerLoadTypeService, CustomerLoadTypeService>();
            container.AddTransient<ICarrierAdminService, CarrierAdminService>();
            container.AddTransient<IRatingService, RatingService>();
            container.AddTransient<IUserCommunicationService, UserCommunicationService>();
            container.AddTransient<ILoadStatusTransactionService, LoadStatusTransactionService>();
            container.AddTransient<IAgreementDocumentService, AgreementDocumentService>();
            container.AddTransient<ILoadshopDocumentService, LoadshopDocumentService>();
            container.AddTransient<ISmartSpotPriceService, SmartSpotPriceService>();
            container.AddTransient(_ => new SmartSpotPriceConfig
            {
                ApiUrl = configuration.GetValue<string>("SmartSpotPriceAPIUrl"),
                AccessKeyId = configuration.GetValue<string>("SmartSpotPriceAWSAccessKeyId"),
                SecretAccessKey = configuration.GetValue<string>("SmartSpotPriceAWSSecretAccessKey"),
                Service = configuration.GetValue<string>("SmartSpotPriceAWSService"),
                Region = configuration.GetValue<string>("SmartSpotPriceAWSRegion"),
            });
            container.AddTransient<IRecaptchaService, RecaptchaService>();
            container.AddTransient<IDateTimeProvider, DateTimeProvider>();
            container.AddSingleton<IHtmlSanitizer, HtmlSanitizer>(x => new HtmlSanitizer());
            container.AddTransient<ServiceUtilities>();
            container.AddTransient<ILoadQueryRepository, LoadQueryRepository>();

            //httpClient Services
            container.AddHttpClient();
            container.AddHttpClient<IAddressValidationService, AddressValidationService>();
            container.AddHttpClient<ICarrierWebAPIService, CarrierWebAPIService>();
            container.AddHttpClient<IMileageService, MileageService>();
            container.AddHttpClient<IVisibilityProxyService, VisibilityProxyService>();
            container.AddHttpClient<ITopsLoadshopApiService, TopsLoadshopApiService>();
            container.AddHttpClient<ISmartSpotPriceService, SmartSpotPriceService>();
            container.AddHttpClient<IRecaptchaService, RecaptchaService>(x =>
            {
#if !DEBUG
                    x = new HttpClient(new ProxiedHttpMessageHandler(configuration));                
#endif
            });
            container.AddHttpClient<Utilities.Utilities>();

            container.AddSingleton(x =>
            {
                return (IConfigurationRoot)configuration;
            });

            //Register Cache Managers
            container.AddTransient<LoadShopCacheManager>();
            container.AddTransient<UserCacheManager>();

            //register container
            container.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            container.AddTransient<ICustomerTransactionLogService, CustomerTransactionLogService>();

            // add document services
            container.AddDocumentServiceApi(configuration["DocumentAppURL"],
                DocumentServiceConstants.AppCode,
                (AuthenticationOptions options) =>
                {
                    options.UseClientCredentialsAuthorization(
                        "documentapi",
                        configuration["IdentityServerURL"],
                        configuration["DocumentAppClientId"],
                        configuration["DocumentAppClientSecret"],
                        configuration["DocumentAppScope"]
                        );
                });

            container.AddFeedbackService(
                new Uri(configuration["FeedbackServiceUrl"]),
                (ClientCredentialsAuthorizationOptions options) =>
                {
                    options.AuthorizationName = "feedbackapi";
                    options.AuthProviderUrl = configuration["IdentityServerURL"];
                    options.ClientId = configuration["LoadshopClientId"];
                    options.ClientSecret = configuration["LoadshopClientSecret"];
                    options.Scopes = configuration["LoadshopClientScopes"];
                });
            container.AddTransient<IFeedbackIntegrationService, FeedbackIntegrationService>();
            container.AddTransient<ILoadshopFeeService, LoadshopFeeService>();
        }
    }
}
