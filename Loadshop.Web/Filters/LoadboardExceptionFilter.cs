using Loadshop.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using Newtonsoft.Json;
using Loadshop.DomainServices.Exceptions;
using Loadshop.API.Models;
using Loadshop.DomainServices.Security;

namespace Loadshop.Web.Filters
{
    public class LoadshopExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<LoadshopExceptionFilter> _logger;
        private readonly IConfigurationRoot _config;
        private readonly IUserContext _userContext;

        public LoadshopExceptionFilter(IHostingEnvironment hostingEnvironment, ILogger<LoadshopExceptionFilter> logger, IConfigurationRoot config, IUserContext userContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            _config = config;
            _userContext = userContext;
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            var exceptionType = context.Exception.GetType();
            var errorKey = Guid.NewGuid().ToString().ToUpper();
            var response = new ResponseMessage<object>();

            if (exceptionType != typeof(EmptyUserIdException) && exceptionType != typeof(ValidationException))
            {
                var modelState = JsonConvert.SerializeObject(context.ModelState.Values);
                var routeData = JsonConvert.SerializeObject(context.RouteData.Values);
                var requestURL = context.HttpContext.Request.Scheme.ToString() + "://"
                               + context.HttpContext.Request.Host.ToString()
                               + context.HttpContext.Request.Path.ToString()
                               + context.HttpContext.Request.QueryString.ToString();

                string bodyAsText = "[]";

                if (context.HttpContext.Request.ContentLength != null && context.HttpContext.Request.ContentLength > 0)
                {
                    context.HttpContext.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);
                    using (var reader = new System.IO.StreamReader(context.HttpContext.Request.Body))
                    {
                        bodyAsText = await reader.ReadToEndAsync();
                    }
                }

                string requestUserId = _userContext?.UserId?.ToString().ToUpper() ?? null;
                _logger.LogError(context.Exception, "{ExceptionMessage} [{ErrorGuid}, {RequestURL}, {RequestBody}, {ModelState}, {RouteData}, {RequestUserId}] ", context.Exception.Message, errorKey, requestURL, bodyAsText, modelState, routeData, requestUserId);
            }

            if (exceptionType != typeof(EmptyUserIdException))
            {
                var displayMessage = _config.GetValue<string>("GenericErrorMessage");

                if (string.IsNullOrWhiteSpace(displayMessage))
                {
                    displayMessage = "An error has occurred. Error reference number: ";
                }

                context.Exception = new Exception($"{displayMessage}{ errorKey }");
                response.AddError(context.Exception, false);
            }

            context.Result = new OkObjectResult(response);
            await base.OnExceptionAsync(context);
        }
    }
}
