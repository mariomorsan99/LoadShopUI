using FluentAssertions;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Utilities;
using Loadshop.DomainServices.Validation.Data;
using Loadshop.DomainServices.Validation.Services;
using Loadshop.Tests.DomainServices;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Contrib.HttpClient;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LoadBoard.Testing.DomainServices.Validation
{
    public class RecaptchaServiceTests
    {
        public class ValidateToken : IClassFixture<TestFixture>
        {
            private readonly Mock<IConfigurationRoot> _mockConfig;
            private RecaptchaService _service;
            private RecaptchaRequest<object> _request;
            private Mock<HttpMessageHandler> _httpHandler;
            private string URL = "https://testing.com/verifyurl/";

            public ValidateToken(TestFixture testFixture)
            {

                _httpHandler = new Mock<HttpMessageHandler>();

                _mockConfig = new Mock<IConfigurationRoot>();
                _mockConfig.SetupGet(x => x["GoogleReCaptchaAcceptableScore"]).Returns("0.8");
                _mockConfig.SetupGet(x => x["GoogleReCaptchaSiteVerify"]).Returns(URL);
                _mockConfig.SetupGet(x => x["GoogleReCaptchaV3Secret"]).Returns("SECRET");
                _mockConfig.SetupGet(x => x["ProxyAddress"]).Returns((string)null);
                
               
                _request = new RecaptchaRequest<object>
                {
                    Token = "RECAPTCHA_TOKEN",
                    Data = null
                };
                InitService();
            }

            private void InitService()
            {
                var http = _httpHandler.CreateClient();
                _service = new RecaptchaService(http, _mockConfig.Object);
            }
            [Fact]
            public void MissingRequest()
            {
                _request = null;
                _service.Awaiting(x => x.ValidateToken(_request))
                    .Should()
                    .Throw<Exception>()
                    .WithMessage("Invalid recaptcha request");
            }

            [Fact]
            public void ValidRequest()
            {
                var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new RecaptchaResponse
                    {
                        Success = true,
                        Score = 0.9m
                    }))
                };
                _httpHandler.SetupRequest(HttpMethod.Post, $"{URL}?secret=SECRET&response=RECAPTCHA_TOKEN")
                        .ReturnsAsync(expectedResponse);
                InitService();
                _service.Awaiting(x => x.ValidateToken(_request))
                   .Should()
                   .NotThrow<Exception>();
            }

            [Fact]
            public void FailureResponse()
            {
                var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new RecaptchaResponse
                    {
                        Success = false,
                        Score = 0.79m
                    }))
                };
                _httpHandler = new Mock<HttpMessageHandler>();
                _httpHandler.SetupRequest(HttpMethod.Post, $"{URL}?secret=SECRET&response=RECAPTCHA_TOKEN")
                        .ReturnsAsync(expectedResponse);
                InitService();

                _service.Awaiting(x => x.ValidateToken(_request))
                   .Should()
                   .Throw<Exception>()
                   .WithMessage("Recaptcha Verification Failed:*");
            }

            [Fact]
            public void BelowThresholdResponse()
            {
                var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new RecaptchaResponse
                    {
                        Success = true,
                        Score = 0.79m
                    }))
                };
                _httpHandler = new Mock<HttpMessageHandler>();
                _httpHandler.SetupRequest(HttpMethod.Post, $"{URL}?secret=SECRET&response=RECAPTCHA_TOKEN")
                        .ReturnsAsync(expectedResponse);
                InitService();
                _service.Awaiting(x => x.ValidateToken(_request))
                   .Should()
                   .Throw<Exception>()
                   .WithMessage("Recaptcha Score Too Low:*");
            }
        }
    }
}

