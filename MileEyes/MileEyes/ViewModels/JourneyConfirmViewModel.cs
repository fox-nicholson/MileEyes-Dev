using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.ViewModels
{
    class JourneyConfirmViewModel : ViewModel
    {
        private DateTimeOffset _date;

        public DateTimeOffset Date
        {
            get { return _date; }
            set
            {
                if (_date == value) return;
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private Vehicle _vehicle;

        public Vehicle Vehicle
        {
            get { return _vehicle; }
            set
            {
                if (_vehicle == value) return;
                _vehicle = value;
                OnPropertyChanged(nameof(Vehicle));
            }
        }

        private Company _company;

        public Company Company
        {
            get { return _company; }
            set
            {
                if (_company == value) return;
                _company = value;
                OnPropertyChanged(nameof(Company));
            }
        }

        private bool _invoiced;

        public bool Invoiced
        {
            get { return _invoiced; }
            set
            {
                if (_invoiced == value) return;
                _invoiced = value;
                OnPropertyChanged(nameof(Invoiced));
            }
        }

        private string _reason;

        public string Reason
        {
            get { return _reason; }
            set
            {
                if (_reason == value) return;
                _reason = value;
                OnPropertyChanged(nameof(Reason));
            }
        }

        private int _passengers;

        public int Passengers
        {
            get { return _passengers; }
            set
            {
                if (_passengers == value) return;
                _passengers = value;
                OnPropertyChanged(nameof(Passengers));
            }
        }

        private Passenger _passenger;

        public Passenger Passenger
        {
            get { return _passenger; }
            set
            {
                if (_passenger?.Number == value.Number) return;
                _passenger = value;
                OnPropertyChanged(nameof(Passenger));
            }
        }

        private bool _sync;

        public bool Sync
        {
            get { return _sync; }
            set
            {
                if (_sync == value) return;
                _sync = value;
                OnPropertyChanged(nameof(Sync));
            }
        }

        public ObservableCollection<Position> Route { get; set; }

        public JourneyConfirmViewModel()
        {
            Date = Services.Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step).FirstOrDefault().Timestamp;

            Route = new ObservableCollection<Position>(Services.Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step)
                    .Select(w => new Position(w.Latitude, w.Longitude)));

            Passenger = new Passenger()
            {
                Name = "Required"
            };

            Vehicle = new Vehicle()
            {
                Registration = "Required"
            };

            Company = new Company()
            {
                Name = "Required"
            };

            SaveCommand = new Command(Save);
        }

        public ICommand SaveCommand { get; set; }

        public event EventHandler<string> SaveFailed = delegate { };

        public event EventHandler Saved = delegate { };

        public async void Save()
        {
            if (string.IsNullOrEmpty(Vehicle.Id))
            {
                SaveFailed?.Invoke(this, "Vehicle is required");
                return;
            }

            if (Reason == "Required")
            {
                SaveFailed?.Invoke(this, "Reason is required.");
                return;
            }

            if (Authenticated)
            {
                if (string.IsNullOrEmpty(Company.Id))
                {
                    SaveFailed?.Invoke(this, "Company is required.");
                    return;
                }
            }

            var journey = new Journey()
            {
                Company = Company,
                Date = Date,
                Invoiced = Invoiced,
                Passengers = Passengers,
                Reason = Reason,
                Vehicle = Vehicle
            };

            foreach (var w in Services.Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step))
            {
                journey.Waypoints.Add(w);
            }

            var result = await Services.Host.JourneyService.SaveJourney(journey);

            Saved?.Invoke(this, EventArgs.Empty);
        }
    }
}
