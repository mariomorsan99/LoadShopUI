using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Pinpoint;
using Amazon.Pinpoint.Model;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class SMSService : ISMSService
    {
        private readonly IMapper _mapper;
        private readonly IConfigurationRoot _configuration;
        private readonly bool _isDev = true;
        private readonly Regex PhoneRegex = new Regex(@"^(\+[0-9]+)?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");

        public SMSService(IMapper mapper, IConfigurationRoot configuration)
        {
            _mapper = mapper;
            _configuration = configuration;
            _isDev = _configuration["Region"].ToLower().Contains("dev");
        }

        public async Task<bool> ValidateNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }
            var phone = FormatPhoneNumber(phoneNumber);

            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }

            var creds = new BasicAWSCredentials(_configuration["SMSAWSCredsAccess"], _configuration["SMSAWSCredsSecret"]);

            AmazonPinpointClient pinpointClient = null;
            //with proxy
            var proxyConfig = _configuration.GetValue<string>("ProxyAddress");
            if (string.IsNullOrEmpty(proxyConfig))
            {
                var config = new AmazonPinpointConfig
                {
                    ProxyHost = proxyConfig,
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
                };
                pinpointClient = new AmazonPinpointClient(creds, config);
            }
            else
            {
                //without proxy
                pinpointClient = new AmazonPinpointClient(creds, Amazon.RegionEndpoint.USEast1);
            }

            PhoneNumberValidateRequest phoneValidateRequest = new PhoneNumberValidateRequest
            {
                NumberValidateRequest = new NumberValidateRequest()
                {
                    PhoneNumber = phone,
                    IsoCountryCode = "US"
                }
            };

            PhoneNumberValidateResponse phoneValidateResponse = await pinpointClient.PhoneNumberValidateAsync(phoneValidateRequest);

            if (phoneValidateResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                var numberValidateResponse = phoneValidateResponse.NumberValidateResponse;
                return numberValidateResponse != null && numberValidateResponse.PhoneTypeCode == 0;
            }
            return false;
        }

        public async Task<bool> SendMessage(string phoneNumber, string message)
        {
            if (_isDev)
            {
                phoneNumber = _configuration["DevPhoneNumber"];
                message = $"TEST ({_configuration["Region"]}) {message.Trim()}";
            }
            if (!await ValidateNumber(phoneNumber))
            {
                return false;
            }
            var phone = FormatPhoneNumber(phoneNumber);
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }

            var creds = new BasicAWSCredentials(_configuration["SMSAWSCredsAccess"], _configuration["SMSAWSCredsSecret"]);

            AmazonSimpleNotificationServiceClient snsClient = null;
            //with proxy
            var proxyConfig = _configuration.GetValue<string>("ProxyAddress");
            if (string.IsNullOrEmpty(proxyConfig))
            {
                var config = new AmazonSimpleNotificationServiceConfig
                {
                    ProxyHost = proxyConfig,
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
                };
                snsClient = new AmazonSimpleNotificationServiceClient(creds, config);
            }
            else
            {
                //without proxy
                snsClient = new AmazonSimpleNotificationServiceClient(creds, Amazon.RegionEndpoint.USEast1);
            }

            PublishRequest request = new PublishRequest
            {
                Message = message.Trim(),
                PhoneNumber = phone
            };

            request.MessageAttributes.Add("AWS.SNS.SMS.SenderID", new MessageAttributeValue { StringValue = "kbxlt2gsms", DataType = "String" });
            request.MessageAttributes["AWS.SNS.SMS.MaxPrice"] = new MessageAttributeValue { StringValue = "0.50", DataType = "Number" };
            request.MessageAttributes["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue { StringValue = "Transactional", DataType = "String" };
            PublishResponse response = await snsClient.PublishAsync(request);

            if (response.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception("The phone number entered is invalid, try again.  Error: AWS SNS SMS - Parameter Value Invalid");
            }
            return true;
        }
        
        private string FormatPhoneNumber(string phoneNumber)
        {
            //if (_isDev)
            //{
            //    phone = "+19204271991";
            //}
            var match = PhoneRegex.Match(phoneNumber);
            if (match.Success)
            {
                return $"{(match.Groups[1].Value != "" ? match.Groups[1].Value : "+1")}{match.Groups[2].Value}{match.Groups[3].Value}{match.Groups[4].Value}";
            }
            return string.Empty;
        }
    }
}
