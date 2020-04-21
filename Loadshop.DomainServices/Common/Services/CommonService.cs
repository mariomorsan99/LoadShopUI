using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Common.DataProvider;
using Loadshop.DomainServices.Common.Services.Data;
using TMS.Infrastructure.EntityFramework;

namespace Loadshop.DomainServices.Common.Services
{
    public class CommonService : ICommonService
    {
        private readonly StateDataContext _context;
        private readonly TopsDataContext _topsDataContext;
        private readonly IMapper _mapper;

        public CommonService(StateDataContext context, TopsDataContext topsDataContext, IMapper mapper)
        {
            _context = context;
            _topsDataContext = topsDataContext;
            _mapper = mapper;
        }

        public List<StateData> GetUSCANStateProvinces()
        {
            var states = _context.States
                .Where(x => (x.CountryCd == "USA" || x.CountryCd == "CAN") && x.PreferredStateCdInd == "Y")
                .OrderBy(x => x.StateName);
            return _mapper.Map<List<StateData>>(states);
        }

        public StateData GetUSCANStateProvince(string stateName)
        {
            var state = _context.States
                .Where(x => (x.CountryCd == "USA" || x.CountryCd == "CAN") && x.PreferredStateCdInd == "Y" && (x.StateName == stateName || x.StateCd == stateName))
                .FirstOrDefault();
            return _mapper.Map<StateData>(state);
        }

        public List<string> GetCarrierVisibilityTypes(string username, string carrierId)
        {
            return _topsDataContext.GetCarrierVisibilityTypes(username, carrierId);
        }

        public CapRateData GetCapRates(string loadId)
        {
            return _topsDataContext.GetCapRates(loadId);
        }
    }
}
