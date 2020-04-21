using System;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Validation.Data.Address;

namespace Loadshop.DomainServices.Validation.Services
{
    public interface IAddressValidationService
    {
        /// <summary>
        /// Checks if an address 1/city/state/country/zip code combination is valid
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        bool IsAddressValid(Guid customerId, LoadStopData stop);

        GeocodeAddress GetValidAddress(LoadStopData stop);
    }
}
