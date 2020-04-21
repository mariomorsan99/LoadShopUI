using Loadshop.DomainServices.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TMS.Infrastructure.Hosting.Web.Models;

namespace Loadshop.Web.Models
{
    public class CustomerUserContext : IUserContext
    {
        private IEnumerable<string> _claims;
        private IConfiguration _configuration;
        private IHttpContextAccessor _httpContextAccessor;

        public CustomerUserContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            var user = httpContextAccessor.HttpContext.User;
            if (user.Identity.IsAuthenticated)
            {
                var claims = user.Claims;

                //retrieve userData
                var userData = claims
                    .Where(c => c.Type == ClaimTypes.UserData && !string.IsNullOrEmpty(c.Value))
                    .Select(c => JsonConvert.DeserializeObject<ApplicationUser>(c.Value))
                    .FirstOrDefault();

                //set user info
                UserId = Guid.Parse(userData.Id);
                UserName = userData.UserName;
                FirstName = userData.FirstName;
                LastName = userData.LastName;
                Email = userData.Email;
                Company = userData.Company;
                //retrieve user claims
                _claims = claims.Select(c => c.Value);
            }
        }

        public string ApplicationCode
        {
            get
            {
                return "LB";
            }
        }

        public Guid? UserId { get; }
        public string UserName { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        //public string Role { get; }
        public string Company { get; }
        public string Token { get; }

        public bool HasClaim(string claim)
        {

            return _claims != null && _claims.Contains(claim);
        }
    }
}
