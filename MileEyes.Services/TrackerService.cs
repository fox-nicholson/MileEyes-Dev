using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MileEyes.Services.Extensions;
using MileEyes.Services.Models;
using Plugin.Geolocator.Abstractions;

namespace MileEyes.Services {
	
    public class TrackerService : ITrackerService {

		public Position CurrentLocation { get; private set; }
		public double CurrentDistance { get; private set; }

		public IEnumerable<Waypoint> CurrentWaypoints => waypoints;

		public event EventHandler HasMoved;
		public event EventHandler Started;
		public event EventHandler Cancelled;
		public event EventHandler<Journey> Stopped;
		public event EventHandler<string> StartFailed;

		public static bool IsTracking;



		private static int MIN_MOVED_DISTANCE = 4; // The minmum distance the user has to move before we push another waypoint in meters.

		private static int POLL_DELAY = 250; // How often we poll the gps for current position.

		private static int MAX_CATCHED_POSITIONS = 30; // Max amount of catched positions between pushes.

		private static int GPS_ACCURACY = 1; // The accuracy of the gps.

        private static double MAX_MILES_PER_HOUR = 75.0;

		private static double MAX_ACCELERATION = 4.0;

		private static double METERS_PER_SECOND = 0.44704;

        private List<Waypoint> waypoints;

		private List<Position> catchedPositions;

        public async Task Reset() {
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();
            waypoints = new List<Waypoint>();
			catchedPositions = new List<Position>(MAX_CATCHED_POSITIONS);
            CurrentDistance = 0.0;
			CurrentLocation = null;
            IsTracking = false;
        }

		public async Task Start() {
			if (!Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationAvailable || !Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled) {
				IsTracking = false;
				StartFailed?.Invoke(this, "GPS is not enabled on this device.");
				return;
			}

            await Reset();

            Plugin.Geolocator.CrossGeolocator.Current.AllowsBackgroundUpdates = true;
            Plugin.Geolocator.CrossGeolocator.Current.DesiredAccuracy = GPS_ACCURACY;

            IsTracking = true;
            CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None);

			if (CurrentLocation == null) {
				CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None);
				if (CurrentLocation == null) {
					IsTracking = false;
					StartFailed?.Invoke(this, "Currently unable to connection to GPS.");
					return;
				}
			}

			waypoints.Add(new Waypoint {
				Step = 0,
				Latitude = CurrentLocation.Latitude,
				Longitude = CurrentLocation.Longitude,
				Timestamp = DateTimeOffset.UtcNow
			});
            
            Plugin.Geolocator.CrossGeolocator.Current.PositionChanged += updatePosition;
			
			var started = await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(POLL_DELAY, MIN_MOVED_DISTANCE);

            if (started) {
                Started?.Invoke(this, EventArgs.Empty);
            } else {
                IsTracking = false;
                StartFailed?.Invoke(this, "There was a problem starting GPS Services on your device.");
            }
        }

        private async void updatePosition(object sender, PositionEventArgs e) {
			var last = CurrentLocation.Timestamp.Ticks / TimeSpan.TicksPerMillisecond;
			var current = e.Position.Timestamp.Ticks / TimeSpan.TicksPerMillisecond;
			var delta = (current - last) / 1000.0;

			if (delta > 0) {
				
				Coordinates origin = new Coordinates() { Latitude = CurrentLocation.Latitude, Longitude = CurrentLocation.Longitude };
				Coordinates destination = new Coordinates() { Latitude = e.Position.Latitude, Longitude = e.Position.Longitude };
				var distance = origin.DistanceTo(destination, UnitOfLength.Meters);

                var speed = 0.0;
                var acceleration = 0.0;

                if (delta >= 1.0) {
                    speed = distance / delta;
                    acceleration = (speed - CurrentLocation.Speed) / delta;
                } else {
                    speed = distance * delta;
                    acceleration = (speed - CurrentLocation.Speed) * delta;
                }

				var accuracy = e.Position.Accuracy;
				var accuracyDelta = CurrentLocation.Accuracy - accuracy;

				var altitude = e.Position.Altitude;
				var altitudeDelta = CurrentLocation.Altitude - altitude;

				var altitudeAccuracy = e.Position.AltitudeAccuracy;
				var altitudeAccuracyDelta = CurrentLocation.AltitudeAccuracy - altitudeAccuracy;

				var maxDistance = 0.0;

				if (delta >= 1.0) {
                    maxDistance = (METERS_PER_SECOND * MAX_MILES_PER_HOUR) / delta;
                } else {
                    maxDistance = (METERS_PER_SECOND * MAX_MILES_PER_HOUR) * delta;
                }

				System.Diagnostics.Debug.WriteLine("delta: " + delta + ", speed: " + speed + "-" + acceleration + ", accuracy: " + accuracy + "-" + accuracyDelta + ", maxDistance: " + maxDistance);

				if (MIN_MOVED_DISTANCE < distance && maxDistance > distance && MAX_ACCELERATION >= acceleration) {
					CurrentLocation = e.Position;
					CurrentLocation.Speed = speed;

					waypoints.Add(new Waypoint {
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

        public async Task Stop() {
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();

            CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None, true);

            waypoints.Add(new Waypoint {
				Step = waypoints.Count + 1,
				Latitude = CurrentLocation.Latitude,
				Longitude = CurrentLocation.Longitude,
				Timestamp = DateTimeOffset.UtcNow
			});

            var j = new Journey() {
                Date = waypoints.OrderBy(w => w.Step).FirstOrDefault().Timestamp
            };
            foreach (var w in waypoints) {
                j.Waypoints.Add(w);
            }
            IsTracking = false;
            Stopped?.Invoke(this, j);
        }

        public async Task Cancel() {
			IsTracking = false;
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}