using AutoMapper;
using Loadshop.DomainServices.Extensions;
using Loadshop.DomainServices.Security;
using Loadshop.Web.Filters;
using Loadshop.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSwag.AspNetCore;
using Loadshop.Web.API.Security;

namespace Loadshop.Web.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add domain services
            services.ConfigureLoadshopServices(Configuration);
            
            services.AddTransient<IUserContext, WebUserContext>();

            services.AddAutoMapper();

            services
                .AddMvcCore()
                .AddApiExplorer()
                .AddMvcOptions(options =>
                {
                    options.Filters.Add(typeof(LoadshopExceptionFilter));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                })
                .AddJsonFormatters()
                .AddLoadBoardAuthorization();

            services.AddWebEncoders();
            services.AddCors();
            services.AddDistributedMemoryCache();
            services.AddLoadBoardAuthentication(Configuration);
            services.AddSwagger();
            services.AddLazyCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            app.UseStaticFiles();

            app.UseSwaggerUi(c =>
            {

            });
            app.UseSwaggerUiWithApiExplorer(settings =>
            {
                settings.SwaggerRoute = "/swagger/v1/swagger.json";
                settings.SwaggerUiRoute = "/swaggerui";
                settings.GeneratorSettings.Title = "KBXL GoDirect ClientApp API";
            });

            app.UseCors(builder => builder
                //.WithOrigins(
                //                "http://localhost:5000",
                //                "http://localhost:5002")
                .AllowAnyOrigin() // if we want to restrict the origins, comment this out, and use WithOrigins (above)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("WWW-Authenticate"));

            app.UseAuthentication();

            app.UseMvc();

            logger.LogInformation("Startup Complete");

        }
    }
}
