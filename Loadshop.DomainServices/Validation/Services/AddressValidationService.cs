using Loadshop.DomainServices.Extensions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Validation.Data;
using Loadshop.DomainServices.Validation.Data.Address;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Loadshop.DomainServices.Loadshop.Services.Data;

namespace Loadshop.DomainServices.Validation.Services
{
    public class AddressValidationService : IAddressValidationService
    {
        private readonly LoadshopDataContext _context;
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly bool _isValidationEnabled;

        public AddressValidationService(LoadshopDataContext context, IConfigurationRoot configuration, HttpClient client)
        {
            _context = context;
            _client = client;
            bool.TryParse(configuration["AddressValidationEnabled"], out _isValidationEnabled);
            _baseUrl = configuration["GoogleGeocodingApiUrl"];
            _apiKey = configuration["GoogleApiKey"];
        }

        public bool IsAddressValid(Guid customerId, LoadStopData stop)
        {
            if (!_isValidationEnabled)
            {
                return true;
            }

            var customer = _context.Customers.SingleOrDefault(x => x.IdentUserId == customerId);
            if (customer == null || !customer.ValidateAddresses)
            {
                return true;
            }

            var response = GetGeocodeResponse(stop);
            var isValid = IsValid(stop, response);

            return isValid;
        }

        public GeocodeAddress GetValidAddress(LoadStopData stop)
        {
            var response = GetGeocodeResponse(stop);

            if (response?.Results != null)
            {
                foreach (var item in response.Results)
                {
                    if (item?.AddressComponents != null)
                    {
                        var address = new GeocodeAddress(item);
                        if (DoesAddressMatchResult(stop, address))
                        {
                            return address;
                        }
                    }
                }
            }

            return new GeocodeAddress(response?.Results?.FirstOrDefault());
        }

