using System;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.Services;
using Xunit;
using FluentAssertions;
using Moq;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class CustomerTransactionLogServiceTests : IClassFixture<TestFixture>
    {
        private readonly IMapper _mapper;
        private Mock<LoadshopDataContext> _db;
        private CustomerTransactionLogService _service;
        private string _username = "LoadBoard.Customer.API";

        private static Guid CUSTOMER_ID = Guid.NewGuid();
        private static Guid IDENT_USER_ID = Guid.NewGuid();

        private CustomerEntity CUSTOMER = new CustomerEntity
        {
            CustomerId = CUSTOMER_ID,
            IdentUserId = IDENT_USER_ID
        };

        public CustomerTransactionLogServiceTests(TestFixture fixture)
        {
            _mapper = fixture.Mapper;
            _db = new MockDbBuilder().Build();
        }

        [Fact]
        public void LogTransaction_CustomerNotFound()
        {
            _service = new CustomerTransactionLogService(_db.Object, _mapper);

            _service.LogTransaction(Guid.NewGuid(), "url", "request", "response").Should().BeFalse();
            _db.Verify(x => x.SaveChanges(_username), Times.Never);
        }

        [Fact]
        public void LogTransaction_ExceptionIsntThrown()
        {
            _db = new MockDbBuilder()
                .WithCustomer(CUSTOMER)
                .Build();
            _db.Setup(x => x.CustomerTransactionLogs).Throws(new Exception("unit test exception"));

            _service = new CustomerTransactionLogService(_db.Object, _mapper);

            _service.LogTransaction(IDENT_USER_ID, "url", "request", "response").Should().BeFalse();
        }

        [Fact]
        public void LogTransaction()
        {
            _db = new MockDbBuilder()
                .WithCustomer(CUSTOMER)
                .Build();
            _service = new CustomerTransactionLogService(_db.Object, _mapper);

            _service.LogTransaction(IDENT_USER_ID, "url", "request", "response").Should().BeTrue();
            _db.Verify(x => x.SaveChanges(_username), Times.Once);
        }

        [Fact]
        public void GetCustomerId_NotFound()
        {
            _service = new CustomerTransactionLogService(_db.Object, _mapper);
            _service.GetCustomerId(Guid.Empty).Should().NotHaveValue();
            _service.GetCustomerId(Guid.NewGuid()).Should().NotHaveValue();
        }

        [Fact]
        public void GetCustomerId()
        {
            _db = new MockDbBuilder()
                .WithCustomer(CUSTOMER)
                .Build();
            _service = new CustomerTransactionLogService(_db.Object, _mapper);

            _service.GetCustomerId(IDENT_USER_ID).Should().Be(CUSTOMER_ID);
        }

        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [Theory]
        public void WasResponseSuccessful_Null(string response)
        {
            _service = new CustomerTransactionLogService(_db.Object, _mapper);
            _service.WasResponseSuccessful(response).Should().BeFalse();
        }

        [Fact]
        public void WasResponseSuccessful()
        {
            _service = new CustomerTransactionLogService(_db.Object, _mapper);
            _service.WasResponseSuccessful(@"{
                ""errors"": [
                    {
                        ""data"": null,
                        ""code"": 0,
                        ""display"": false,
                        ""message"": ""Load not found"",
                        ""stackTrace"": null
                    }
                ],
                ""data"": null,
                ""Success"":TRUE
            }").Should().BeTrue();
        }
    }
}
