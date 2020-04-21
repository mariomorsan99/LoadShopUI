using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class CommodityService : ICommodityService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public CommodityService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<CommodityData> GetCommodities()
        {
            var commodities = _context.Commodities.OrderBy(x => x.CommodityName);
            return _mapper.Map<List<CommodityData>>(commodities);
        }
    }
}
