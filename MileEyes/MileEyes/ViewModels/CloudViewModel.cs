﻿using System;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    internal class CloudViewModel : ViewModel
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
            
            SaveComplete?.Invoke(this, EventArgs.Empty);
            Busy = false;
        }

        public event EventHandler LoggedOut = delegate { };

        public void Logout()
        {
            if (Busy) return;

            Busy = true;

            Services.Host.AuthService.Logout();

//            var model = new SettingsViewModel {InvoicedDefault = false};

            LoggedOut?.Invoke(this, EventArgs.Empty);
            Busy = false;
        }
    }
}