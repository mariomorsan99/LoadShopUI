using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Web.Filters
{
    public class CustomerTransactionLoggingFilter : ActionFilterAttribute
    {
        private readonly ICustomerTransactionLogService _logService;
        private readonly IUserContext _userContext;

        public CustomerTransactionLoggingFilter(ICustomerTransactionLogService logService, IUserContext userContext)
        {
            _logService = logService;
            _userContext = userContext;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (_userContext.UserId.HasValue && filterContext.Exception == null)
            {
                var requestUri = filterContext.HttpContext.Request.Scheme.ToString() + "://"
                               + filterContext.HttpContext.Request.Host.ToString()
                               + filterContext.HttpContext.Request.Path.ToString()
                               + filterContext.HttpContext.Request.QueryString.ToString();

                string request = "[]";
                if (filterContext.HttpContext.Request.ContentLength != null && filterContext.HttpContext.Request.ContentLength > 0)
                {
                    filterContext.HttpContext.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);
                    using (var reader = new System.IO.StreamReader(filterContext.HttpContext.Request.Body))
                    {
                        request = reader.ReadToEnd();
                    }
                }

                var response = JsonConvert.SerializeObject(filterContext.Result);

                _logService.LogTransaction(_userContext.UserId.Value, requestUri, request, response);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
