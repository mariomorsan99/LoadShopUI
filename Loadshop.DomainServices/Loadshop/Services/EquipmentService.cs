using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        const string _vanCategoryId = "Van";

        public EquipmentService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<EquipmentData> GetEquipment()
        {
            //Sort Equipment by Category with nulls at bottom
            var equipment = _context.Equipment
                                    .Where(x => !x.IsDeleted)
                                    //Sort Van to the top and other to the bottom
                                    .OrderBy(x => x.CategoryId == _vanCategoryId? "aaa" : x.CategoryId ?? "zzz")
                                    .ThenBy(x => x.Sort)
                                    .ThenBy(x => x.EquipmentDesc);
            return _mapper.Map<List<EquipmentData>>(equipment);
        }
    }
}
