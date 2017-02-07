using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.Services.Extensions
{
    public static class JourneyExtensions
    {
        public static async Task<double> CalculateDistance(this Journey journey)
        {
            return await CalculateJourneyDistance(journey.Waypoints.ToList());
        }

        public static async Task<double> CalculateJourneyDistance(IEnumerable<Waypoint> waypoints)
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
                        var newDistance = d + CalculateBetweenLegs(new Coordinates()
                        {
                            Latitude = lastWaypoint.Latitude,
                            Longitude = lastWaypoint.Longitude
                        }, new Coordinates()
                        {
                            Latitude = w.Latitude,
                            Longitude = w.Longitude
                        });

                        if (!double.IsNaN(newDistance))
                        {
                            d += CalculateBetweenLegs(new Coordinates()
                            {
                                Latitude = lastWaypoint.Latitude,
                                Longitude = lastWaypoint.Longitude
                            }, new Coordinates()
                            {
                                Latitude = w.Latitude,
                                Longitude = w.Longitude
                            });
                        }
                    }

                    lastWaypoint = w;
                }
            }
            else if (enumerable.Count() > 1)
            {
                var start = enumerable.OrderBy(w => w.Step).First();
                var end = enumerable.OrderBy(w => w.Step).Last();

                if (string.IsNullOrEmpty(start.PlaceId) && string.IsNullOrEmpty(end.PlaceId))
                {
                    d = await Host.GeocodingService.GetDistanceFromGoogle(
                        new[] { start.Latitude, start.Longitude },
                        new[] { end.Latitude, end.Longitude });
                }
                else
                {
                    d = await Host.GeocodingService.GetDistanceFromGoogle(start.PlaceId, end.PlaceId);
                }
            }
            else
            {
                d = 0;
            }

            var e = d;

            return d;
        }



        public static double CalculateBetweenLegs(Coordinates origin, Coordinates destination)
        {
            var distance = origin.DistanceTo(destination, UnitOfLength.Meters);

            return distance;
        }
    }
}
