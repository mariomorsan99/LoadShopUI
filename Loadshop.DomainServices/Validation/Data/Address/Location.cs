using Newtonsoft.Json;
using System.Globalization;

namespace Loadshop.DomainServices.Validation.Data.Address
{
    public class Location
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }

        public string Address { get; set; }

        public Location()
        {
        }

        public Location(string address)
            : this()
        {
            this.Address = address;
        }

        public Location(double latitude, double longitude)
            : this()
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public override string ToString()
        {
            return this.Address ?? this.Latitude.ToString(CultureInfo.InvariantCulture) + "," + this.Longitude.ToString(CultureInfo.InvariantCulture);
        }
    }
}