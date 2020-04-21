using Loadshop.API.Models;
using Loadshop.DomainServices.Exceptions;
using System;

namespace Loadshop.Web.Models
{
    public static class ResponseMessageExtensions
    {
        public static void AddError<T>(this ResponseMessage<T> msg, Exception ex, bool includeStackTrace = false)
        {
            string exceptionMessage = string.Empty;
            string stackTrace = ex.StackTrace;
            if (ex.InnerException == null)
            {
                exceptionMessage = ex.Message;
            }
            else
            {
                exceptionMessage = ex.InnerException.Message;
                stackTrace += ex.InnerException.StackTrace;
            }

            if (ex is ValidationException)
            {
                var validationException = (ValidationException)ex;
                msg.Errors.Add(new ResponseError()
                {
                    Code = validationException.Code,
                    Display = validationException.UserFriendlyMessage,
                    Data = validationException.Data,
                    Message = exceptionMessage,
                    StackTrace = (includeStackTrace ? stackTrace : null)
                });

                if (validationException.ErrorMessages != null)
                {
                    foreach (var errorMessage in validationException.ErrorMessages)
                    {
                        msg.Errors.Add(new ResponseError()
                        {
                            Code = validationException.Code,
                            Display = validationException.UserFriendlyMessage,
                            Message = errorMessage
                        });
                    }
                }
            }
            else if (ex is BaseException)
            {
                var baseException = (BaseException)ex;
                msg.Errors.Add(new ResponseError()
                {
                    Code = baseException.Code,
                    Display = baseException.UserFriendlyMessage,
                    Data = baseException.Data,
                    Message = exceptionMessage,
                    StackTrace = (includeStackTrace ? stackTrace : null)
                });
            }
            else
            {
                msg.Errors.Add(new ResponseError()
                {
                    Message = exceptionMessage,
                    StackTrace = (includeStackTrace ? stackTrace : null)
                });
            }
        }

        public static void AddError<T>(this ResponseMessage<T> msg, string message)
        {
            msg.Errors.Add(new ResponseError()
            {
                Message = message,
                Display = true
            });
        }

        public static void AddError<T>(this ResponseMessage<T> msg, int code, string message)
        {
            msg.Errors.Add(new ResponseError()
            {
                Code = code,
                Display = true,
                Message = message
            });
        }

        public static void AddError<T>(this ResponseMessage<T> msg, ResponseError error)
        {
            msg.Errors.Add(error);
        }
    }
}
