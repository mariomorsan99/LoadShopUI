using System.Collections.Generic;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Loadshop.Services.Interfaces
{
    public interface IMileageService
    {
        /// <summary>
        /// Gets direct miles for an origin/destination
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        int GetDirectMiles(MileageRequestData request);

        /// <summary>
        /// Gets the route mileage from the Google Route API
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<int> GetDirectRouteMiles(MileageRequestData request);

        /// <summary>
        /// Gets the route mileage from the Google Route API allowing multiple stops
        /// </summary>
        /// <returns></returns>
        Task<int> GetRouteMiles(IList<LoadStopData> stopData);
    }
}
