using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MileEyes.CustomControls;
using MileEyes.Services;
using MileEyes.Services.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.ViewModels
{
    internal class JourneyCurrentViewModel : ViewModel
    {
        private bool _hasLocation;

        public bool HasLocation
        {
            get { return _hasLocation; }
            set
            {
                if (_hasLocation == value) return;
                _hasLocation = value;
                OnPropertyChanged(nameof(HasLocation));
            }
        }

        private string _currentLocation;

        public string CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                if (_currentLocation == value) return;
                _currentLocation = value;
                OnPropertyChanged(nameof(CurrentLocation));
            }
        }

        private double _currentDistance;

        public double CurrentDistance
        {
            get { return _currentDistance; }
            set
            {
                if (DoubleComparison.isEqual(_currentDistance, value)) return;
                _currentDistance = value;
                OnPropertyChanged(nameof(CurrentDistance));
            }
        }

        private TimeSpan _currentDuration;

        public TimeSpan CurrentDuration
        {
            get { return _currentDuration; }
            set
            {
                if (_currentDuration == value) return;
                _currentDuration = value;
                OnPropertyChanged(nameof(CurrentDuration));
            }
        }

        public ObservableCollection<Position> Route { get; set; } = new ObservableCollection<Position>();

        public JourneyCurrentViewModel()
        {
            Host.TrackerService.HasMoved += TrackerService_HasMoved;
            StopCommand = new Command(Stop);
            Refresh();

            Device.StartTimer(TimeSpan.FromSeconds(1), RefreshDuration);

            Device.StartTimer(TimeSpan.FromMinutes(1), UpdateLocalityCallback);
            UpdateLocalityCallback();
        }

        private void TrackerService_HasMoved(object sender, EventArgs e)
        {
            if (Host.Backgrounded) return;

            Refresh();
        }

        public override void Refresh()
        {
            base.Refresh();

            Refreshing = true;

            Route.Clear();

            foreach (var waypoint in Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step))
            {
                Route.Add(new Position(waypoint.Latitude, waypoint.Longitude));
            }

            CurrentDistance = Units.MetersToMiles(Host.TrackerService.CurrentDistance);

            Refreshing = false;
        }

        public bool UpdateLocalityCallback()
        {
            if (!Host.Backgrounded)
            {
                UpdateLocation();
            }

            return true;
        }

        public async void UpdateLocation()
        {
            var loc =
                await Host.GeocodingService.GetWeather(
                    Host.TrackerService.CurrentLocation.Latitude,
                    Host.TrackerService.CurrentLocation.Longitude);

            if (loc!= null && loc != "Unknown Location")
            {
                HasLocation = true;
                CurrentLocation = loc;
            }
            else
            {
                HasLocation = false;
            }
        }

        public bool RefreshDuration()
        {
            if (Host.Backgrounded) return true;
            var currentWaypoints = Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step);

            if (!currentWaypoints.Any()) return false;

            var currentWaypoint = currentWaypoints.OrderBy(w => w.Step).First();
            CurrentDuration = DateTimeOffset.UtcNow - currentWaypoint.Timestamp;

            return true;
        }

        public ICommand StopCommand { get; set; }

        public event EventHandler StopRequested = delegate { };

        public async void Stop()
        {
            if (Busy) return;
            var currentWaypoints = Host.TrackerService.CurrentWaypoints;
            if (currentWaypoints.Count() < 2)
            {
                await Host.TrackerService.Cancel();
                StopRequested?.Invoke(this, EventArgs.Empty);
                return;
            }
            StopRequested?.Invoke(this, EventArgs.Empty);
            TrackerService.IsTracking = false;
            Busy = false;
        }

        public async void EndJourneyConfirmed()
        {
            Busy = true;
            TrackerService.IsTracking = false;
            await Host.TrackerService.Stop();
        }
    }
}