using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Extensions
{
    public static class CoordinatesExtensions
    {
        public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates)
        {
            return DistanceTo(baseCoordinates, targetCoordinates, UnitOfLength.Meters);
        }

        public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates,
            UnitOfLength unitOfLength)
        {
            var baseRad = Math.PI * baseCoordinates.Latitude / 180;
            var targetRad = Math.PI * targetCoordinates.Latitude / 180;
            var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return unitOfLength.ConvertFromMiles(dist);
        }

        public static Coordinates FindMidpoint(this Coordinates posA, Coordinates posB)
        {
            var midPoint = new Coordinates();

            var dLon = Helpers.TrigHelpers.Deg2Rad(posB.Longitude - posA.Longitude);
            var bx = Math.Cos(Helpers.TrigHelpers.Deg2Rad(posB.Latitude)) * Math.Cos(dLon);
            var by = Math.Cos(Helpers.TrigHelpers.Deg2Rad(posB.Latitude)) * Math.Sin(dLon);

            midPoint.Latitude =
                Helpers.TrigHelpers.Rad2Deg(
                    Math.Atan2(
                        Math.Sin(Helpers.TrigHelpers.Deg2Rad(posA.Latitude)) +
                        Math.Sin(Helpers.TrigHelpers.Deg2Rad(posB.Latitude)),
                        Math.Sqrt((Math.Cos(Helpers.TrigHelpers.Deg2Rad(posA.Latitude)) + bx) *
                                  (Math.Cos(Helpers.TrigHelpers.Deg2Rad(posA.Latitude))) + bx) + by * by));

            midPoint.Longitude = posA.Longitude +
                                 Helpers.TrigHelpers.Rad2Deg(Math.Atan2(by,
                                     Math.Cos(Helpers.TrigHelpers.Deg2Rad(posA.Latitude)) + bx));

            return midPoint;
        }
    }

    public class UnitOfLength
    {
        public static UnitOfLength Meters = new UnitOfLength(1609.34);
        public static UnitOfLength Kilometers = new UnitOfLength(1.609344);
        public static UnitOfLength NauticalMiles = new UnitOfLength(0.8684);
        public static UnitOfLength Miles = new UnitOfLength(1);

        private readonly double _fromMilesFactor;

        private UnitOfLength(double fromMilesFactor)
        {
            _fromMilesFactor = fromMilesFactor;
        }

        public double ConvertFromMiles(double input)
        {
            return input * _fromMilesFactor;
        }
    }
}