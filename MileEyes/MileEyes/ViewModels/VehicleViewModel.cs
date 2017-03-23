using System;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    internal class VehicleViewModel : ViewModel
    {
        private Vehicle _vehicle;

        public Vehicle Vehicle => _vehicle;

        private string _id;

        public string Id => _id;

        private DateTime _regDate;

        public DateTime RegDate
        {
            get { return _regDate; }
            set
            {
                if (_regDate == value) return;
                _regDate = value;
                OnPropertyChanged(nameof(RegDate));
            }
        }

        private DateTime _maxDate;

        public DateTime MaxDate
        {
            get { return _maxDate; }
            set
            {
                if (_maxDate == value) return;
                _maxDate = value;
                OnPropertyChanged(nameof(MaxDate));
            }
        }

        private DateTime _minDate;

        public DateTime MinDate
        {
            get { return _minDate; }
            set
            {
                if (_minDate == value) return;
                _minDate = value;
                OnPropertyChanged(nameof(MinDate));
            }
        }

        private string _registration;

        public string Registration
        {
            get { return _registration; }
            set
            {
                if (_registration == value) return;
                _registration = value;
                OnPropertyChanged(nameof(Registration));
            }
        }

        private EngineType _engineType;

        public EngineType EngineType
        {
            get { return _engineType; }
            set
            {
                _engineType = value;
                OnPropertyChanged(nameof(EngineType));
            }
        }

        private VehicleType _vehicleType;

        public VehicleType VehicleType
        {
            get { return _vehicleType; }
            set
            {
                _vehicleType = value;
                OnPropertyChanged(nameof(VehicleType));
            }
        }

        public VehicleViewModel()
        {
            Init();
            Reset();
            Refresh();
        }

        public VehicleViewModel(string id)
        {
            Init();
            Load(id);
        }

        public VehicleViewModel(Vehicle v)
        {
            Init();
            _vehicle = v;
            Refresh();
        }

        private void Init()
        {
            SaveCommand = new Command(Save);
        }

        public void Reset()
        {
            Busy = true;

            RegDate = DateTime.UtcNow;
            MaxDate = DateTime.UtcNow;
            MinDate = new DateTime(1970, 1, 1);

            _vehicle = new Vehicle
            {
                EngineType = new EngineType
                {
                    Name = "Required"
                },

                VehicleType = new VehicleType
                {
                    Name = "Required"
                }
            };

            Busy = false;
        }

        public async void Load(string id)
        {
            _vehicle = await Services.Host.VehicleService.GetVehicle(id);

            Refresh();
        }

        public void Refresh()
        {
            Refreshing = true;

            _id = _vehicle.Id;
            Registration = _vehicle.Registration;
            EngineType = _vehicle.EngineType;
            VehicleType = _vehicle.VehicleType;

            Refreshing = false;
        }

        public event EventHandler<string> VehicleNotSaved = delegate { };
        public event EventHandler<string> VehicleSaved = delegate { };

        public ICommand SaveCommand { get; set; }

        public async void Save()
        {
            if (Busy) return;

            Busy = true;

            if (!string.IsNullOrEmpty(Id))
            {
                VehicleNotSaved?.Invoke(this, Registration + " already exists.");
                Busy = false;
                return;
            }

            if (string.IsNullOrEmpty(Registration))
            {
                VehicleNotSaved?.Invoke(this, "Registration is required.");
                Busy = false;
                return;
            }

            if (string.IsNullOrEmpty(EngineType.Id))
            {
                VehicleNotSaved?.Invoke(this, "Engine Type is required.");
                Busy = false;
                return;
            }

            _vehicle.Registration = Registration;
            _vehicle.EngineType = EngineType;
            _vehicle.VehicleType = VehicleType;
            _vehicle.RegDate = RegDate;

            _vehicle = await Services.Host.VehicleService.AddVehicle(_vehicle);

            if (string.IsNullOrEmpty(_vehicle.Id))
            {
                VehicleNotSaved?.Invoke(this, Registration + " was not saved.");
                Busy = false;
                return;
            }

            VehicleSaved?.Invoke(this, Registration + " was saved.");

            if (Authenticated)
            Services.Host.VehicleService.Sync();
            Busy = false;
        }
    }
}