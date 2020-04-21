using System;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public CustomerService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public CustomerData GetCustomer(Guid customerId)
        {
            var customer = _context.Customers.Find(customerId);
            return _mapper.Map<CustomerData>(customer);
        }
    }
}
