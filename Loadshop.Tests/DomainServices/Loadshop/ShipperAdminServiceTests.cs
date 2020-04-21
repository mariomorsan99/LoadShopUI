using AutoMapper;
using FluentAssertions;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Moq;
using System;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Xunit;
using Loadshop.DomainServices.Proxy.Tops.Loadshop;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using System.Collections.Generic;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class ShipperAdminServiceTests
    {
        public class AddShipperTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private ShipperAdminService _svc;
            private Mock<ITopsLoadshopApiService> _topsLoadshopApiService;
            private Mock<IUserAdminService> _userAdminService;
            private Mock<INotificationService> _notificationService;
            private Mock<IUserContext> _userContext;

            private const string USERNAME = "unit_test";

            public AddShipperTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder().Build();
                _topsLoadshopApiService = new Mock<ITopsLoadshopApiService>();
                _userAdminService = new Mock<IUserAdminService>();
                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _notificationService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
            }

            [Fact]
            public void InvalidCustomerLoadType()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.AddShipper(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.NewShipper,
                    CustomerLoadTypeExpirationDate = null
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Expiration Date is required for New Shipper type.");
            }
            private ShipperAdminService BuildService()
            {
                return new ShipperAdminService(_db.Object, _securityService.Object, _mapper,
                    _topsLoadshopApiService.Object, _userAdminService.Object,
                    _notificationService.Object, _userContext.Object);
            }


            [Fact]
            public void NegativeInNetworkFlatFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.AddShipper(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkFlatFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("In Network Flat Fee must be $0.00 or more.");
            }

            [Fact]
            public void NegativeInNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.AddShipper(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkPercentFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("In Network Flat Fee must be 0.00% or more.");
            }


            [Fact]
            public void TooHighInNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.AddShipper(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkPercentFee = 10.00m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("In Network Flat Fee must be less than 1,000%.");
            }

            [Fact]
            public void NegativeOutNetworkFlatFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.AddShipper(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    OutNetworkFlatFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Out Network Flat Fee must be $0.00 or more.");
            }

            [Fact]
            public void NegativeOutNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.AddShipper(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    OutNetworkPercentFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Out Network Flat Fee must be 0.00% or more.");
            }


            [Fact]
            public void TooHighOutNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.AddShipper(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    OutNetworkPercentFee = 10.00m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Out Network Flat Fee must be less than 1,000%.");
            }

            [Fact]
            public void SendsFeeChangeEmail()
            {
                var customerId = new Guid("11111111-1111-1111-1111-111111111111");
                var updatedCustomer = new CustomerProfileData()
                {
                    CustomerId = customerId,
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkFlatFee = 10.00m,
                    CustomerContacts = new List<CustomerContactData>(),
                    CustomerCarrierScacs = new List<string>()
                };
                _userContext.SetupGet(_ => _.FirstName).Returns("fname");
                _userContext.SetupGet(_ => _.LastName).Returns("lname");
                _svc = BuildService();

                _svc.AddShipper(updatedCustomer, USERNAME);

                _notificationService.Verify(_ => _.SendShipperFeeChangeEmail(null, updatedCustomer, "fname lname"));
            }
        }

        public class ValidateCustomerOrderTypeTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private ShipperAdminService _svc;
            private Mock<ITopsLoadshopApiService> _topsLoadshopApiService;
            private Mock<IUserAdminService> _userAdminService;
            private Mock<INotificationService> _notificationService;
            private Mock<IUserContext> _userContext;

            public ValidateCustomerOrderTypeTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder().Build();
                _topsLoadshopApiService = new Mock<ITopsLoadshopApiService>();
                _userAdminService = new Mock<IUserAdminService>();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _notificationService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
            }

            [Fact]
            public void Null()
            {
                _svc = BuildService();
                _svc.ValidateCustomerOrderType(null);
            }

            [Fact]
            public void Empty()
            {
                _svc = BuildService();
                _svc.ValidateCustomerOrderType(new CustomerProfileData());
            }

            [Fact]
            public void CustomerLoadTypeId_HighPriority()
            {
                _svc = BuildService();
                _svc.ValidateCustomerOrderType(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null
                });

                _svc.ValidateCustomerOrderType(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = DateTime.Today
                });
            }

            [Fact]
            public void CustomerLoadTypeId_NewShipper_Invalid()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.ValidateCustomerOrderType(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.NewShipper,
                    CustomerLoadTypeExpirationDate = null
                })).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Expiration Date is required for New Shipper type.");
            }

            [Fact]
            public void CustomerLoadTypeId_NewShipper_Valid()
            {
                _svc = BuildService();

                _svc.ValidateCustomerOrderType(new CustomerProfileData()
                {
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.NewShipper,
                    CustomerLoadTypeExpirationDate = DateTime.Today
                });
            }

            private ShipperAdminService BuildService()
            {
                return new ShipperAdminService(_db.Object, _securityService.Object, _mapper,
                    _topsLoadshopApiService.Object, _userAdminService.Object,
                    _notificationService.Object, _userContext.Object);
            }
        }

        public class UpdateShipperTests : IClassFixture<TestFixture>
        {
            private readonly IMapper _mapper;
            private Mock<LoadshopDataContext> _db;
            private Mock<ISecurityService> _securityService;
            private ShipperAdminService _svc;
            private Mock<ITopsLoadshopApiService> _topsLoadshopApiService;
            private Mock<IUserAdminService> _userAdminService;
            private Mock<INotificationService> _notificationService;
            private Mock<IUserContext> _userContext;

            private const string USERNAME = "unit_test";

            public UpdateShipperTests(TestFixture fixture)
            {
                _mapper = fixture.Mapper;
                _db = new MockDbBuilder().Build();
                _topsLoadshopApiService = new Mock<ITopsLoadshopApiService>();
                _userAdminService = new Mock<IUserAdminService>();

                _securityService = new Mock<ISecurityService>();
                _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
                _notificationService = new Mock<INotificationService>();
                _userContext = new Mock<IUserContext>();
            }

            [Fact]
            public void InvalidCustomerLoadType()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.UpdateShipper(new CustomerProfileData()
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.NewShipper,
                    CustomerLoadTypeExpirationDate = null
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Expiration Date is required for New Shipper type.");
            }
            private ShipperAdminService BuildService()
            {
                return new ShipperAdminService(_db.Object, _securityService.Object, _mapper,
                    _topsLoadshopApiService.Object, _userAdminService.Object,
                    _notificationService.Object, _userContext.Object);
            }

            [Fact]
            public void NegativeInNetworkFlatFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.UpdateShipper(new CustomerProfileData()
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkFlatFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("In Network Flat Fee must be $0.00 or more.");
            }

            [Fact]
            public void NegativeInNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.UpdateShipper(new CustomerProfileData()
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkPercentFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("In Network Flat Fee must be 0.00% or more.");
            }


            [Fact]
            public void TooHighInNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.UpdateShipper(new CustomerProfileData()
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkPercentFee = 10.00m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("In Network Flat Fee must be less than 1,000%.");
            }

            [Fact]
            public void NegativeOutNetworkFlatFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.UpdateShipper(new CustomerProfileData()
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    OutNetworkFlatFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Out Network Flat Fee must be $0.00 or more.");
            }

            [Fact]
            public void NegativeOutNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.UpdateShipper(new CustomerProfileData()
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    OutNetworkPercentFee = -0.01m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Out Network Flat Fee must be 0.00% or more.");
            }


            [Fact]
            public void TooHighOutNetworkPercentFee()
            {
                _svc = BuildService();

                _svc.Invoking(x => x.UpdateShipper(new CustomerProfileData()
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    OutNetworkPercentFee = 10.00m
                }, USERNAME)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Out Network Flat Fee must be less than 1,000%.");
            }

            [Fact]
            public void SendsFeeChangeEmail()
            {
                var customerId = new Guid("11111111-1111-1111-1111-111111111111");
                var customer = new CustomerEntity
                {
                    CustomerId = customerId,
                    Name = "Test Customer",
                    InNetworkFlatFee = 15.00m,
                    CustomerContacts = new List<CustomerContactEntity>(),
                    CustomerCarrierScacContracts = new List<CustomerCarrierScacContractEntity>()
                };
                var updatedCustomer = new CustomerProfileData()
                {
                    CustomerId = customerId,
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkFlatFee = 10.00m,
                    CustomerContacts = new List<CustomerContactData>(),
                    CustomerCarrierScacs = new List<string>()
                };
                _db = new MockDbBuilder()
                    .WithCustomer(customer)
                    .Build();
                _userContext.SetupGet(_ => _.FirstName).Returns("fname");
                _userContext.SetupGet(_ => _.LastName).Returns("lname");
                _svc = BuildService();

                _svc.UpdateShipper(updatedCustomer, USERNAME);

                _notificationService.Verify(_ => _.SendShipperFeeChangeEmail(customer, updatedCustomer, "fname lname"));
            }

            [Fact]
            public void DoesntSendFeeChangeEmail()
            {
                var customerId = new Guid("11111111-1111-1111-1111-111111111111");
                var customer = new CustomerEntity
                {
                    CustomerId = customerId,
                    Name = "Test Customer",
                    InNetworkFlatFee = 10.00m,
                    CustomerContacts = new List<CustomerContactEntity>(),
                    CustomerCarrierScacContracts = new List<CustomerCarrierScacContractEntity>()
                };
                var updatedCustomer = new CustomerProfileData()
                {
                    CustomerId = customerId,
                    CustomerLoadTypeId = (int)CustomerLoadTypeEnum.HighPriority,
                    CustomerLoadTypeExpirationDate = null,
                    InNetworkFlatFee = 10.00m,
                    CustomerContacts = new List<CustomerContactData>(),
                    CustomerCarrierScacs = new List<string>()
                };
                _db = new MockDbBuilder()
                    .WithCustomer(customer)
                    .Build();
                _userContext.SetupGet(_ => _.FirstName).Returns("fname");
                _userContext.SetupGet(_ => _.LastName).Returns("lname");
                _svc = BuildService();

                _svc.UpdateShipper(updatedCustomer, USERNAME);

                _notificationService.Verify(_ => _.SendShipperFeeChangeEmail(customer, updatedCustomer, "fname lname"), Times.Never);
            }
        }
    }
}
