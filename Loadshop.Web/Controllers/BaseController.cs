using Loadshop.API.Models;
using Loadshop.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Loadshop.Web.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult Success<T>(T returnObject)
        {
            if (returnObject != null &&
                returnObject.GetType().IsGenericType &&
                returnObject.GetType().GetGenericTypeDefinition() == typeof(ResponseMessage<string>).GetGenericTypeDefinition())
            {
                return Ok(returnObject);
            }

            var response = new ResponseMessage<T> { Data = returnObject };
            return Ok(response);
        }

        protected IActionResult Error<T>(Exception ex)
        {
            var response = new ResponseMessage<T>();
            response.AddError(ex);
            return Ok(response);
        }

        protected IActionResult Error<T>(string message)
        {
            var response = new ResponseMessage<T>();
            response.AddError(message);
            return Ok(response);
        }

        protected IActionResult Error<T>(string message, int code)
        {
            var response = new ResponseMessage<T>();
            response.AddError(code, message);
            return Ok(response);
        }

        protected IActionResult Forbidden<T>(string message)
        {
            var response = new ResponseMessage<T>();
            response.AddError(message);
            ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Ok(response);
        }

        protected IActionResult Forbidden<T>(UnauthorizedAccessException ex)
        {
            var response = new ResponseMessage<T>();
            response.AddError(ex);
            ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Ok(response);
        }

        public ActionResult BadRequest<T>(ResponseMessage<T> response, ModelStateDictionary modelState)
        {
            foreach (var modelStateValue in modelState)
            {
                foreach (var error in modelStateValue.Value.Errors)
                {
                    response.Errors.Add(new ResponseError
                    {
                        Data = modelStateValue.Key,
                        Message = error.ErrorMessage
                    });
                }
            }

            return base.BadRequest(response);
        }
    }
}
