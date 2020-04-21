using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public interface ILoadshopFeeService
    {
        void ApplyLoadshopFee(string userPrimaryScac, ILoadFeeData load, IList<CustomerEntity> customers);
        void ReapplyLoadshopFeeToLineHaul(ILoadFeeData load);
    }
}