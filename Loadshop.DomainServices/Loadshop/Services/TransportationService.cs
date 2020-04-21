using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class TransportationService : ITransportationService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public TransportationService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<TransportationModeData> GetTransportationModes()
        {
            var transportationModes = _context.TransportationModes.OrderBy(x => x.Name);
            return _mapper.Map<List<TransportationModeData>>(transportationModes);
        }
    }
}
