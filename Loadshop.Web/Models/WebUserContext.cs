using Loadshop.DomainServices.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace Loadshop.Web.Models
{
    public class WebUserContext : IUserContext
    {
        private IEnumerable<string> _claims;
        private IConfiguration _configuration;
        private IHttpContextAccessor _httpContextAccessor;

        public WebUserContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            var user = httpContextAccessor.HttpContext.User;
            if (user.Identity.IsAuthenticated)
            {
                var claims = user.Claims;

                //retrieve user name
                UserId = claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier && !string.IsNullOrEmpty(c.Value))
                    .Select(c => Guid.Parse(c.Value))
                    .FirstOrDefault();

                //get actual username
                UserName = claims
                    .Where(c => c.Type == "preferred_username" && !string.IsNullOrEmpty(c.Value))
                    .Select(c => c.Value)
                    .FirstOrDefault();

                //get actual firstname
                FirstName = claims
                    .Where(c => c.Type == ClaimTypes.GivenName && !string.IsNullOrEmpty(c.Value))
                    .Select(c => c.Value)
                    .FirstOrDefault();

                //get actual lastname
                LastName = claims
                    .Where(c => c.Type == ClaimTypes.Surname && !string.IsNullOrEmpty(c.Value))
                    .Select(c => c.Value)
                    .FirstOrDefault();

                //get actual email
                Email = claims
                    .Where(c => c.Type == ClaimTypes.Email && !string.IsNullOrEmpty(c.Value))
                    .Select(c => c.Value)
                    .FirstOrDefault();

                //get company
                Company = claims
                    .Where(c => c.Type.ToLower() == "company" && !string.IsNullOrEmpty(c.Value))
                    .Select(c => c.Value)
                    .FirstOrDefault();


                //retrieve user claims
                _claims = claims
                    //.Where(c => c.Type == "vcust")
                    .Select(c => c.Value);

                //get TOPS role
                //Role = userService.GetApplicationRole(ApplicationCode, UserName);

                HandleTokenRefresh(claims);

                Token = AuthorizationToken();
            }
        }

        private void HandleTokenRefresh(IEnumerable<Claim> claims)
        {
            try
            {
                var notBeforeTime = long.Parse(claims.Where(c => c.Type == "nbf").Select(c => c.Value).Single());
                var expTime = long.Parse(claims.Where(c => c.Type == "exp").Select(c => c.Value).Single());
                var totalTime = expTime - notBeforeTime;
                var current = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var currentUsage = current - notBeforeTime;
                var refresh = _configuration.GetSection("AppSettings").GetValue<double>("TokenRefreshLimitPercent");

                if ((currentUsage / (double)totalTime) > refresh)
                {
                    DoRefresh();                   
                }
            }
            catch (Exception)
            {
                //log me
            }
        }

        private void DoRefresh()
        {
            var refreshToken = string.Empty;
            this._httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("TopsRT", out refreshToken);
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                using (var httpClient = new HttpClient())
                {
                    var identityServer = _configuration.GetSection("AppSettings").GetValue<string>("TopsIdentServUrl");
                    var clientId = _configuration.GetSection("AppSettings").GetValue<string>("ClientId");
                    var clientSecret = _configuration.GetSection("AppSettings").GetValue<string>("ClientSecret");

                    httpClient.BaseAddress = new Uri(identityServer);
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(clientId, clientSecret);

                    string uri = $"connect/token";

                    string content = string.Format("grant_type=refresh_token&refresh_token={0}", refreshToken);

                    var resp = httpClient.PostAsync(uri, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"), default(CancellationToken)).Result;
                    if (resp.IsSuccessStatusCode)
                    {
                        string json = resp.Content.ReadAsStringAsync().Result;
                        var results = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);

                        if (results.ContainsKey("access_token"))
                        {
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("TopsAT", results["access_token"]);
                        }

                        if (results.ContainsKey("refresh_token"))
                        {
                            _httpContextAccessor.HttpContext.Response.Cookies.Append("TopsRT", results["refresh_token"]);
                        }
                    }
                }
            }
        }

        public string AuthorizationToken()
        {
            StringValues token;
            var haveToken = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out token);

            if (haveToken)
                return token.ToString();
            else
                return null;
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
