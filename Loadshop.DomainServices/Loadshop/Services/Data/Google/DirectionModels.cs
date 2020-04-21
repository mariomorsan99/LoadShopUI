using System.Collections.Generic;


// Copied from Tops 2 Go

namespace Loadshop.DomainServices.Loadshop.Services.Data.Google
{
    public class GeocodedWaypoint
    {
        public string geocoder_status { get; set; }
        public string place_id { get; set; }
        public List<string> types { get; set; }
    }

    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Bounds
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Distance
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class EndLocation : Location
    {
        public EndLocation() { }
        public EndLocation(string latitude, string longitude) : base(latitude, longitude) { }
        public EndLocation(double latitude, double longitude) : base(latitude, longitude) { }
    }

    public class MidLocation : Location
    {
        public MidLocation() { }
        public MidLocation(string latitude, string longitude) : base(latitude, longitude) { }
        public MidLocation(double latitude, double longitude) : base(latitude, longitude) { }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }

        public Location() { }
        public Location(string latitude, string longitude)
        {
            if (double.TryParse(latitude, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat1))
                lat = lat1;
            if (double.TryParse(longitude, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng1))
                lng = lng1;
        }
        public Location(double latitude, double longitude)
        {
            lat = latitude;
            lng = longitude;
        }
    }

    public class StartLocation : Location
    {
        public StartLocation() { }
        public StartLocation(string latitude, string longitude) : base(latitude, longitude) { }
        public StartLocation(double latitude, double longitude) : base(latitude, longitude) { }
    }

    public class Distance2
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration2
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class EndLocation2
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Polyline
    {
        public string points { get; set; }
    }

    public class StartLocation2
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Step
    {
        public Distance2 distance { get; set; }
        public Duration2 duration { get; set; }
        public EndLocation2 end_location { get; set; }
        public string html_instructions { get; set; }
        public Polyline polyline { get; set; }
        public StartLocation2 start_location { get; set; }
        public string travel_mode { get; set; }
        public string maneuver { get; set; }
    }

    public class Leg
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public string end_address { get; set; }
        public EndLocation end_location { get; set; }
        public string start_address { get; set; }
        public StartLocation start_location { get; set; }
        public List<Step> steps { get; set; }
        public List<object> via_waypoint { get; set; }
    }

    public class OverviewPolyline
    {
        public string points { get; set; }
    }

    public class Route
    {
        public Bounds bounds { get; set; }
        public string copyrights { get; set; }
        public List<Leg> legs { get; set; }
        public OverviewPolyline overview_polyline { get; set; }
        public string summary { get; set; }
        public List<object> warnings { get; set; }
        public List<object> waypoint_order { get; set; }
    }

    public class GoogleDirectionClass
    {
        public List<GeocodedWaypoint> geocoded_waypoints { get; set; }
        public List<Route> routes { get; set; }
        public string status { get; set; }
    }
}
