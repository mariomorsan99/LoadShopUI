using Loadshop.DomainServices.Loadshop.Services.Data;
using System;
using Loadshop.DomainServices.Loadshop.Services.Enum;

namespace Loadshop.DomainServices.Validation.Services
{
    public interface ILoadValidationService
    {
        void ValidateLoad(LoadDetailData load, Guid customerId, OrderAddressValidationEnum validateAddress, bool manuallyCreated, string urnPrefix, BaseServiceResponse response);
    }
}
