using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class CustomerLoadTypeService : ICustomerLoadTypeService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public CustomerLoadTypeService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<CustomerLoadTypeData> GetCustomerLoadTypes()
        {
            var commodities = _context.CustomerLoadTypes.OrderBy(x => x.Name);
            return _mapper.Map<List<CustomerLoadTypeData>>(commodities);
        }
    }
}
