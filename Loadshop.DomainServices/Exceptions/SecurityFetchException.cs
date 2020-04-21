using System;

namespace Loadshop.DomainServices.Exceptions
{
    public class SecurityFetchException : BaseException
    {
        public SecurityFetchException(string message, Exception ex) : base(message, ex)
        {

        }

        public override int Code => 1042;
    }
}
