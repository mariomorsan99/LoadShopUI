using System;
using System.Collections.Generic;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface ILocationService
    {
        /// <summary>
        /// Gets location by name
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="locationName"></param>
        /// <returns></returns>
        LocationData GetLocation(Guid customerId, string locationName);

        /// <summary>
        /// Gets locations for a customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        List<LocationData> GetLocations(Guid customerId);

        /// <summary>
        /// Searches customer locations
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        List<LocationData> SearchLocations(Guid customerId, string searchTerm);

        /// <summary>
        /// Adds or updates locations
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="locations"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        List<LocationData> AddOrUpdateLocations(Guid customerId, List<LocationData> locations, string username);

        /// <summary>
        /// Deletes a location
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        void DeleteLocation(Guid customerId, long locationId);
    }
}
