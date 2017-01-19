using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services;
using MileEyes.Services.Models;

namespace MileEyes.ViewModels
{
    class SettingsViewModel : ViewModel
    {
        private bool _invoicedDefault;

        public bool InvoicedDefault
        {
            get { return _invoicedDefault; }
            set
            {
                if (_invoicedDefault == value) return;
                _invoicedDefault = value;

                Helpers.Settings.InvoicedDefault = value;

                OnPropertyChanged(nameof(InvoicedDefault));
            }
        }

        private int _vehicleCount;

        public int VehicleCount
        {
            get { return _vehicleCount; }
            set
            {
                if (_vehicleCount == value) return;
                _vehicleCount = value;
                OnPropertyChanged(nameof(VehicleCount));
                OnPropertyChanged(nameof(MultipleVehicles));
                OnPropertyChanged(nameof(SingleVehicle));
            }
        }

        private int _reasonCount;

        public int ReasonCount
        {
            get { return _reasonCount; }
            set
            {
                if (_reasonCount == value) return;
                _reasonCount = value;
                OnPropertyChanged(nameof(ReasonCount));
                OnPropertyChanged(nameof(MultipleReasons));
                OnPropertyChanged(nameof(SingleReason));
            }
        }

        private Vehicle _defaultVehicle;

        public Vehicle DefaultVehicle
        {
            get { return _defaultVehicle; }
            set
            {
                if (_defaultVehicle == value) return;
                _defaultVehicle = value;
                OnPropertyChanged(nameof(DefaultVehicle));
            }
        }

        private Passenger _defaultPassenger;
        public Passenger DefaultPassenger
        {
            get { return _defaultPassenger; }
            set
            {
                if (_defaultPassenger == value) return;
                _defaultPassenger = value;
                OnPropertyChanged(nameof(DefaultPassenger));
            }
        }

        private string _mileEyesConnectDetail;

        public string MileEyesConnectDetail
        {
            get { return _mileEyesConnectDetail; }
            set
            {
                if (_mileEyesConnectDetail == value) return;
                _mileEyesConnectDetail = value;
                OnPropertyChanged(nameof(MileEyesConnectDetail));
            }
        }


        public bool MultipleVehicles => VehicleCount > 1;
        public bool MultipleReasons => ReasonCount > 1;
        public bool SingleReason => ReasonCount == 1;
        public bool SingleVehicle => VehicleCount == 1;

        public SettingsViewModel()
        {
            Refresh();
        }

        public override async void Refresh()
        {
            base.Refresh();

            if (Authenticated)
            {
                var userDetails = await Host.UserService.GetDetails();
                MileEyesConnectDetail = userDetails.Result.Email;
            }
            else
            {
                MileEyesConnectDetail = "Connect";
            }

            InvoicedDefault = Helpers.Settings.InvoicedDefault;
            VehicleCount = (await Services.Host.VehicleService.GetVehicles()).Count();
            ReasonCount = (await Services.Host.ReasonService.GetReasons()).Count();

            if (Helpers.Settings.DefaultPassengers > 0)
            {
                DefaultPassenger =
                    (new PassengersSelectionViewModel()).Passengers.FirstOrDefault(
                        p => p.Number == Helpers.Settings.DefaultPassengers);
            }
            else
            {
                DefaultPassenger = new Passenger()
                {
                    Name = "Just Me",
                    Number = 0
                };
            }

            var defaultVehicle =
                (await Services.Host.VehicleService.GetVehicles()).FirstOrDefault(v => v.Default == true);

            DefaultVehicle = defaultVehicle;
        }
    }
}
