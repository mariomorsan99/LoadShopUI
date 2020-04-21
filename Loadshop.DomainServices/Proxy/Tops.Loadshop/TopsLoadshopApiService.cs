using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.API.Models.DataModels;
using Loadshop.API.Models.Models;
using Loadshop.API.Models.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TMS.Infrastructure.Common.Utilities;

namespace Loadshop.DomainServices.Proxy.Tops.Loadshop
{
    public class TopsLoadshopApiService : ITopsLoadshopApiService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public TopsLoadshopApiService(IConfigurationRoot configuration, HttpClient httpClient)
        {
            _client = httpClient;
            _baseUrl = configuration["TOPSLoadshopApiUrl"];

            var encryptionKey = configuration["TOPSLoadshopApiEncryptionKey"];

            var user = CryptoUtils.Decrypt3DES(configuration["TOPSLoadshopApiUser"], encryptionKey);
            var password = CryptoUtils.Decrypt3DES(configuration["TOPSLoadshopApiPassword"], encryptionKey);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}")));
        }

        public async Task<ResponseMessage<Dictionary<string, List<string>>>> GetSourceSystemOwners(string ownerId)
        {
            var url = $"{_baseUrl}/v1/loadshop/sourcesystem-owners?ownerId={ownerId}";

            var resp = _client.GetAsync(url, default(CancellationToken)).Result;
            return await ProcessData<Dictionary<string, List<string>>, string>(resp, url, ownerId);
        }

        public async Task<ResponseMessage<List<LoadshopShipperMappingModel>>> GetShipperMappings(string ownerId)
        {
            var url = $"{_baseUrl}/v1/loadshop/shippermappings?ownerId={ownerId}";

            var resp = _client.GetAsync(url, default(CancellationToken)).Result;
            return await ProcessData<List<LoadshopShipperMappingModel>, string>(resp, url, ownerId);
        }

        public async Task<ResponseMessage<LoadshopShipperMappingModel>> CreateShipperMapping(LoadshopShipperMappingModel shipperMappingModel)
        {
            var url = $"{_baseUrl}/v1/loadshop/shippermapping";
            var content = JsonConvert.SerializeObject(shipperMappingModel);
            var requestBody = new StringContent(content, Encoding.UTF8, "application/json");
            
            var resp = _client.PostAsync(url, requestBody, default(CancellationToken)).Result;
            return await ProcessData<LoadshopShipperMappingModel, LoadshopShipperMappingModel>(resp, url, shipperMappingModel);
        }

        public async Task<ResponseMessage<LoadshopShipperMappingModel>> UpdateShipperMapping(LoadshopShipperMappingModel shipperMappingModel)
        {
            var url = $"{_baseUrl}/v1/loadshop/shippermapping";
            var content = JsonConvert.SerializeObject(shipperMappingModel);
            var requestBody = new StringContent(content, Encoding.UTF8, "application/json");

            var resp = _client.PutAsync(url, requestBody, default(CancellationToken)).Result;
            return await ProcessData<LoadshopShipperMappingModel, LoadshopShipperMappingModel>(resp, url, shipperMappingModel);
        }

        public async Task<ResponseMessage<IdentityUserData>> GetIdentityUser(string username)
        {
            var url = $"{_baseUrl}/v1/identity/getIdentityUserByUsername?username={username}";

            var resp = _client.GetAsync(url, default(CancellationToken)).Result;
            return await ProcessData<IdentityUserData, string>(resp, url, username);
        }

        public async Task<ResponseMessage<IdentityUserData>> CreateCustomerUser(RegisterViewModel newUser)
        {
            var url = $"{_baseUrl}/v1/identity/CustomerUser";
            var content = JsonConvert.SerializeObject(newUser);
            var requestBody = new StringContent(content, Encoding.UTF8, "application/json");

            var resp = _client.PostAsync(url, requestBody, default(CancellationToken)).Result;
            return await ProcessData<IdentityUserData, RegisterViewModel>(resp, url, newUser);
        }

        private async Task<ResponseMessage<T>> ProcessData<T,TD>(HttpResponseMessage resp, string url, TD data)
        {
            try {
                if (resp.IsSuccessStatusCode || resp.StatusCode == HttpStatusCode.BadRequest)
                {
                    var result = await resp.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ResponseMessage<T>>(result);
                }

                return new ResponseMessage<T>
                {
                    Errors = new List<ResponseError>
                    {
                        new ResponseError
                        {
                            Data = data,
                            Message = $"API call could not be completed. Url: {url} Resp: {resp.StatusCode}, {resp.ReasonPhrase}",
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage<T>
                {
                    Errors = new List<ResponseError>
                    {
                        new ResponseError
                        {
                            Data = data,
                            Message = $"API call could not be completed. Url: {url} Resp: {ex.Message}",
                            StackTrace = ex.StackTrace
                        }
                    }
                };
            }
        }
    }
}
