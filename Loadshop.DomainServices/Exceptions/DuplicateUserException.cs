using System;

namespace Loadshop.DomainServices.Exceptions
{
    public class DuplicateUserException : BaseException
    {
        public DuplicateUserException(string message) : base(message)
        {
        }
        public DuplicateUserException(string message, Exception ex) : base(message, ex)
        {
        }
        public override int Code => 1031;
    }
}
