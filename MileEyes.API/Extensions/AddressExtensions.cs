using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Extensions
{
    public static class AddressExtensions
    {
        public static async Task<string> FindPlaceId(this Address address)
        {
            return await Services.GeocodingService.GetPlaceId(address.Coordinates.Latitude,
                address.Coordinates.Longitude);
        }

        public static async Task<string> FindAddress(this Address address)
        {
            return (await Services.GeocodingService.GetAddress(address.PlaceId)).Label;
        }
    }
}