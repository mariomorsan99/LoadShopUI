using System.Collections.Generic;
using System.Linq;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Validation.Data.Address
{
    public class GeocodeAddress
    {
        public GeocodeAddress(Result item)
        {
            if(item != null) {
                var streetNumberComponent = GetAddressComponent(item.AddressComponents, new List<string> { "street_number" });
                var routeComponent = GetAddressComponent(item.AddressComponents, new List<string> { "route" });
                var cityAddressComponent = GetAddressComponent(item.AddressComponents, new List<string> { "locality", "political" });
                var stateAddressComponent = GetAddressComponent(item.AddressComponents, new List<string> { "administrative_area_level_1", "political" });
                var countryAddressComponent = GetAddressComponent(item.AddressComponents, new List<string> { "country", "political" });
                var postalCodeComponent = GetAddressComponent(item.AddressComponents, new List<string> { "postal_code" });

                StreetNumber = streetNumberComponent?.ShortName;
                Route = routeComponent?.ShortName;
                City = cityAddressComponent?.ShortName ?? cityAddressComponent?.LongName;
                State = stateAddressComponent?.ShortName;
                PostalCode = postalCodeComponent?.ShortName;
                Country = countryAddressComponent?.ShortName;
            }
        }

        public GeocodeAddress(LoadStopData stop)
        {
            if (!string.IsNullOrWhiteSpace(stop.Address1))
            {
                StreetNumber = stop.Address1.Split(' ')[0];
                Route = stop.Address1.Remove(0, StreetNumber.Length + 1);
            }
            City = stop.City;
            State = stop.State;
            PostalCode = stop.PostalCode;
            Country = stop.Country;
        }

        public string StreetNumber { get; set; }
        public string Route { get; set; }
        public string Address1
        {
            get
            {
                if (StreetNumber == null && Route == null) return null;
                return $"{StreetNumber} {Route}";
            }
        }

        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        /// <summary>
        /// Gets address component
        /// </summary>
        /// <param name="addressComponents"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static AddressComponent GetAddressComponent(IEnumerable<AddressComponent> addressComponents, List<string> types)
        {
            AddressComponent result = null;
            if (addressComponents != null)
            {
                result = addressComponents.FirstOrDefault(x => x.Types != null && !Enumerable.Except(x.Types, types).Any() && !types.Except(x.Types).Any());
            }

            return result;
        }
    }
}
