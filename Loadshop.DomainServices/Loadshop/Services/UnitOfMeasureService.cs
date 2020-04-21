using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class UnitOfMeasureService : IUnitOfMeasureService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public UnitOfMeasureService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<UnitOfMeasureData> GetUnitOfMeasures()
        {
            var results = _context.UnitOfMeasures.OrderBy(x => x.Name);
            return _mapper.Map<List<UnitOfMeasureData>>(results);
        }
    }
}
