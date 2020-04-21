using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class LoadStopService : ILoadStopService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public LoadStopService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<StopTypeData> GetStopTypes()
        {
            var types = _context.StopTypes.OrderBy(x => x.Name);
            return _mapper.Map<List<StopTypeData>>(types);
        }

        public List<AppointmentSchedulerConfirmationTypeData> GetAppointmentSchedulerConfirmationTypes()
        {
            var types = _context.AppointmentSchedulerConfirmationTypes.OrderBy(x => x.AppointmentSchedulingCode);
            return _mapper.Map<List<AppointmentSchedulerConfirmationTypeData>>(types);
        }
    }
}
