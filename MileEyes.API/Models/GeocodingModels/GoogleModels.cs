using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MileEyes.API.Models.GeocodingModels
{
    public class GeocodeResult
    {
        public Result result { get; set; }
        public Result[] results { get; set; }
        public string status { get; set; }
    }

    public class ReverseGeocodeResult
    {
        public Result[] results { get; set; }
        public string status { get; set; }
    }

    public class Result
    {
        public string adr_address { get; set; }
        public string formatted_phone_number { get; set; }
        public string icon { get; set; }
        public string id { get; set; }
        public string international_phone_number { get; set; }
        public string name { get; set; }
        public Opening_Hours opening_hours { get; set; }
        public Photo[] photos { get; set; }
        public float rating { get; set; }
        public string reference { get; set; }
        public Review[] reviews { get; set; }
        public string scope { get; set; }
        public string url { get; set; }
        public int utc_offset { get; set; }
        public string vicinity { get; set; }
        public string website { get; set; }

        public Address_Components[] address_components { get; set; }
        public string formatted_address { get; set; }
        public Geometry geometry { get; set; }
        public string place_id { get; set; }
        public string[] types { get; set; }
    }

    public class Geometry
    {
        public Location location { get; set; }
        public string location_type { get; set; }
        public Viewport viewport { get; set; }
        public Bounds bounds { get; set; }
    }

    public class Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Viewport
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Northeast
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Southwest
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Bounds
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Address_Components
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public string[] types { get; set; }
    }

    public class Opening_Hours
    {
        public bool open_now { get; set; }
        public Period[] periods { get; set; }
        public string[] weekday_text { get; set; }
    }

    public class Period
    {
        public Close close { get; set; }
        public Open open { get; set; }
    }

    public class Close
    {
        public int day { get; set; }
        public string time { get; set; }
    }

    public class Open
    {
        public int day { get; set; }
        public string time { get; set; }
    }

    public class Photo
    {
        public int height { get; set; }
        public string[] html_attributions { get; set; }
        public string photo_reference { get; set; }
        public int width { get; set; }
    }

    public class Review
    {
        public Aspect[] aspects { get; set; }
        public string author_name { get; set; }
        public string author_url { get; set; }
        public string language { get; set; }
        public int rating { get; set; }
        public string text { get; set; }
        public int time { get; set; }
        public string profile_photo_url { get; set; }
    }

    public class Aspect
    {
        public int rating { get; set; }
        public string type { get; set; }
    }

    public class DistanceResult
    {
        public Geocoded_Waypoints[] geocoded_waypoints { get; set; }
        public Route[] routes { get; set; }
        public string status { get; set; }
    }

    public class Geocoded_Waypoints
    {
        public string geocoder_status { get; set; }
        public string place_id { get; set; }
        public string[] types { get; set; }
    }

    public class Route
    {
        public Bounds bounds { get; set; }
        public string copyrights { get; set; }
        public Leg[] legs { get; set; }
        public Overview_Polyline overview_polyline { get; set; }
        public string summary { get; set; }
        public object[] warnings { get; set; }
        public object[] waypoint_order { get; set; }
    }

    public class Overview_Polyline
    {
        public string points { get; set; }
    }

    public class Leg
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public string end_address { get; set; }
        public End_Location end_location { get; set; }
        public string start_address { get; set; }
        public Start_Location start_location { get; set; }
        public Step[] steps { get; set; }
        public object[] traffic_speed_entry { get; set; }
        public object[] via_waypoint { get; set; }
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

    public class End_Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Start_Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Step
    {
        public Distance1 distance { get; set; }
        public Duration1 duration { get; set; }
        public End_Location1 end_location { get; set; }
        public string html_instructions { get; set; }
        public Polyline polyline { get; set; }
        public Start_Location1 start_location { get; set; }
        public string travel_mode { get; set; }
        public string maneuver { get; set; }
    }

    public class Distance1
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration1
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class End_Location1
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class Polyline
    {
        public string points { get; set; }
    }

    public class Start_Location1
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }
}
