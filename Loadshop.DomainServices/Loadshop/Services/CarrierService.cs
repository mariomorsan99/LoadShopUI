using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class CarrierService : ICarrierService
    {
        private readonly LoadshopDataContext _db;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTime;

        public CarrierService(LoadshopDataContext db, IMapper mapper, IDateTimeProvider dateTime)
        {
            _db = db;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public bool IsActiveCarrier(string carrierId)
        {
            if (string.IsNullOrWhiteSpace(carrierId))
            {
                return false;
            }

            var carrier = _db.Carriers.SingleOrDefault(x => x.CarrierId == carrierId);
            if (carrier == null)
            {
                carrier = _db.Carriers.FirstOrDefault(x => x.CarrierName == carrierId);
            }

            return !string.IsNullOrWhiteSpace(carrier?.CarrierId) && (carrier?.IsLoadshopActive ?? false);
        }

        public bool IsPlanningEligible(string scac)
        {
            if (string.IsNullOrWhiteSpace(scac))
            {
                return false;
            }

            var carrierScacs = _db.CarrierScacs.SingleOrDefault(x => x.Scac == scac);
            //Changed IsPlanningEligible to IsBookingEligible, need to varify this for Port fro Tops
            return carrierScacs?.IsBookingEligible ?? false;
        }

        /// <summary>
        /// Get all Carrier scac that are active Scacs
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<CarrierCarrierScacGroupData>> GetAllCarrierScacsAsync()
        {
            var carrierScacs = await _db.CarrierScacs
                .Include(x => x.Carrier)
                .Where(carrierScac =>
                    carrierScac.Carrier.IsLoadshopActive
                    && carrierScac.IsActive
                    && carrierScac.IsBookingEligible
                    && (carrierScac.EffectiveDate == null || _dateTime.Today >= carrierScac.EffectiveDate)
                    && (carrierScac.ExpirationDate == null || _dateTime.Today <= carrierScac.ExpirationDate))
                .GroupBy(x => x.CarrierId)
                .ToListAsync();

                return carrierScacs
                .Select(x => new CarrierCarrierScacGroupData
                {
                    Carrier = _mapper.Map<CarrierData>(x.First().Carrier),
                    CarrierScacs = _mapper.Map<List<CarrierScacData>>(x.ToList())
                })
                .OrderBy(x => x.Carrier.CarrierName)
                .ToList().AsReadOnly();
        }
    }
}