        public static string StandardizeAddress(string value, bool scrubSuffix = false)
        {
            // Find all occurrences of multiple spaces
            var regex = new Regex("[ ]{2,}", RegexOptions.None);

            var result = regex.Replace(value, " ")
                .Replace(".", "").Replace("'", "")
                .Replace("MOUNT ", "MT ").Replace("SAINT ", "ST ").Replace("FORT ", "FT ")
                // Directions may also be in road name so look for space before and after
                // USPS 233 Directionals
                .Replace(" NORTH ", " N ", StringComparison.OrdinalIgnoreCase)
                .Replace(" SOUTH ", " S ", StringComparison.OrdinalIgnoreCase)
                .Replace(" EAST ", " E ", StringComparison.OrdinalIgnoreCase)
                .Replace(" WEST ", " W ", StringComparison.OrdinalIgnoreCase)
                .Replace(" NORTHEAST ", " N E ", StringComparison.OrdinalIgnoreCase)
                .Replace(" SOUTHEAST ", " S E ", StringComparison.OrdinalIgnoreCase)
                .Replace(" NORTHWEST ", " N W ", StringComparison.OrdinalIgnoreCase)
                .Replace(" SOUTHWEST ", " S W ", StringComparison.OrdinalIgnoreCase)
                // Road Types
                .Replace(" AVENUE", " AVE", StringComparison.OrdinalIgnoreCase)
                .Replace(" BOULEVARD", " BLVD", StringComparison.OrdinalIgnoreCase)
                .Replace(" CIRCLE", " CIR", StringComparison.OrdinalIgnoreCase)
                .Replace(" COURT", " CT", StringComparison.OrdinalIgnoreCase)
                .Replace(" DRIVE", " DR", StringComparison.OrdinalIgnoreCase)
                .Replace(" EXPRESSWAY", " EXPY", StringComparison.OrdinalIgnoreCase)
                .Replace(" FREEWAY", " FWY", StringComparison.OrdinalIgnoreCase)
                .Replace(" LANE", " LN", StringComparison.OrdinalIgnoreCase)
                .Replace(" PARKWAY", " PKY", StringComparison.OrdinalIgnoreCase)
                .Replace(" ROAD", " RD", StringComparison.OrdinalIgnoreCase)
                .Replace(" SQUARE", " SQ", StringComparison.OrdinalIgnoreCase)
                .Replace(" STREET", " ST", StringComparison.OrdinalIgnoreCase)
                .Replace(" TURNPIKE", " TPKE", StringComparison.OrdinalIgnoreCase);

            if (scrubSuffix)
            {
                result = result?
                    .Replace(" AVE", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" BLVD", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" CIR", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" CT", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" DR", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" EXPY", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" FWY", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" LN", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" PKY", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" RD", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" SQ", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" ST", "", StringComparison.OrdinalIgnoreCase)
                    .Replace(" TPKE", "", StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        private GeocodeResponse GetGeocodeResponse(LoadStopData stop)
        {
            var url = BuildRequestUrl(stop.Address1, stop.City, stop.State, stop.Country, stop.PostalCode);
            var responseMessage = _client.GetAsync(url, default(CancellationToken)).Result;
            var responseString = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<GeocodeResponse>(responseString);
        }

        /// <summary>
        /// Builds request url
        /// </summary>
        /// <param name="address1"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="country"></param>
        /// <param name="postalCode"></param>
        /// <returns></returns>
        private string BuildRequestUrl(string address1, string city, string state, string country, string postalCode)
        {
            var address = "";
            address += string.IsNullOrWhiteSpace(address1) ? "" : $"{EscapeString(address1)}+";
            address += $"{EscapeString(city)}+{EscapeString(state)}";
            if (!string.IsNullOrWhiteSpace(country) && country.ToUpper() != "USA" && country.ToUpper() != "US")
            {
                address += $"+{EscapeString(country)}";
            }
            address += string.IsNullOrWhiteSpace(postalCode) ? "" : $"+{EscapeString(postalCode)}";

            return $"{_baseUrl}?address={address}&key={_apiKey}";
        }

        /// <summary>
        /// Escapes a string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string EscapeString(string value)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = Uri.EscapeUriString(value);
            }

            return result;
        }

        /// <summary>
        /// Checks if a response is valid
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private bool IsValid(LoadStopData stop, GeocodeResponse response)
        {
            var result = false;
            if (response?.Results != null)
            {
                foreach (var item in response.Results)
                {
                    if (item?.AddressComponents != null)
                    {
                        var address = new GeocodeAddress(item);
                        result = DoesAddressMatchResult(stop, address);
                        if (result)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if city, state, and country match address
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool DoesAddressMatchResult(LoadStopData stop, GeocodeAddress address)
        {
            var isValid = false;
            if (address != null)
            {
                if (address.City != null && address.State != null && address.Country != null)
                {
                    stop.Country = ConvertCountry(stop.Country);

                    isValid =
                        ConvertStringForComparison(address.City).Equals(ConvertStringForComparison(stop.City),
                            StringComparison.OrdinalIgnoreCase)
                        && ConvertStringForComparison(address.State).Equals(ConvertStringForComparison(stop.State),
                            StringComparison.OrdinalIgnoreCase)
                        && ConvertStringForComparison(address.Country).Equals(ConvertStringForComparison(stop.Country),
                            StringComparison.OrdinalIgnoreCase);
                }

                if (isValid && !string.IsNullOrWhiteSpace(stop.Address1))
                {
                    isValid = ConvertStringForComparison(address.Address1)
                        .Equals(ConvertStringForComparison(stop.Address1), StringComparison.OrdinalIgnoreCase);
                }

                if (isValid && !string.IsNullOrWhiteSpace(stop.PostalCode))
                    isValid = ConvertStringForComparison(address.PostalCode)
                        .Equals(ConvertStringForComparison(stop.PostalCode), StringComparison.OrdinalIgnoreCase);
            }

            return isValid;
        }

        /// <summary>
        /// Converts country to compare against GeocodeResponse
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        private string ConvertCountry(string country)
        {
            switch (country)
            {
                case "USA":
                    country = "US";
                    break;
                case "CAN":
                    country = "CA";
                    break;
                case "MEX":
                    country = "MX";
                    break;
            }

            return country;
        }

        /// <summary>
        /// Converts a string for comparison
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ConvertStringForComparison(string value)
        {
            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(value))
            {
                result = value.ToUpper();

                //remove township
                result = result.Replace("TOWNSHIP OF ", "").Replace(" TOWNSHIP", "");
                //convert to abbreviations
                result = result.Replace("MOUNT ", "MT ").Replace("SAINT ", "ST ").Replace("FORT ", "FT ");
                //remove special characters
                result = result?.Replace(" ", "").Replace(".", "").Replace("'", "");
            }

            return result?.Trim();
        }
    }
}
