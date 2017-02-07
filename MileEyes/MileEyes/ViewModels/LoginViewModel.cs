using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    class LoginViewModel : ViewModel
    {
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

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            //Email = "test@123.com";
            //Password = "Ab.123";

            LoginCommand = new Command(Login);
        }

        public event EventHandler<string> LoginFailed = delegate { };
        public event EventHandler LoginSuccess = delegate { };

        public async void Login()
        {
            Busy = true;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                LoginFailed?.Invoke(this, "Email and Password are required.");

                Busy = false;

                return;
            }

            var result = await Services.Host.AuthService.Authenticate(Email, Password);

            if (result.Success)
            {
                LoginSuccess?.Invoke(this, EventArgs.Empty);

                Busy = false;

                return;
            }

            LoginFailed?.Invoke(this, result.error_description);

            Busy = false;
        }
    }
}
