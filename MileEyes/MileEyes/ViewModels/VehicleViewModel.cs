﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    class VehicleViewModel : ViewModel
    {
        private Vehicle _vehicle;

        public Vehicle Vehicle => _vehicle;

        private string _id;

        public string Id => _id;

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

            _vehicle = new Vehicle()
            {
                EngineType = new EngineType()
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

            _vehicle = await Services.Host.VehicleService.AddVehicle(_vehicle);

            if (string.IsNullOrEmpty(_vehicle.Id))
            {
                VehicleNotSaved?.Invoke(this, Registration + " was not saved.");
                Busy = false;
                return;
            }

            VehicleSaved?.Invoke(this, Registration + " was saved.");
            Busy = false;
        }
    }
}
