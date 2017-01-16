using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    class JourneyManualViewModel : ViewModel
    {
        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date == value) return;
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private Address _originAddress;

        public Address OriginAddress
        {
            get { return _originAddress; }
            set
            {
                if (_originAddress == value) return;
                _originAddress = value;
                OnPropertyChanged(nameof(OriginAddress));
            }
        }

        private Address _destinationAddress;

        public Address DestinationAddress
        {
            get { return _destinationAddress; }
            set
            {
                if (_destinationAddress == value) return;
                _destinationAddress = value;
                OnPropertyChanged(nameof(DestinationAddress));
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

        public JourneyManualViewModel()
        {
            Date = DateTime.UtcNow;
            
            OriginAddress = new Address()
            {
                Label = "Required"
            };
            DestinationAddress = new Address()
            {
                Label = "Required"
            };

            InitDefaults();

            SaveCommand = new Command(Save);
            CancelCommand = new Command(Cancel);
        }

        private async void InitDefaults()
        {
            var defaultVehicles = (await Services.Host.VehicleService.GetVehicles()).Where(v => v.Default);
            var defaultPassengers = Helpers.Settings.DefaultPassengers;
            var defaultReasons = (await Services.Host.ReasonService.GetReasons()).Where(r => r.Default);
            var defaultCompanies = (await Services.Host.CompanyService.GetCompanies()).Where(c => c.Default);

            if (defaultVehicles.Any())
            {
                Vehicle = defaultVehicles.FirstOrDefault();
            }
            else
            {
                Vehicle = new Vehicle()
                {
                    Registration = "Required"
                };
            }

            if (defaultReasons.Any())
            {
                Reason = defaultReasons.FirstOrDefault().Text;
            }
            else
            {
                Reason = "Required";
            }

            #region Set Passengers
            switch (defaultPassengers)
            {
                case 0:
                    Passenger = new Passenger()
                    {
                        Name = "Just Me",
                        Number = 0
                    };
                    break;
                case 1:
                    new Passenger()
                    {
                        Name = "One",
                        Number = 1
                    };
                    break;
                case 2:
                    Passenger = new Passenger()
                    {
                        Name = "Two",
                        Number = 2
                    };
                    break;
                case 3:
                    Passenger = new Passenger()
                    {
                        Name = "Three",
                        Number = 3
                    };
                    break;
                case 4:
                    Passenger = new Passenger()
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
                    Company = new Company()
                    {
                        Name = "Required"
                    };
                }
            }
            else
            {
                Company = new Company()
                {
                    Name = "N/A"
                };
            }
        }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public event EventHandler<string> SaveFailed = delegate { };
        public event EventHandler Saved = delegate { };

        public async void Save()
        {
            if (string.IsNullOrEmpty(OriginAddress.PlaceId))
            {
                SaveFailed?.Invoke(this, "Origin is required.");
                return;
            }

            if (string.IsNullOrEmpty(DestinationAddress.PlaceId))
            {
                SaveFailed?.Invoke(this, "Destination is required.");
                return;
            }

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

            var originWaypoint = new Waypoint()
            {
                PlaceId = OriginAddress.PlaceId,
                Latitude = OriginAddress.Latitude,
                Longitude = OriginAddress.Longitude,
                Label = OriginAddress.Label,
                Step = 0
            };
            var destinationWaypoint = new Waypoint()
            {
                PlaceId = DestinationAddress.PlaceId,
                Latitude = DestinationAddress.Latitude,
                Longitude = DestinationAddress.Longitude,
                Label = DestinationAddress.Label,
                Step = 1
            };

            var journey = new Journey()
            {
                Company = Company,
                Date = Date,
                Invoiced = Invoiced,
                Passengers = Passengers,
                Reason = Reason,
                Vehicle = Vehicle
            };

            journey.Waypoints.Add(originWaypoint);
            journey.Waypoints.Add(destinationWaypoint);

            var result = await Services.Host.JourneyService.SaveJourney(journey);

            Saved?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Cancelled = delegate { };

        public async void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}
