using System;

namespace Loadshop.DomainServices.Security
{
    public interface IUserContext
    {
        Guid? UserId { get; }
        string UserName { get; }
        string FirstName { get; }
        string LastName { get; }
        string Email { get; }
        //string Role { get; }
        string ApplicationCode { get; }
        string Company { get; }
        bool HasClaim(string claim);
        string Token { get; }
    }
}
