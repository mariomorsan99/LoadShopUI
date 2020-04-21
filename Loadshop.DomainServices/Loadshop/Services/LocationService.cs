using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class LocationService : ILocationService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;

        public LocationService(LoadshopDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public LocationData GetLocation(Guid customerId, string locationName)
        {
            locationName = locationName ?? string.Empty;

            var result = _context.Locations.FirstOrDefault(x => x.CustomerId == customerId && x.LocationName.ToLower() == locationName.ToLower());
            return _mapper.Map<LocationData>(result);
        }

        public List<LocationData> GetLocations(Guid customerId)
        {
            var results = _context.Locations.Where(x => x.CustomerId == customerId).OrderBy(x => x.LocationName).ToList();
            return _mapper.Map<List<LocationData>>(results);
        }

        public List<LocationData> SearchLocations(Guid customerId, string searchTerm)
        {
            searchTerm = searchTerm ?? string.Empty;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<LocationData>();
            }

            var results = _context.Locations.Where(x => x.CustomerId == customerId && x.LocationName.ToLower().Contains(searchTerm.ToLower())).OrderBy(x => x.LocationName).ToList();
            return _mapper.Map<List<LocationData>>(results);
        }

        public List<LocationData> AddOrUpdateLocations(Guid customerId, List<LocationData> locations, string username)
        {
            if (locations != null && locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    if (location != null)
                    {
                        location.CustomerId = customerId;
                        if (location.LocationId > 0)
                        {
                            var dbLocation = _context.Locations.SingleOrDefault(x => x.CustomerId == customerId && x.LocationId == location.LocationId);
                            if (dbLocation == null)
                            {
                                throw new Exception($"Location not found");
                            }

                            _mapper.Map(location, dbLocation);
                            
                        }
                        else
                        {
                            var dbLocation = _context.Locations.FirstOrDefault(x => x.CustomerId == customerId && x.LocationName.ToLower() == location.LocationName.ToLower());
                            if (dbLocation != null)
                            {
                                _mapper.Map(location, dbLocation);
                            }
                            else
                            {
                                dbLocation = _mapper.Map<LocationEntity>(location);
                                _context.Locations.Add(dbLocation);
                            }
                        }
                    }
                }

                _context.SaveChanges(username);
            }

            return locations;
        }

        public void DeleteLocation(Guid customerId, long locationId)
        {
            var location = _context.Locations.SingleOrDefault(x => x.CustomerId == customerId && x.LocationId == locationId);
            if (location == null)
            {
                throw new Exception($"Location not found");
            }

            _context.Locations.Remove(location);
            _context.SaveChanges();
        }
    }
}
