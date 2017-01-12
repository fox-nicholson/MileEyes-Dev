using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;
using Newtonsoft.Json;

namespace MileEyes.Services
{
    public class GeocodingService : IGeocodingService
    {
        // PRODUCTION KEY
        private const string key = "AIzaSyArLAcqpQ1v_IxC_o0Qo41SYPUlGxKtMtI";

        // BEVS KEY
        //private const string key = "AIzaSyAxtO1tAyx-0SQcBhEUU_XP4cnyOFWixZs";

        public async Task<IEnumerable<Address>> AddressLookup(string input)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + input + "&components=country:GB&sensor=true&&key=" + key;

            var response = await Host.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return new Address[1] { new Address() { PlaceId = "", Label = "Unknown Address" } };
            }

            var result = JsonConvert.DeserializeObject<ReverseGeocodeResult>(response);

            if (!result.results.Any())
                return new Address[1] { new Address() { PlaceId = "", Label = "Unknown Address" } };

            return result.results.Select(a => new Address()
            {
                PlaceId = a.place_id,
                Label = a.formatted_address,
                Latitude = a.geometry.location.lat,
                Longitude = a.geometry.location.lng
            });
        }

        public async Task<string> GetPlaceId(double lat, double lng)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + key;

            var response = await Host.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return "";
            }

            var geocodeResult = JsonConvert.DeserializeObject<GeocodeResult>(response);

            if (geocodeResult == null) return null;

            return !geocodeResult.results.Any() ? null : geocodeResult.results.First().place_id;
        }

        public async Task<string> GetPlaceId(string address)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address +
                "&components=country:GB&key=" + key;

            var response = await Host.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return "";
            }

            var geocodeResult = JsonConvert.DeserializeObject<GeocodeResult>(response);

            if (geocodeResult == null) return null;

            return !geocodeResult.results.Any() ? null : geocodeResult.results.First().place_id;
        }

        public async Task<Address> GetAddress(string placeId)
        {
            var url = "https://maps.googleapis.com/maps/api/place/details/json?placeid=" + placeId +
                "&components=country:GB&key=" + key;

            var response = await Host.HttpHelper.FileGetContents(url);

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

        public async Task<Address> GetAddress(double lat, double lng)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + key;

            var response = await Host.HttpHelper.FileGetContents(url);

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

            if (geocodeResult.result == null)
            {
                var result = geocodeResult.results.FirstOrDefault();
                return new Address()
                {
                    PlaceId = result.place_id,
                    Label = result.formatted_address,
                    Latitude = result.geometry.location.lat,
                    Longitude = result.geometry.location.lng
                };
            }
            else
            {
                return new Address()
                {
                    PlaceId = geocodeResult.result.place_id,
                    Label = geocodeResult.result.formatted_address,
                    Latitude = geocodeResult.result.geometry.location.lat,
                    Longitude = geocodeResult.result.geometry.location.lng
                };
            }
        }

        public async Task<double[]> GetCoordinates(string placeId)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?place_id=" + placeId +
                   "&key=" + key;

            var response = await Host.HttpHelper.FileGetContents(url);

            if (string.IsNullOrEmpty(response))
            {
                return new[] { 0D, 0D };
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

        public async Task<string> GetLocality(double lat, double lng)
        {
            var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + key;

            var response = await Host.HttpHelper.FileGetContents(url);

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
                        var administrativeLevel = result.address_components.FirstOrDefault(a => a.types.Contains("administrative_area_level_2"));

                        return administrativeLevel == null ? "Unknown Location" : administrativeLevel.long_name;
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
            };
        }

        public async Task<double> GetDistanceFromGoogle(double[] origin, double[] destination)
        {
            var url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + origin[0] + "," + origin[1] + "&destination=" +
                     destination[0] + "," + destination[1] + "&sensor=true&units=metric";

            var content = await Host.HttpHelper.FileGetContents(url);

            var distanceResult = JsonConvert.DeserializeObject<DistanceResult>(content);

            var route = distanceResult?.routes.FirstOrDefault();

            var leg = route?.legs.FirstOrDefault();

            return leg?.distance.value ?? 0;
        }

        public async Task<double> GetDistanceFromGoogle(string origin, string destination)
        {
            var startLatLng = await GetAddress(origin);
            var endLatLng = await GetAddress(destination);

            var url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + startLatLng.Latitude + "," + startLatLng.Longitude + "&destination=" +
                     endLatLng.Latitude + "," + endLatLng.Longitude + "&sensor=true&units=metric";

            var content = await Host.HttpHelper.FileGetContents(url);

            var t = content;

            var distanceResult = JsonConvert.DeserializeObject<DistanceResult>(content);

            var route = distanceResult?.routes.FirstOrDefault();

            var leg = route?.legs.FirstOrDefault();

            return leg?.distance.value ?? 0;
        }

        public async Task<Leg> GetDirectionsFromGoogle(double[] origin, double[] destination)
        {
            var url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + origin[0] + "," + origin[1] + "&destination=" +
                     destination[0] + "," + destination[1] + "&sensor=true&units=metric";

            var content = await Host.HttpHelper.FileGetContents(url);

            var distanceResult = JsonConvert.DeserializeObject<DistanceResult>(content);

            var route = distanceResult?.routes.FirstOrDefault();

            return route?.legs.FirstOrDefault();
        }

        public async Task<WeatherResponse> GetWeather(double lat, double lng)
        {
            var url = "http://api.openweathermap.org/data/2.5/weather?lat=" + lat + "&lon=" + lng +
                      "&appid=a08866147f5b57cc8b5a0fcf000ef1be";
            
            var content = await Host.HttpHelper.FileGetContents(url);

            var weatherResult = JsonConvert.DeserializeObject<WeatherResponse>(content);

            return weatherResult;
        }
    }
}
