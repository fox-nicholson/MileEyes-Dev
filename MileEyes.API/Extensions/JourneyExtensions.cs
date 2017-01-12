using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MileEyes.API.Helpers;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Extensions
{
    public static class JourneyExtensions
    {
        public static async Task<double> Distance(this Journey journey)
        {
            return await CalculateJourneyDistance(journey.Waypoints);
        }

        public static async Task<double> CalculateJourneyDistance(IEnumerable<Waypoint> waypoints)
        {
            var distance = await Task.Run(async () =>
            {
                var d = 0.0;
                var enumerable = waypoints as Waypoint[] ?? waypoints.OrderBy(w => w.Step).ToArray();

                if (enumerable.Count() > 2)
                {
                    Waypoint lastWaypoint = null;

                    foreach (var w in enumerable)
                    {
                        if (lastWaypoint != null)
                        {
                            d += CalculateBetweenLegs(lastWaypoint.Address.Coordinates,
                                w.Address.Coordinates);
                        }

                        lastWaypoint = w;
                    }
                }
                else
                {
                    var start = enumerable.OrderBy(w => w.Step).First();
                    var end = enumerable.OrderBy(w => w.Step).Last();

                    d = await Services.GeocodingService.GetDistanceFromGoogle(
                        new[] { start.Address.Coordinates.Latitude, start.Address.Coordinates.Longitude },
                        new[] { end.Address.Coordinates.Latitude, end.Address.Coordinates.Longitude });
                }

                return d;
            });

            return distance;
        }

        public static double CalculateBetweenLegs(Coordinates origin, Coordinates destination)
        {
            var distance = origin.DistanceTo(destination, UnitOfLength.Miles);

            return distance;
        }

        public static decimal CalculateFuelVat(this Journey journey)
        {
            return Convert.ToDecimal(Units.MetersToMiles(journey.Distance)) * (journey.Vehicle.EngineType.FuelRate * 0.20M);
        }

        public static decimal CalculateCost(this Journey journey)
        {
            var underMiles = 0.0;
            var overMiles = 0.0;

            var currentMiles = journey.Driver.Journeys.Sum(j => j.Distance);

            var newMiles = currentMiles + journey.Distance;
            var ceiling = Units.MilesToMeters(10000);

            if (currentMiles > ceiling)
            {
                overMiles = journey.Distance;
            }
            else
            {
                if (newMiles > ceiling)
                {
                    underMiles = ceiling - currentMiles;
                    overMiles = newMiles - ceiling;
                }
                else
                {
                    underMiles = journey.Distance;
                }
            }

            var underCost = CalculateCost(underMiles, journey.Company.HighRate);
            var overCost =  CalculateCost(overMiles, journey.Company.LowRate);

            return underCost + overCost;
        }

        public static decimal CalculateCost(double distance, decimal rate)
        {
            return Convert.ToDecimal(Units.MetersToMiles(distance)) * rate;
        }
    }
}