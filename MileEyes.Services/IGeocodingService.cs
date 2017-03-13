using System.Collections.Generic;
using System.Threading.Tasks;
using MileEyes.Services.Models;

namespace MileEyes.Services
{
    public interface IGeocodingService
    {
        Task<IEnumerable<Address>> AddressLookup(string input);

        Task<string> GetPlaceId(double lat, double lng);
        Task<string> GetPlaceId(string address);

        Task<Address> GetAddress(string placeId);
        Task<Address> GetAddress(double lat, double lng);

        Task<double[]> GetCoordinates(string placeId);

        Task<string> GetLocality(double lat, double lng);

        Task<double> GetDistanceFromGoogle(double[] origin, double[] destination);
        Task<double> GetDistanceFromGoogle(string origin, string destination);

        Task<Leg> GetDirectionsFromGoogle(double[] origin, double[] destination);

        Task<string> GetWeather(double lat, double lng);
    }
}