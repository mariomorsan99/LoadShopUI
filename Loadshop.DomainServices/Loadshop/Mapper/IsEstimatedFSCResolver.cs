using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Utilities;
using System;
using System.Linq;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Mapper
{
    public class BaseIsEstimatedFscResolver
    {
        private readonly IDateTimeProvider _dateTime;
        private readonly ICustomerService _custSvc;

        public BaseIsEstimatedFscResolver()
        {
            // Required for unit testing's inability to inject dependencies for these resolvers
        }

        public BaseIsEstimatedFscResolver(IDateTimeProvider dateTime, ICustomerService custSvc)
        {
            _dateTime = dateTime;
            _custSvc = custSvc;
        }

        protected bool IsEstimatedFsc(Guid customerId, DateTime? originPickupDate)
        {
            if (customerId.Equals(default(Guid)) || !originPickupDate.HasValue)
            {
                return false;
            }

            if (_custSvc == null)
            {
                return false; // When unit testing, we have no ICustomerService injected, so just always return false
            }

            var customer = _custSvc.GetCustomer(customerId);
            return FscUtilities.IsEstimatedFsc(customer, originPickupDate.Value, _dateTime.Now);
        }
    }

    public class LoadEntity_ShippingLoadData_IsEstimatedFscResolver : BaseIsEstimatedFscResolver, IValueResolver<LoadEntity, ShippingLoadData, bool>
    {
        public LoadEntity_ShippingLoadData_IsEstimatedFscResolver() : base() {}

        public LoadEntity_ShippingLoadData_IsEstimatedFscResolver(ICustomerService custSvc, IDateTimeProvider dateTime)
            : base(dateTime, custSvc) { }

        public bool Resolve(LoadEntity source, ShippingLoadData destination, bool destMember, ResolutionContext context)
        {
            if (source == null || source.CustomerId.Equals(default(Guid)))
            {
                return false;
            }

            var originPickupDate = source.LoadStops?.Select(_ => _.EarlyDtTm ?? _.LateDtTm).FirstOrDefault();
            return IsEstimatedFsc(source.CustomerId, originPickupDate);
        }
    }

    public class LoadDetailViewEntity_LoadData_IsEstimatedFscResolver : BaseIsEstimatedFscResolver, IValueResolver<LoadDetailViewEntity, LoadData, bool>
    {
        public LoadDetailViewEntity_LoadData_IsEstimatedFscResolver() : base() { }

        public LoadDetailViewEntity_LoadData_IsEstimatedFscResolver(ICustomerService custSvc, IDateTimeProvider dateTime)
            : base(dateTime, custSvc) { }

        public bool Resolve(LoadDetailViewEntity source, LoadData destination, bool destMember, ResolutionContext context)
        {
            var originPickupDate = source.LoadStops?.Select(_ => _.EarlyDtTm ?? _.LateDtTm).FirstOrDefault();
            return IsEstimatedFsc(source.CustomerId, originPickupDate);
        }
    }
}
