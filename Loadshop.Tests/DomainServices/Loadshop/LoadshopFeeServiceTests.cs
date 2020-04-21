using FluentAssertions;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class LoadshopFeeServiceTests
    {
        private static Guid CUSTOMER_ID = new Guid("11111111-1111-1111-1111-111111111111");
        private static string SCAC = "ABCD";
        private static CustomerEntity CUSTOMER => new CustomerEntity
        {
            CustomerId = CUSTOMER_ID,
            CustomerCarrierScacContracts = new List<CustomerCarrierScacContractEntity>
            {
                new CustomerCarrierScacContractEntity { Scac = SCAC }
            }
        };

        public static LoadData LOAD => new LoadData
        {
            CustomerId = CUSTOMER_ID,
            LineHaulRate = 100m,
            FuelRate = 5m
        };
            

        public class ApplyLoadshopFeeTests
        {
            [Fact]
            public void NullParameters()
            {
                var svc = new LoadshopFeeService();

                svc.Invoking(_ => _.ApplyLoadshopFee(null, null, null))
                    .Should().Throw<Exception>()
                    .WithMessage("Customer not found");
            }

            [Fact]
            public void CustomerNotFound()
            {
                var svc = new LoadshopFeeService();

                svc.Invoking(_ => _.ApplyLoadshopFee(null, LOAD, new List<CustomerEntity>()))
                    .Should().Throw<Exception>()
                    .WithMessage("Customer not found");
            }

            [Fact]
            public void UnbookedInNetworkFlatFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(SCAC, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(90m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m
                });
            }

            [Fact]
            public void UnbookedInNetworkPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.InNetworkPercentFee = .10m;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(SCAC, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(89.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m
                });
            }

            [Fact]
            public void UnbookedInNetworkFlatAndPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                customer.InNetworkPercentFee = .10m;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(SCAC, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(79.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m
                });
            }

            [Fact]
            public void UnbookedInNetworkFlatFeeAdd()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                customer.InNetworkFeeAdd = true;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(SCAC, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void UnbookedInNetworkPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.InNetworkPercentFee = .10m;
                customer.InNetworkFeeAdd = true;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(SCAC, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void UnbookedInNetworkFlatAndPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                customer.InNetworkPercentFee = .10m;
                customer.InNetworkFeeAdd = true;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(SCAC, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void BookingInNetworkFlatFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                var load = LOAD;
                load.Scac = SCAC;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(90m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m
                });
            }

            [Fact]
            public void BookingInNetworkPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.InNetworkPercentFee = .10m;
                var load = LOAD;
                load.Scac = SCAC;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(89.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m
                });
            }

            [Fact]
            public void BookingInNetworkFlatAndPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                customer.InNetworkPercentFee = .10m;
                var load = LOAD;
                load.Scac = SCAC;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(79.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m
                });
            }

            [Fact]
            public void BookingInNetworkFlatFeeAdd()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                customer.InNetworkFeeAdd = true;
                var load = LOAD;
                load.Scac = SCAC;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void BookingInNetworkPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.InNetworkPercentFee = .10m;
                customer.InNetworkFeeAdd = true;
                var load = LOAD;
                load.Scac = SCAC;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void BookingInNetworkFlatAndPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.InNetworkFlatFee = 10m;
                customer.InNetworkPercentFee = .10m;
                customer.InNetworkFeeAdd = true;
                var load = LOAD;
                load.Scac = SCAC;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m,
                    LoadshopFeeAdd = true
                });
            }






            [Fact]
            public void UnbookedOutNetworkFlatFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee("EFGH", load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(90m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m
                });
            }

            [Fact]
            public void UnbookedOutNetworkPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.OutNetworkPercentFee = .10m;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee("EFGH", load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(89.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m
                });
            }

            [Fact]
            public void UnbookedOutNetworkFlatAndPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                customer.OutNetworkPercentFee = .10m;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee("EFGH", load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(79.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m
                });
            }

            [Fact]
            public void UnbookedOutNetworkFlatFeeAdd()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                customer.OutNetworkFeeAdd = true;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee("EFGH", load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void UnbookedOutNetworkPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.OutNetworkPercentFee = .10m;
                customer.OutNetworkFeeAdd = true;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee("EFGH", load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void UnbookedOutNetworkFlatAndPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                customer.OutNetworkPercentFee = .10m;
                customer.OutNetworkFeeAdd = true;
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee("EFGH", load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void BookingOutNetworkFlatFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                var load = LOAD;
                load.Scac = "EFGH";

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(90m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m
                });
            }

            [Fact]
            public void BookingOutNetworkPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.OutNetworkPercentFee = .10m;
                var load = LOAD;
                load.Scac = "EFGH";

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(89.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m
                });
            }

            [Fact]
            public void BookingOutNetworkFlatAndPercentFeeSubtract()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                customer.OutNetworkPercentFee = .10m;
                var load = LOAD;
                load.Scac = "EFGH";

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(79.5m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m
                });
            }

            [Fact]
            public void BookingOutNetworkFlatFeeAdd()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                customer.OutNetworkFeeAdd = true;
                var load = LOAD;
                load.Scac = "EFGH";

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopFee = 10m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void BookingOutNetworkPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.OutNetworkPercentFee = .10m;
                customer.OutNetworkFeeAdd = true;
                var load = LOAD;
                load.Scac = "EFGH";

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 10.5m,
                    LoadshopFeeAdd = true
                });
            }

            [Fact]
            public void BookingOutNetworkFlatAndPercentFeeAdd()
            {
                var customer = CUSTOMER;
                customer.OutNetworkFlatFee = 10m;
                customer.OutNetworkPercentFee = .10m;
                customer.OutNetworkFeeAdd = true;
                var load = LOAD;
                load.Scac = "EFGH";

                var svc = new LoadshopFeeService();
                svc.ApplyLoadshopFee(null, load, new List<CustomerEntity> { customer });

                load.LineHaulRate.Should().Be(100m);
                load.FeeData.Should().BeEquivalentTo(new LoadshopFeeData
                {
                    LoadshopFlatFee = 10m,
                    LoadshopPercentFee = .10m,
                    LoadshopFee = 20.5m,
                    LoadshopFeeAdd = true
                });
            }
        }

        public class ReapplyLoadshopFeeTests
        {
            public static LoadData LOAD => new LoadData
            {
                CustomerId = CUSTOMER_ID,
                LineHaulRate = 100m,
                FuelRate = 5m,
                FeeData = new LoadshopFeeData
                {
                    LoadshopFee = 10m
                }
            };

            [Fact]
            public void FeeSubtract()
            {
                var load = LOAD;

                var svc = new LoadshopFeeService();
                svc.ReapplyLoadshopFeeToLineHaul(load);

                load.LineHaulRate.Should().Be(90m);
            }

            [Fact]
            public void FeeAdd()
            {
                var load = LOAD;
                load.FeeData.LoadshopFeeAdd = true;

                var svc = new LoadshopFeeService();
                svc.ReapplyLoadshopFeeToLineHaul(load);

                load.LineHaulRate.Should().Be(100m);
            }
        }
    }
}
