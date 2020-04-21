using System;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ICustomerService
    {
        CustomerData GetCustomer(Guid customerId);
    }
}
