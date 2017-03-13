using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MileEyes.Services.Extensions;
using MileEyes.Services.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.ViewModels
{
    internal class JourneyConfirmViewModel : ViewModel
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

            Route =
                new ObservableCollection<Position>(Services.Host.TrackerService.CurrentWaypoints.OrderBy(w => w.Step)
                    .Select(w => new Position(w.Latitude, w.Longitude)));

            InitDefaults();

            SaveCommand = new Command(Save);
        }

        private async void InitDefaults()
        {
            var defaultVehicles = (await Services.Host.VehicleService.GetVehicles()).Where(v => v.Default);
            var defaultPassengers = Helpers.Settings.DefaultPassengers;
            var defaultReasons = (await Services.Host.ReasonService.GetReasons()).Where(r => r.Default);
            var defaultCompanies = (await Services.Host.CompanyService.GetCompanies()).Where(c => c.Default);
            var defaultInvoiced = Helpers.Settings.InvoicedDefault;

            Invoiced = defaultInvoiced;

            if (defaultVehicles.Any())
            {
                Vehicle = defaultVehicles.FirstOrDefault();
            }
            else
            {
                Vehicle = new Vehicle
                {
                    Registration = "Required"
                };
            }

            Reason = defaultReasons.Any() ? defaultReasons.FirstOrDefault().Text : "Required";

            #region Set Passengers

            switch (defaultPassengers)
            {
                case 0:
                    Passenger = new Passenger
                    {
                        Name = "Just Me",
                        Number = 0
                    };
                    break;
                case 1:
                    Passenger = new Passenger
                    {
                        Name = "One",
                        Number = 1
                    };
                    break;
                case 2:
                    Passenger = new Passenger
                    {
                        Name = "Two",
                        Number = 2
                    };
                    break;
                case 3:
                    Passenger = new Passenger
                    {
                        Name = "Three",
                        Number = 3
                    };
                    break;
                case 4:
                    Passenger = new Passenger
                    {
                        Name = "Four",
                        Number = 4
                    };
                    break;
            }

            #endregion

            if (Authenticated)
            {
                if (defaultCompanies.Any())
                {
                    Company = defaultCompanies.FirstOrDefault();
                }
                else
                {
                    Company = new Company
                    {
                        Name = "Required"
                    };
                }
            }
            else
            {
                Company = new Company
                {
                    Name = "N/A"
                };
            }
        }

        public ICommand SaveCommand { get; set; }

        public event EventHandler<string> SaveFailed = delegate { };

        public event EventHandler Saved = delegate { };

        public async void Save()
        {
            if (Busy) return;

            Busy = true;

            if (string.IsNullOrEmpty(Vehicle.Id))
            {
                SaveFailed?.Invoke(this, "Vehicle is required");
                Busy = false;
                return;
            }

            if (Reason == "Required")
            {
                SaveFailed?.Invoke(this, "Reason is required.");
                Busy = false;
                return;
            }

            if (Authenticated)
            {
                if (string.IsNullOrEmpty(Company.Id))
                {
                    SaveFailed?.Invoke(this, "Company is required.");
                    Busy = false;
                    return;
                }
            }

            var journey = new Journey
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

            journey.Distance = await journey.CalculateDistance();

            await Services.Host.JourneyService.SaveJourney(journey);

            Saved?.Invoke(this, EventArgs.Empty);

            Services.Host.JourneyService.Sync();

            Busy = false;
        }
    }
}