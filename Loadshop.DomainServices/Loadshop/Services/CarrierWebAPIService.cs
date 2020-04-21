using Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class CarrierWebAPIService : ICarrierWebAPIService
    {
        private readonly IConfigurationRoot _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _shipmentsURL;
        private readonly string _apiKey;


        public CarrierWebAPIService(HttpClient httpClient, IConfigurationRoot configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _shipmentsURL = _configuration["CarrierWebAPIUrl"]?.TrimEnd('/');
            _apiKey = _configuration["TopsLoadshopApiAuth"];


            //TODO: If we need to send individual credentials for requests based on user then we should not
            // use the DefaultRequestHeaders and instead should create request message and set their headers
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _apiKey);
        }

        public async Task<LoadStatusEvent<CarrierApiResponseMessages>> Send<T>(LoadStatusEvent<T> message)
        {
            var content = JsonConvert.SerializeObject(message);
            var requestBody = new StringContent(content, Encoding.UTF8, "application/json");
            var apiResponse = await _httpClient.PostAsync(_shipmentsURL, requestBody, default(CancellationToken));

            string result = await apiResponse.Content.ReadAsStringAsync();
            if (apiResponse.IsSuccessStatusCode)
            {
                var eventResult = JsonConvert.DeserializeObject<LoadStatusEvent<CarrierApiResponseMessages>>(result);
                return eventResult;
            }
            else
            {
                throw new Exception($"CarrierWebApi Failure: Url: {_shipmentsURL} Reason: {(int)apiResponse.StatusCode} - {apiResponse.ReasonPhrase}");
            }
        }
    }
}
