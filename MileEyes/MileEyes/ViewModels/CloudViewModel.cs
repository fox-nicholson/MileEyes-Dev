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
    class CloudViewModel : ViewModel
    {
        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName == value) return;
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName == value) return;
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email == value) return;
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private Address _address;
        public Address Address
        {
            get { return _address; }
            set
            {
                if (_address == value) return;
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public CloudViewModel()
        {
            SaveCommand = new Command(Save);
            LogoutCommand = new Command(Logout);
            Refresh();
        }

        public override async void Refresh()
        {
            base.Refresh();

            var userdetails = await Services.Host.UserService.GetDetails();

            if (userdetails.Error) return;

            Email = userdetails.Result.Email;
            FirstName = userdetails.Result.FirstName;
            LastName = userdetails.Result.LastName;
            Address = await Services.Host.GeocodingService.GetAddress(userdetails.Result.PlaceId);
        }

        public event EventHandler<string> SaveFailed = delegate { };
        public event EventHandler SaveComplete = delegate { };

        public async void Save()
        {
            if (Busy) return;

            Busy = true;

            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(FirstName))
            {
                SaveFailed?.Invoke(this, "First and Last Name are required.");
                Busy = false;
                return;
            }

            if (string.IsNullOrEmpty(Email))
            {
                SaveFailed?.Invoke(this, "Email is required.");
                Busy = false;
                return;
            }

            if (string.IsNullOrEmpty(Address.PlaceId))
            {
                SaveFailed?.Invoke(this, "Address is required.");
                Busy = false;
                return;
            }

            var result = await Services.Host.UserService.UpdateDetails(FirstName, LastName, Email, Address.PlaceId);

            if (result.Error)
            {
                SaveFailed?.Invoke(this, result.Message);
                Busy = false;
            }

            SaveComplete?.Invoke(this, EventArgs.Empty);
            Busy = false;
        }

        public event EventHandler LoggedOut = delegate { };

        public void Logout()
        {
            if (Busy) return;

            Busy = true;

            Services.Host.AuthService.Logout();
            Busy = false;

            LoggedOut?.Invoke(this, EventArgs.Empty);
            Busy = false;
        }
    }
}
