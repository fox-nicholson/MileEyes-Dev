using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Annotations;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    class TrackViewModel : ViewModel
    {
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

        private bool _gpsAvailable;

        public bool GPSAvailable
        {
            get { return _gpsAvailable; }
            set
            {
                if (_gpsAvailable == value) return;
                _gpsAvailable = value;
                OnPropertyChanged(nameof(GPSAvailable));
                OnPropertyChanged(nameof(GPSUnavailable));
            }
        }

        public bool GPSUnavailable => !_gpsAvailable;
        
        public TrackViewModel()
        {
            StartJourneyCommand = new Command(StartJourney);

            CurrentLocation = "Searching...";

            Device.StartTimer(TimeSpan.FromMinutes(5), GpsPingCallback);

            Refresh();

            GpsPingCallback();
        }

        public void Refresh()
        {
            GPSAvailable = Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationAvailable && Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled;
        }

        public bool GpsPingCallback()
        {
            if (!Services.Host.Backgrounded)
            {
                UpdateLocation();
            }

            return true;
        }

        private async void UpdateLocation()
        {
            if (GPSAvailable)
            {
                var real = Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationAvailable;
                var real2 = Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled;

                var position = await Plugin.Geolocator.CrossGeolocator.Current.GetPositionAsync(5000);

                var loc = await Services.Host.GeocodingService.GetWeather(position.Latitude, position.Longitude);

                CurrentLocation = loc.name != "Unknown Location" ? loc.name : "Searching...";
            }
            else
            {
                CurrentLocation = "GPS Unavailable";
            }
        }

        public ICommand StartJourneyCommand { get; set; }

        public void StartJourney()
        {
            Services.Host.TrackerService.Start();
        }
    }
}
