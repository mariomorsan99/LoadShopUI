using System;

namespace Loadshop.Web.API.Security.Exceptions
{
    public class LoadShopAuthorizationException : Exception
    {
        public LoadShopAuthorizationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
