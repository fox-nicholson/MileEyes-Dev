using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MileEyes.API.Helpers;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;

namespace MileEyes.API.Extensions
{
    public static class JourneyExtensions
    {
        /// <summary>
        /// Calculates the distance travelled on the journey
        /// </summary>
        /// <param name="journey">The Journey itself</param>
        /// <returns></returns>
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
                        new[] {start.Address.Coordinates.Latitude, start.Address.Coordinates.Longitude},
                        new[] {end.Address.Coordinates.Latitude, end.Address.Coordinates.Longitude});
                }

                return d;
            });

            return distance;
        }

        /// <summary>
        /// Calculates the distance between 2 GPS Coordinates
        /// </summary>
        /// <param name="origin">Start Location</param>
        /// <param name="destination">End Location</param>
        /// <returns></returns>
        public static double CalculateBetweenLegs(Coordinates origin, Coordinates destination)
        {
            var distance = origin.DistanceTo(destination, UnitOfLength.Miles);

            return distance;
        }

        public static decimal CalculateFuelVat(this Journey journey)
        {
            // Fuel VAT is the distance multiplied by the result of the calculation fuelrate multiplied by 0.20
            return Convert.ToDecimal(Units.MetersToMiles(journey.Distance)) *
                   (journey.Vehicle.EngineType.FuelRate * 0.20M);
        }

        public static CostModel CalculateCost(this Journey journey, double currentMileage)
        {
            var underMiles = 0.0;
            var overMiles = 0.0;
            var result = new CostModel();

            result.Distance = journey.Distance;

            var acceptedJourneys = journey.Driver.Journeys.Where(a => a.Accepted);

            var journeysToProcess = acceptedJourneys.Where(p => DateTimeOffset.Compare(p.Modified, journey.Modified) < 0);

            var currentMiles = currentMileage + journeysToProcess.Sum(j => j.Distance);

            

            // What is the number of miles after adding this journey distance onto the existing distance
            var newMiles = currentMiles + journey.Distance;
            // Rate cut off distance
            var ceiling = Units.MilesToMeters(10000);

            // Does the current miles travelled go over the cut off
            if (currentMiles > ceiling)
            {
                // Set over miles to the amount it went over
                overMiles = journey.Distance;
                result.OverDistance = overMiles;
            }
            else
            {
                // Is the New miles over the cutoff
                if (newMiles > ceiling)
                {
                    underMiles = ceiling - currentMiles;
                    overMiles = newMiles - ceiling;
                    result.UnderDistance = underMiles;
                    result.OverDistance = overMiles;
                }
                // Its under
                else
                {
                    underMiles = journey.Distance;
                    result.UnderDistance = underMiles;
                }
            }

            // Calculate the under cut off cost
            var underCost = CalculateCost(underMiles, journey.Company.HighRate);
            result.UnderCost = underCost;
            // Calculate the over cut off cost
            var overCost = CalculateCost(overMiles, journey.Company.LowRate);
            result.OverCost = overCost;

            result.Cost = underCost + overCost;

            // Return the combined sum
            return result;
        }

        public static decimal CalculateCost(double distance, decimal rate)
        {
            return Convert.ToDecimal(Units.MetersToMiles(distance)) * rate;
        }
    }
}