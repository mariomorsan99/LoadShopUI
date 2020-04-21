using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Exceptions
{
    public class ValidationException : BaseException
    {
        public IList<string> ErrorMessages { get; set; }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, IList<string> errors) : base(message)
        {
            this.ErrorMessages = errors;
        }

        public ValidationException(string message, Exception ex) : base(message, ex)
        {
        }

        public override int Code => 1040;
    }
}
