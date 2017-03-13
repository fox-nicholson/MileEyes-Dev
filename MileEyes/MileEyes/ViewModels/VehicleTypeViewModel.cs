using MileEyes.Services.Models;
using System;

namespace MileEyes.ViewModels
{
    internal class VehicleTypeViewModel : ViewModel
    {
        private VehicleType _vehicleType;

        public VehicleType VehicleType
        {
            get { return _vehicleType; }
            set
            {
                if (_vehicleType == value) return;
                _vehicleType = value;
                OnPropertyChanged(nameof(VehicleType));
            }
        }

        private string _id;

        public string Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public VehicleTypeViewModel(VehicleType et)
        {
            VehicleType = et;

            Id = _vehicleType.Id;
            Name = _vehicleType.Name;
        }
    }
}