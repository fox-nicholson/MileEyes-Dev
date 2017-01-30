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

        private Reason _defaultReason;
        public Reason DefaultReason
        {
            get { return _defaultReason; }
            set
            {
                if (_defaultReason == value) return;
                _defaultReason = value;
                OnPropertyChanged(nameof(DefaultReason));
            }
        }

        private Company _defaultCompany;

        public Company DefaultCompany
        {
            get { return _defaultCompany; }
            set
            {
                if (_defaultCompany == value) return;
                _defaultCompany = value;
                OnPropertyChanged(nameof(DefaultCompany));
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

        private string _vehiclesCountString;

        public string VehiclesCountString
        {
            get { return _vehiclesCountString; }
            set
            {
                if (_vehiclesCountString == value) return;
                _vehiclesCountString = value;
                OnPropertyChanged(nameof(VehiclesCountString));
            }
        }

        private string _reasonsCountString;

        public string ReasonsCountString
        {
            get { return _reasonsCountString; }
            set
            {
                if (_reasonsCountString == value) return;
                _reasonsCountString = value;
                OnPropertyChanged(nameof(ReasonsCountString));
            }
        }

        private string _companyCountString;

        public string CompanyCountString
        {
            get { return _companyCountString; }
            set
            {
                if(_companyCountString == value) return;
                _companyCountString = value;
                OnPropertyChanged(nameof(CompanyCountString));
            }
        }
        
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

                var temp = userDetails;
                if (!string.IsNullOrEmpty(userDetails?.Result.Email))
                {
                    MileEyesConnectDetail = userDetails.Result.Email;
                }
            }
            else
            {
                MileEyesConnectDetail = "Connect";
            }

            InvoicedDefault = Helpers.Settings.InvoicedDefault;

            var vcount = (await Host.VehicleService.GetVehicles()).Count();
            switch (vcount)
            {
                case 0:
                    VehiclesCountString = "No vehicles saved";
                    break;
                case 1:
                    VehiclesCountString = vcount + " vehicle saved";
                    break;
                default:
                    VehiclesCountString = vcount + " vehicles saved";
                    break;
            }

            var rcount = (await Host.ReasonService.GetReasons()).Count();
            switch (rcount)
            {
                case 0:
                    ReasonsCountString = "No reasons saved";
                    break;
                case 1:
                    ReasonsCountString = vcount + " reason saved";
                    break;
                default:
                    ReasonsCountString = vcount + " reasons saved";
                    break;
            }

            var ccount = (await Host.CompanyService.GetCompanies()).Count();
            switch (ccount)
            {
                case 0:
                    CompanyCountString = "No companies available";
                    break;
                case 1:
                    CompanyCountString = vcount + " company available";
                    break;
                default:
                    CompanyCountString = vcount + " companies available";
                    break;
            }

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

            var defaultReason =
                (await Services.Host.ReasonService.GetReasons()).FirstOrDefault(v => v.Default == true);

            DefaultReason = defaultReason;

            if (Authenticated)
            {
                var defaultCompany =
                        (await Services.Host.CompanyService.GetCompanies()).FirstOrDefault(v => v.Default == true);

                DefaultCompany = defaultCompany; 
            }
        }
    }
}

