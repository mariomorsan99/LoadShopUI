using System;

namespace Loadshop.DomainServices.Exceptions
{
    public abstract class BaseException : Exception
    {
        public object ReturnObject { get; set; }

        public abstract int Code { get; }

        public bool UserFriendlyMessage { get; set; }

        public BaseException(string message) : base(message)
        {
        }

        public BaseException(string message, object returnObject) : base(message)
        {
            this.ReturnObject = returnObject;
        }

        public BaseException(string message, Exception ex) : base(message, ex)
        {
        }

        public BaseException(string message, Exception ex, object returnObject) : base(message, ex)
        {
            this.ReturnObject = returnObject;
        }
    }
}
