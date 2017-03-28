using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MileEyes.Services.Extensions;
using MileEyes.Services.Models;
using Plugin.Geolocator.Abstractions;

namespace MileEyes.Services
{

    public class TrackerService : ITrackerService
    {

        public Position CurrentLocation { get; private set; }
        public double CurrentDistance { get; private set; }

        public IEnumerable<Waypoint> CurrentWaypoints => waypoints;

        public event EventHandler HasMoved;
        public event EventHandler Started;
        public event EventHandler Cancelled;
        public event EventHandler<Journey> Stopped;
        public event EventHandler<string> StartFailed;

        public static bool IsTracking;



        private static int MIN_MOVED_DISTANCE = 2; // The minmum distance the user has to move before we push another waypoint in meters.

        private static int MIN_SLOW_MOVED_DISTANCE = 6;

        private static int MIN_SLOW_MOVED_TIME = 14;

        private static int POLL_DELAY = 2000; // How often we poll the gps for current position.

        private static int MAX_CATCHED_POSITIONS = 30; // Max amount of catched positions between pushes.

        private static int GPS_ACCURACY = 1; // The accuracy of the gps.

        private static int MAX_CURRENT_LOCATION_TRYS = 4;

        private static double MAX_MILES_PER_HOUR = 75.0;

        private static double MAX_ACCELERATION = 38.0;

        private static double START_ACCELERTION = 3.0;

        private static double MAX_ACCELERATION_TIMEOUT = 4;

        private static double MAX_ACCURACY = 60.0;

        private static double MAX_ACCURACY_TIMEOUT = 6.0;

        private static double START_ACCURACY = 20.0;

        private static double MIN_SPEED = 4.9; // 6.9

        private static double METERS_PER_SECOND = 0.44704;

        private List<Waypoint> waypoints;

        private List<Position> catchedPositions;

        public async Task Reset()
        {
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            waypoints = new List<Waypoint>();
            catchedPositions = new List<Position>(MAX_CATCHED_POSITIONS);
            CurrentDistance = 0.0;
            CurrentLocation = null;
        }

        public async Task Start()
        {
            if (!Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationAvailable || !Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled)
            {
                StartFailed?.Invoke(this, "GPS is not enabled on this device.");
                return;
            }

            await Reset();


            Plugin.Geolocator.CrossGeolocator.Current.AllowsBackgroundUpdates = true;
            Plugin.Geolocator.CrossGeolocator.Current.DesiredAccuracy = GPS_ACCURACY;

            int trys = 0;
            catchedPositions.Clear();
            while (trys <= MAX_CURRENT_LOCATION_TRYS)
            {
                CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None);
                if (CurrentLocation != null)
                {
                    catchedPositions.Add(CurrentLocation);
                    if (CurrentLocation.Accuracy < START_ACCURACY)
                    {
                        break;
                    }
                }
                trys++;
            }

            CurrentLocation = null;
            if (catchedPositions.Count != 0)
            {
                double accuracy = 100.0;
                foreach (Position position in catchedPositions)
                {
                    if (position.Accuracy < accuracy)
                    {
                        accuracy = position.Accuracy;
                        CurrentLocation = position;
                    }
                }

            }
            if (CurrentLocation == null)
            {
                CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None);
            }
            if (CurrentLocation == null || catchedPositions.Count == 0)
            {
                StartFailed?.Invoke(this, "Currently unable to connection to GPS.");
                return;
            }

            waypoints.Add(new Waypoint
            {
                Step = 0,
                Latitude = CurrentLocation.Latitude,
                Longitude = CurrentLocation.Longitude,
                Timestamp = DateTimeOffset.UtcNow
            });

            Plugin.Geolocator.CrossGeolocator.Current.PositionChanged += updatePosition;

			var started = await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(POLL_DELAY, 0);

