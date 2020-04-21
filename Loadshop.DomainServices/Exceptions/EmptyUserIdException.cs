using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Exceptions
{
    public class EmptyUserIdException : BaseException
    {
        public IList<string> ErrorMessages { get; set; }

        public EmptyUserIdException(string message) : base(message)
        {
        }

        public EmptyUserIdException(string message, IList<string> errors) : base(message)
        {
            this.ErrorMessages = errors;
        }

        public EmptyUserIdException(string message, Exception ex) : base(message, ex)
        {
        }

        public override int Code => 1050;
    }
}
