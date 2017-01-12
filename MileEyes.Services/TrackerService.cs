using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MileEyes.Services.Extensions;
using MileEyes.Services.Models;
using Plugin.Geolocator.Abstractions;

namespace MileEyes.Services
{
    public class TrackerService : ITrackerService
    {
        private List<Waypoint> _currentWaypoints;

        public Position CurrentLocation { get; private set; }

        /// <summary>
        /// This Distance is in Meters
        /// </summary>
        public double CurrentDistance { get; private set; }

        public IEnumerable<Waypoint> CurrentWaypoints => _currentWaypoints;

        public event EventHandler HasMoved;
        public event EventHandler Started;
        public event EventHandler Cancelled;
        public event EventHandler<Journey> Stopped;
        public event EventHandler<string> StartFailed;

        public async Task Reset()
        {
            if (_currentWaypoints == null) _currentWaypoints = new List<Waypoint>();

            _currentWaypoints.Clear();

            CurrentDistance = 0;
            CurrentLocation =
                await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None, true);

            _currentWaypoints.Add(new Waypoint
            {
                Step = 0,
                Latitude = CurrentLocation.Latitude,
                Longitude = CurrentLocation.Longitude,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        public async Task Start()
        {
            await Reset();

            if (!Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationAvailable)
            {
                StartFailed?.Invoke(this, "GPS is not available on this device.");
                return;
            }

            if (!Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled)
            {
                StartFailed?.Invoke(this, "GPS is not enabled on this device.");
                return;
            }

            Plugin.Geolocator.CrossGeolocator.Current.PositionChanged += Current_PositionChanged;

            Plugin.Geolocator.CrossGeolocator.Current.AllowsBackgroundUpdates = true;
            Plugin.Geolocator.CrossGeolocator.Current.DesiredAccuracy = 5;
            var started = await Plugin.Geolocator.CrossGeolocator.Current.StartListeningAsync(5000, 50, false);

            if (started)
            {
                Started?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StartFailed?.Invoke(this, "There was a problem starting GPS Services on your device.");
            }
        }

        private async void Current_PositionChanged(object sender, PositionEventArgs e)
        {
            CurrentLocation = e.Position;

            var nextStep = _currentWaypoints.OrderBy(w => w.Step).Last().Step + 1;

            _currentWaypoints.Add(new Waypoint
            {
                Step = nextStep,
                Latitude = CurrentLocation.Latitude,
                Longitude = CurrentLocation.Longitude,
                Timestamp = DateTimeOffset.UtcNow
            });

            if (CurrentWaypoints.Any())
            {
                var j = new Journey();
                foreach (var w in CurrentWaypoints.OrderBy(w => w.Step).ToList())
                {
                    j.Waypoints.Add(w);
                }

                CurrentDistance = await j.CalculateDistance();
            }
            else
            {
                CurrentDistance = 0;
            }

            HasMoved?.Invoke(this, EventArgs.Empty);
        }

        public async Task Stop()
        {
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();

            CurrentLocation = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000, CancellationToken.None, true);

            var nextStep = _currentWaypoints.OrderBy(w => w.Step).Last().Step + 1;

            _currentWaypoints.Add(new Waypoint
            {
                Step = nextStep,
                Latitude = CurrentLocation.Latitude,
                Longitude = CurrentLocation.Longitude,
                Timestamp = DateTimeOffset.UtcNow
            });

            var j = new Journey()
            {
                Date = _currentWaypoints.OrderBy(w => w.Step).FirstOrDefault().Timestamp
            };

            foreach (var w in _currentWaypoints.OrderBy(w => w.Step))
            {
                j.Waypoints.Add(w);
            }

            Stopped?.Invoke(this, j);
        }

        public async Task Cancel()
        {
            await Plugin.Geolocator.CrossGeolocator.Current.StopListeningAsync();

            await Reset();

            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}