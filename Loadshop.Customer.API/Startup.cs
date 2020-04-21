using AutoMapper;
using Loadshop.Web.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSwag.AspNetCore;
using Loadshop.Web.Models;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Mvc;
using Loadshop.DomainServices.Extensions;
using TMS.Infrastructure.Common.Configuration;
using TMS.Infrastructure.Hosting.Web.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TMS.Infrastructure.WebApi.Security;

namespace Loadshop.Customer.API
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

            services.AddTransient<DomainServices.Security.IUserContext, CustomerUserContext>();

            services.AddAutoMapper();

            services
                .AddMvcCore()
                .AddApiExplorer()
                .AddMvcOptions(options =>
                {
                    options.Filters.Add(typeof(CustomerTransactionLoggingFilter));
                    options.Filters.Add(typeof(LoadshopExceptionFilter));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddJsonFormatters()
                .AddAuthorization();

            services.AddWebEncoders();
            services.AddCors();
            services.AddDistributedMemoryCache();

            var connection = Configuration.CommonSettings().ConfigDbConnection;
            // Add framework services.
            services.AddDbContext<DbContext, ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connection);
            });
            services.TryAddScoped<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
            services.TryAddScoped<IRoleStore<IdentityRole>, RoleStore<IdentityRole>>();

            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityBasic<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(o =>
                {
                    o.Realm = Configuration.CommonSettings().ProgramId;
                });
            //.AddIdentityBasic<CustomerUserContext, object>(o =>
            //{
            //    o.Realm = "Loadshop.Customer.API";
            //});

            services.AddSwagger();

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });
            services.AddLazyCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            app.UseStaticFiles();

            app.UseSwaggerUiWithApiExplorer(settings =>
            {
                settings.SwaggerRoute = "/swagger/v1/swagger.json";
                settings.SwaggerUiRoute = "/swaggerui";
                settings.GeneratorSettings.Title = "KBXL GoDirect API";
            });

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("WWW-Authenticate"));

            app.UseAuthentication();
            app.UseMvc();

            logger.LogInformation("Startup Complete");

        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("ident");

            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
