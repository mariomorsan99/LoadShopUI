using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Proxy.Visibility.Interfaces;
using Loadshop.DomainServices.Proxy.Visibility.Models;
using Loadshop.DomainServices.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Proxy.Visibility
{
    public class VisibilityProxyService : IVisibilityProxyService
    {
        private const string ErrorPrefix = "urn:Visibility";
        private readonly ILogger<VisibilityProxyService> logger;
        private readonly HttpClient httpClient;
        private readonly IUserContext userContext;
        private readonly IMapper mapper;
        private readonly IConfigurationRoot config;
        private const string LoadshopSenderCode = "LS";

        public VisibilityProxyService(ILogger<VisibilityProxyService> logger, HttpClient httpClient, IUserContext userContext, IMapper mapper,
            IConfigurationRoot config)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.userContext = userContext;
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<LoadStatusNotificationsData> GetLoadStatusNotificationsAsync()
        {
            const string mediaType = "application/json";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            httpClient.DefaultRequestHeaders.Add("Authorization", userContext.Token);

            var url = $"{config.GetValue<string>("MobileApiAddress")}t2g/api/Notification/OnStatusChangeV2/LSShipper";

            var responseMessage = await httpClient.GetAsync(url, default(CancellationToken));
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<VisibilityResponseModel<GetLoadStatusNotificationResponseModel>>(responseString);

            var result = mapper.Map<LoadStatusNotificationsData>(response.ResponsePayload);

            if (result == null)
            {
                // user hasn't subscribed to any notifications
                result = new LoadStatusNotificationsData();
            }

            return result;
        }
        public async Task<SaveLoadStatusNotificationsResponse> UpdateLoadStatusNotificationsAsync(LoadStatusNotificationsData notificationsData)
        {
            const string mediaType = "application/json";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            httpClient.DefaultRequestHeaders.Add("Authorization", userContext.Token);

            var url = $"{config.GetValue<string>("MobileApiAddress")}t2g/api/Notification/OnStatusChange/LSShipper";

            var payload = mapper.Map<VisibilityNotificationRegistrationModel>(notificationsData);

            payload.Sender = LoadshopSenderCode;
            // currently don't support these
            payload.IsMobilePush = false;
            payload.IsWebPush = false;
            if (!string.IsNullOrEmpty(payload.PhoneNumber))
            {
                payload.PhoneNumber = payload.PhoneNumber.Replace("(", string.Empty);
                payload.PhoneNumber = payload.PhoneNumber.Replace(")", string.Empty);
                payload.PhoneNumber = payload.PhoneNumber.Replace("-", string.Empty);
                payload.PhoneNumber = payload.PhoneNumber.Replace(" ", string.Empty);
                payload.PhoneNumber = payload.PhoneNumber.Trim();
            }
            var formdata = JsonConvert.SerializeObject(payload, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var jsonContent = new StringContent(formdata, Encoding.UTF8, mediaType);

            var responseMessage = await httpClient.PostAsync(url, jsonContent, default(CancellationToken));
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<VisibilityResponseModel<bool>>(responseString);

            var result = new SaveLoadStatusNotificationsResponse()
            {
                Successful = response.Successful
            };

            if (!result.Successful)
            {
                foreach (var error in response.Errors)
                {
                    result.AddError(ErrorPrefix, error.Message);
                }
            }

            return result;
        }
    }
}
