using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Validation.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Validation.Services
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _verificationUrl;
        private readonly decimal _minScore;
        private readonly string _secret;

        public RecaptchaService(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            _configuration = configuration;

            _minScore = decimal.Parse(configuration["GoogleReCaptchaAcceptableScore"]);
            _verificationUrl = configuration["GoogleReCaptchaSiteVerify"];
            _secret = configuration["GoogleReCaptchaV3Secret"];
        }

        public async Task ValidateToken<T>(RecaptchaRequest<T> request)
        {
            if (request == null)
                throw new Exception("Invalid recaptcha request");

            var uri = $"{_verificationUrl}?secret={_secret}&response={request.Token}";

            var requestPayload = new HttpRequestMessage(HttpMethod.Post, uri);

            var responseMessage = await httpClient.SendAsync(requestPayload);
            responseMessage.EnsureSuccessStatusCode();

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<RecaptchaResponse>(responseString);
            if (!response.Success)
                throw new Exception($"Recaptcha Verification Failed: {responseString}");
            else if (response.Score < _minScore)
                throw new Exception($"Recaptcha Score Too Low: {responseString}");
        }
    }
}
