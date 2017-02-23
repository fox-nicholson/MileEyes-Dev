using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MileEyes.CustomControls;
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
            Services.Host.TrackerService.HasMoved += TrackerService_HasMoved;
            StopCommand = new Command(Stop);
            Refresh();

            Device.StartTimer(TimeSpan.FromSeconds(1), RefreshDuration);

            Device.StartTimer(TimeSpan.FromMinutes(1), UpdateLocalityCallback);
            UpdateLocalityCallback();
        }

        private void TrackerService_HasMoved(object sender, EventArgs e)
        {
            if (Services.Host.Backgrounded) return;

            Refresh();
        }

        public override void Refresh()
        {
            base.Refresh();

            Refreshing = true;

            Route.Clear();

            foreach (var waypoint in Services.Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step))
            {
                Route.Add(new Position(waypoint.Latitude, waypoint.Longitude));
            }

            CurrentDistance = Units.MetersToMiles(Services.Host.TrackerService.CurrentDistance);

            Refreshing = false;
        }

        public bool UpdateLocalityCallback()
        {
            if (!Services.Host.Backgrounded)
            {
                UpdateLocation();
            }

            return true;
        }

        public async void UpdateLocation()
        {
            var loc =
                await Services.Host.GeocodingService.GetWeather(Services.Host.TrackerService.CurrentLocation.Latitude,
                    Services.Host.TrackerService.CurrentLocation.Longitude);

            if (loc.name != "Unknown Location")
            {
                HasLocation = true;
                CurrentLocation = loc.name;
            }
            else
            {
                HasLocation = false;
            }
        }

        public bool RefreshDuration()
        {
            if (Services.Host.Backgrounded) return true;
            var currentWaypoints = Services.Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step);

            if (currentWaypoints.Any())
            {
                var currentWaypoint = currentWaypoints.OrderBy(w => w.Step).First();
                CurrentDuration = DateTimeOffset.UtcNow - currentWaypoint.Timestamp;

                return true;
            }
            return false;
        }

        public ICommand StopCommand { get; set; }

        public event EventHandler StopRequested = delegate { };

        public async void Stop()
        {
            if (Busy) return;
            var currentWaypoints = Services.Host.TrackerService.CurrentWaypoints;
            if (currentWaypoints.Count() < 2)
            {
                await Services.Host.TrackerService.Cancel();
                return;
            }
            StopRequested?.Invoke(this, EventArgs.Empty);
            Busy = false;
        }

        public async void EndJourneyConfirmed()
        {
            Busy = true;
            await Services.Host.TrackerService.Stop();
        }
    }
}