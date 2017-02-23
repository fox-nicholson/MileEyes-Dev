using System;
using System.Linq;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    internal class RegisterViewModel : ViewModel
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

        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value) return;
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _confirmPassword;

        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                if (_confirmPassword == value) return;
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
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

        public ICommand RegisterCommand { get; set; }

        public RegisterViewModel()
        {
            //FirstName = "Test";
            //LastName = "Owner";
            //Email = "test@123.com";

            //Password = "Ab.123";
            //ConfirmPassword = "Ab.123";

            Address = new Address()
            {
                Label = "Required"
            };

            RegisterCommand = new Command(Register);
        }

        public event EventHandler<string> RegisterFailed = delegate { };
        public event EventHandler RegisterSuccess = delegate { };

        public async void Register()
        {
            Busy = true;

            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            {
                RegisterFailed?.Invoke(this, "First and Last Name are required.");

                Busy = false;
                return;
            }

            if (string.IsNullOrEmpty(Email))
            {
                RegisterFailed?.Invoke(this, "Email is required.");

                Busy = false;
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                RegisterFailed?.Invoke(this, "Password is required.");

                Busy = false;
                return;
            }

            if (ConfirmPassword != Password)
            {
                RegisterFailed?.Invoke(this, "Password and Confirm Password must be the same.");

                Busy = false;
                return;
            }

            if (string.IsNullOrEmpty(Address.PlaceId))
            {
                RegisterFailed?.Invoke(this, "Address is required.");

                Busy = false;
                return;
            }

            var result =
                await
                    Services.Host.AuthService.Register(FirstName, LastName, Email, Password, ConfirmPassword,
                        Address.PlaceId);

            if (result.Error)
            {
                if (result.ModelState == null)
                {
                    RegisterFailed?.Invoke(this,
                        "There was a connection problem, please check that you have an active internet connection on your device.");

                    Busy = false;
                }
                else
                {
                    var emailTaken = result.ModelState._?.FirstOrDefault();

                    var errorMessage = result.ModelState.FirstName?.Aggregate("",
                        (current, s) => current + s + Environment.NewLine);

                    errorMessage = result.ModelState.LastName?.Aggregate(errorMessage,
                        (current, s) => current + s + Environment.NewLine);

                    errorMessage = result.ModelState.Email?.Aggregate(errorMessage,
                        (current, s) => current + s + Environment.NewLine);

                    errorMessage = result.ModelState.Password?.Aggregate(errorMessage,
                        (current, s) => current + s + Environment.NewLine);

                    if (!string.IsNullOrEmpty(emailTaken))
                    {
                        errorMessage = result.ModelState.PlaceId?.Aggregate(errorMessage,
                            (current, s) => current + "Address is required." + Environment.NewLine);
                        errorMessage += emailTaken + Environment.NewLine;
                    }
                    else
                    {
                        errorMessage = result.ModelState.PlaceId?.Aggregate(errorMessage,
                            (current, s) => current + "Address is required.");
                    }

                    errorMessage = result.ModelState.Password?.Aggregate(errorMessage,
                        (current, s) => current + s + Environment.NewLine);

                    errorMessage = result.ModelState._?.Aggregate(errorMessage,
                        (current, s) => current + s + Environment.NewLine);

                    RegisterFailed?.Invoke(this, errorMessage?.Trim());

                    Busy = false;
                }
            }
            else
            {
                await Services.Host.AuthService.Authenticate(Email, Password);

                RegisterSuccess?.Invoke(this, EventArgs.Empty);

                Busy = false;
            }
        }
    }
}