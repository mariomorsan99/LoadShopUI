using Loadshop.DomainServices.Security;
using System;
using System.Collections.Generic;

namespace Loadshop.Tests.DomainServices.Models
{
    public class MockUserContext : IUserContext
    {
        public List<string> Claims { get; set; }

        public Guid? UserId { get; set; } = Guid.Parse("472F871B-FA4D-494C-A723-B2D3EB8FB7F2");

        public string UserName { get; set; } = "CHARLIE";

        public string FirstName { get; set; } = "CHARLIE";

        public string LastName { get; set; } = "CHARLIE";

        public string Email { get; set; } = "charlie.charlie@kbxlogistics.com";

        public string Role { get; set; } = "";

        public string ApplicationCode { get; set; } = "LB";

        public string Company { get; set; } = "KBX LOGISTICS, LLC";

        public string Token { get; set; }

        public bool HasClaim(string claim)
        {
            return Claims.Contains(claim);
        }

    }
}