            if (started)
            {
                Started?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StartFailed?.Invoke(this, "There was a problem starting GPS Services on your device.");
            }
        }

        private async void updatePosition(object sender, PositionEventArgs e)
        {
            var last = CurrentLocation.Timestamp.Ticks / TimeSpan.TicksPerMillisecond;
            var current = e.Position.Timestamp.Ticks / TimeSpan.TicksPerMillisecond;
            var delta = (current - last) / 1000.0;

            if (delta > 0)
            {

                Coordinates origin = new Coordinates() { Latitude = CurrentLocation.Latitude, Longitude = CurrentLocation.Longitude };
                Coordinates destination = new Coordinates() { Latitude = e.Position.Latitude, Longitude = e.Position.Longitude };
                var distance = origin.DistanceTo(destination, UnitOfLength.Meters);

                var speed = 0.0;
                var acceleration = 0.0;

                if (delta >= 1.0)
                {
                    speed = distance / delta;
                    acceleration = (speed - CurrentLocation.Speed) / delta;
                }
                else
                {
                    speed = distance * delta;
                    acceleration = (speed - CurrentLocation.Speed) * delta;
                }

                var accuracy = e.Position.Accuracy;

                var maxDistance = 0.0;

                if (delta >= 1.0)
                {
                    maxDistance = (METERS_PER_SECOND * MAX_MILES_PER_HOUR) * delta;
                }
                else
                {
                    maxDistance = (METERS_PER_SECOND * MAX_MILES_PER_HOUR) / delta;
                }

                var currentMaxAccuracy = START_ACCURACY + (((MAX_ACCURACY - START_ACCURACY) / MAX_ACCURACY_TIMEOUT) * delta);

                var currentMaxAcceleration = START_ACCELERTION + (((MAX_ACCELERATION - START_ACCELERTION) / MAX_ACCELERATION_TIMEOUT) * delta);

                if (currentMaxAccuracy > MAX_ACCURACY)
                {
                    currentMaxAccuracy = MAX_ACCURACY;
                }


                if (currentMaxAcceleration > MAX_ACCELERATION)
                {
                    currentMaxAcceleration = MAX_ACCELERATION;
                }

                System.Diagnostics.Debug.WriteLine("Delta: " + delta + ", Current Accuracy: " + currentMaxAccuracy + ", acceleration: " + acceleration + ", max: " + currentMaxAcceleration + ", speed: " + speed + ", maxDistance: " + maxDistance);

                if ((MIN_MOVED_DISTANCE < distance && currentMaxAcceleration >= acceleration && currentMaxAccuracy >= accuracy) && (speed >= MIN_SPEED || MIN_SLOW_MOVED_DISTANCE < distance && delta >= MIN_SLOW_MOVED_TIME))
                {
                    currentMaxAccuracy = START_ACCURACY;
                    currentMaxAcceleration = START_ACCELERTION;

                    CurrentLocation = e.Position;
                    CurrentLocation.Speed = speed;

                    waypoints.Add(new Waypoint
                    {
                        Step = waypoints.Count + 1,
                        Latitude = CurrentLocation.Latitude,
                        Longitude = CurrentLocation.Longitude,
                        Timestamp = CurrentLocation.Timestamp
                    });

                    CurrentDistance += distance;
                }
            }
            HasMoved?.Invoke(this, EventArgs.Empty);
        }

        public async Task Stop()
        {
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            int trys = 0;
            catchedPositions.Clear();
            while (trys <= MAX_CURRENT_LOCATION_TRYS)
            {
                CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None);
                if (CurrentLocation != null)
                {
                    catchedPositions.Add(CurrentLocation);
                    if (CurrentLocation.Accuracy < START_ACCURACY)
                    {
                        break;
                    }
                }
                trys++;
            }
            CurrentLocation = null;
            if (catchedPositions.Count != 0)
            {
                double accuracy = 100.0;
                foreach (Position position in catchedPositions)
                {
                    if (position.Accuracy < accuracy)
                    {
                        accuracy = position.Accuracy;
                        CurrentLocation = position;
                    }
                }

            }
            if (CurrentLocation == null)
            {
                CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None);
            }

            waypoints.Add(new Waypoint
            {
                Step = waypoints.Count + 1,
                Latitude = CurrentLocation.Latitude,
                Longitude = CurrentLocation.Longitude,
                Timestamp = DateTimeOffset.UtcNow
            });
            var j = new Journey()
            {
                Date = waypoints.OrderBy(w => w.Step).FirstOrDefault().Timestamp
            };
            List<Waypoint> temp = waypoints;
            Waypoint first, last;
            first = temp.ElementAt(0);
            last = temp.ElementAt(temp.Count - 1);
            temp.RemoveAt(0);
            temp.RemoveAt(temp.Count - 1);
            temp = filterWaypoints(temp, MIN_MOVED_DISTANCE);
            List<Waypoint> newWaypoints = new List<Waypoint>();
            newWaypoints.Add(first);
            newWaypoints.AddRange(temp);
            newWaypoints.Add(last);
            waypoints = newWaypoints;
            for (var i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].Step = i;
            }
            foreach (var w in waypoints)
            {
                j.Waypoints.Add(w);
            }
            IsTracking = false;
            Stopped?.Invoke(this, j);
        }

        public async Task Cancel()
        {
            IsTracking = false;
            if (Plugin.Geolocator.CrossGeolocator.Current.IsListening)
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        private static List<Waypoint> filterWaypoints(List<Waypoint> waypoints, double distance)
        {
            if (waypoints.Count < 3)
            {
                return waypoints;
            }
            var n_source = 0;
            var n_stack = 0;
            var n_dest = 0;
            var start = 0;
            var end = 0;
            var i = 0;
            var sig = 0;

            double dev_sqr = 0;
            double max_dev_sqr = 0;
            double band_sqr = 0;

            double x12 = 0;
            double y12 = 0;
            double d12 = 0;
            double x13 = 0;
            double y13 = 0;
            double d13 = 0;
            double x23 = 0;
            double y23 = 0;
            double d23 = 0;

            var F = ((Math.PI / 180.0) * 0.5);

            int[] index = new int[waypoints.Count];
            int[] sig_start = new int[waypoints.Count];
            int[] sig_end = new int[waypoints.Count];

            n_source = waypoints.Count;
            band_sqr = distance * 360.0 / (2.0 * Math.PI * 6378137.0);
            band_sqr *= band_sqr;
            n_dest = 0;
            sig_start[0] = 0;
            sig_end[0] = n_source - 1;
            n_stack = 1;

            while (n_stack > 0)
            {
                start = sig_start[n_stack - 1];
                end = sig_end[n_stack - 1];
                n_stack--;

                if (end - start > 1)
                {

                    x12 = waypoints[end].Longitude - waypoints[start].Longitude;
                    y12 = waypoints[end].Latitude - waypoints[start].Latitude;
                    if (Math.Abs(x12) > 180.0)
                    {
                        x12 = 360.0 - Math.Abs(x12);
                    }
                    x12 *= Math.Cos(F * (waypoints[end].Latitude + waypoints[start].Latitude));
                    d12 = (x12 * x12) + (y12 * y12);

                    for (i = start + 1, sig = start, max_dev_sqr = -1.0; i < end; i++)
                    {

                        x13 = waypoints[i].Longitude - waypoints[start].Longitude;
                        y13 = waypoints[i].Latitude - waypoints[start].Latitude;
                        if (Math.Abs(x13) > 180.0)
                        {
                            x13 = 360.0 - Math.Abs(x13);
                        }
                        x13 *= Math.Cos(F * (waypoints[i].Latitude + waypoints[start].Latitude));
                        d13 = (x13 * x13) + (y13 * y13);

                        x23 = waypoints[i].Longitude - waypoints[end].Longitude;
                        y23 = waypoints[i].Latitude - waypoints[end].Latitude;
                        if (Math.Abs(x23) > 180.0)
                        {
                            x23 = 360.0 - Math.Abs(x23);
                        }
                        x23 *= Math.Cos(F * (waypoints[i].Latitude + waypoints[end].Latitude));
                        d23 = (x23 * x23) + (y23 * y23);

                        if (d13 >= (d12 + d13))
                        {
                            dev_sqr = d23;
                        } else if (d23 >= (d12 + d13))
                        {
                            dev_sqr = d13;
                        } else
                        {
                            dev_sqr = (x13 * y12 - y13 * x12) * (x13 * y12 - y13 * x12) / d12;
                        }

                        if (dev_sqr > max_dev_sqr)
                        {
                            sig = i;
                            max_dev_sqr = dev_sqr;
                        }
                    }

                    if (max_dev_sqr < band_sqr)
                    {
                        index[n_dest] = start;
                        n_dest++;
                    } else
                    {
                        n_stack++;
                        sig_start[n_stack - 1] = sig;
                        sig_end[n_stack - 1] = end;
                        n_stack++;
                        sig_start[n_stack - 1] = start;
                        sig_end[n_stack - 1] = sig;
                    }

                } else
                {
                    index[n_dest] = start;
                    n_dest++;
                }
            }

            index[n_dest] = n_source - 1;
            n_dest++;

            List<Waypoint> newWaypoints = new List<Waypoint>();

            for (i = 0; i < n_dest; i++)
            {
                newWaypoints.Add(waypoints[index[i]]);
            }

            return newWaypoints;
        }
    }
}