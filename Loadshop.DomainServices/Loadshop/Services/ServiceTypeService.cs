using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public ServiceTypeService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<ServiceTypeData> GetServiceTypes()
        {
            var types = _context.ServiceTypes.OrderBy(x => x.Name);
            return _mapper.Map<List<ServiceTypeData>>(types);
        }
    }
}
