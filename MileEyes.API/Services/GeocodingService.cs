using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.API.Models.GeocodingModels;
using Newtonsoft.Json;
using System.Linq;

namespace MileEyes.API.Services
{
    public class GeocodingService
    {
        private static string key = "AIzaSyArLAcqpQ1v_IxC_o0Qo41SYPUlGxKtMtI";

        public static async Task<IEnumerable<Address>> AddressLookup(string input)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + input +
                      "&components=country:GB&sensor=true&&key=" + key;

            var response = await Helpers.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return new Address[1] {new Address() {PlaceId = "", Label = "Unknown Address"}};
            }

            var result = JsonConvert.DeserializeObject<ReverseGeocodeResult>(response);

            if (!result.results.Any())
                return new Address[1] {new Address() {PlaceId = "", Label = "Unknown Address"}};

            return result.results.Select(a => new Address()
            {
                PlaceId = a.place_id,
                Label = a.formatted_address,
                Latitude = a.geometry.location.lat,
                Longitude = a.geometry.location.lng
            });
        }

        public static async Task<string> GetPlaceId(double lat, double lng)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + key;

            var response = await Helpers.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return "";
            }

            var geocodeResult = JsonConvert.DeserializeObject<GeocodeResult>(response);

            if (geocodeResult == null) return null;

            return !geocodeResult.results.Any() ? null : geocodeResult.results.First().place_id;
        }

        public static async Task<string> GetPlaceId(string address)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address +
                      "&components=country:GB&key=" + key;

            var response = await Helpers.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return "";
            }

            var geocodeResult = JsonConvert.DeserializeObject<GeocodeResult>(response);

            if (geocodeResult == null) return null;

            return !geocodeResult.results.Any() ? null : geocodeResult.results.First().place_id;
        }

        public static async Task<Address> GetAddress(string placeId)
        {
            var url = "https://maps.googleapis.com/maps/api/place/details/json?placeid=" + placeId +
                      "&components=country:GB&key=" + key;

            var response = await Helpers.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return new Address()
                {
                    PlaceId = "",
                    Label = "Unknown Location",
                    Latitude = 0D,
                    Longitude = 0D
                };
            }

            var geocodeResult = JsonConvert.DeserializeObject<GeocodeResult>(response);

            if (geocodeResult == null) return null;

            return new Address()
            {
                PlaceId = geocodeResult.result.place_id,
                Label = geocodeResult.result.formatted_address,
                Latitude = geocodeResult.result.geometry.location.lat,
                Longitude = geocodeResult.result.geometry.location.lng
            };
        }

        public static async Task<Address> GetAddress(double lat, double lng)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + key;

            var response = await Helpers.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return new Address()
                {
                    PlaceId = "",
                    Label = "Unknown Location",
                    Latitude = 0D,
                    Longitude = 0D
                };
            }

            var geocodeResult = JsonConvert.DeserializeObject<GeocodeResult>(response);

            if (geocodeResult == null) return null;
            if (geocodeResult.result != null)
            {
                return new Address()
                {
                    PlaceId = geocodeResult.result.place_id,
                    Label = geocodeResult.result.formatted_address,
                    Latitude = geocodeResult.result.geometry.location.lat,
                    Longitude = geocodeResult.result.geometry.location.lng
                };
            }
            else
            {
                if (geocodeResult.results == null || geocodeResult.results.Length < 1) return null;

                var geoResult = geocodeResult.results[0];
                return new Address()
                {
                    PlaceId = geoResult.place_id,
                    Label = geoResult.formatted_address,
                    Latitude = geoResult.geometry.location.lat,
                    Longitude = geoResult.geometry.location.lng
                };
            }
        }

        public static async Task<double[]> GetCoordinates(string placeId)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?place_id=" + placeId +
                      "&key=" + key;

            var response = await Helpers.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return new[] {0D, 0D};
            }

            var geocodeResult = JsonConvert.DeserializeObject<GeocodeResult>(response);
            if (geocodeResult == null) return null;

            return !geocodeResult.results.Any()
                ? null
                : new double[]
                {
                    geocodeResult.results.First().geometry.location.lat,
                    geocodeResult.results.First().geometry.location.lat
                };
        }

        public static async Task<string> GetLocality(double lat, double lng)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + key;

            var response = await Helpers.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return "Unknown Location";
            }

            var googleresponse = JsonConvert.DeserializeObject<GeocodeResult>(response);
            var result = googleresponse.results.FirstOrDefault();

            if (result == null)
            {
                return "Unknown Location";
            }
            else
            {
                var postalTown = result.address_components.FirstOrDefault(a => a.types.Contains("postal_town"));

                if (postalTown == null)
                {
                    var locality = result.address_components.FirstOrDefault(a => a.types.Contains("locality"));

                    if (locality == null)
                    {
                        var administrativeLevel =
                            result.address_components.FirstOrDefault(
                                a => a.types.Contains("administrative_area_level_2"));

                        return administrativeLevel == null ? "Unknown Address" : administrativeLevel.long_name;
                    }
                    else
                    {
                        return locality.long_name;
                    }
                }
                else
                {
                    return result.address_components.First(a => a.types.Contains("postal_town")).long_name;
                }
            }
        }

        public static async Task<double> GetDistanceFromGoogle(double[] origin, double[] destination)
        {
            var url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + origin[0] + "," + origin[1] +
                      "&destination=" +
                      destination[0] + "," + destination[1] + "&sensor=true&units=metric";

            var content = await Helpers.HttpHelper.FileGetContents(url);

            var distanceResult = JsonConvert.DeserializeObject<DistanceResult>(content);

            var route = distanceResult?.routes.FirstOrDefault();

            var leg = route?.legs.FirstOrDefault();

            return leg?.distance.value ?? 0;
        }

        public static async Task<Leg> GetDirectionsFromGoogle(double[] origin, double[] destination)
        {
            var url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + origin[0] + "," + origin[1] +
                      "&destination=" +
                      destination[0] + "," + destination[1] + "&sensor=true&units=metric";

            var content = await Helpers.HttpHelper.FileGetContents(url);

            var distanceResult = JsonConvert.DeserializeObject<DistanceResult>(content);

            var route = distanceResult?.routes.FirstOrDefault();

            return route?.legs.FirstOrDefault();
        }
    }
}