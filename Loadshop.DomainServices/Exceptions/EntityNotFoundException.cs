using System;

namespace Loadshop.DomainServices.Exceptions
{
    public class EntityNotFoundException : BaseException
    {
        public EntityNotFoundException(string message) : base(message)
        {
        }
        public EntityNotFoundException(string message, Exception ex) : base(message, ex)
        {
        }
        public override int Code => 1030;
    }
}
