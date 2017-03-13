using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    internal class VehicleTypesViewModel : ViewModel
    {
        public ObservableCollection<VehicleTypeViewModel> VehicleTypes { get; set; } =
            new ObservableCollection<VehicleTypeViewModel>();

        private VehicleTypeViewModel _selectedVehicleType;

        public VehicleTypeViewModel SelectedVehicleType
        {
            get { return _selectedVehicleType; }
            set
            {
                if (_selectedVehicleType == value) return;

                _selectedVehicleType = value;

                OnPropertyChanged(nameof(SelectedVehicleType));
            }
        }

        public VehicleTypesViewModel()
        {
            SelectCommand = new Command(Select);
            RefreshCommand = new Command(Refresh);
            Refresh();
        }

        public ICommand RefreshCommand { get; set; }

        public override async void Refresh()
        {
            base.Refresh();

            if (Refreshing) return;
            if (Busy) return;

            Refreshing = true;
            Busy = true;

            VehicleTypes.Clear();

            var ets = await Services.Host.VehicleTypeService.GetVehicleTypes();

            var vehicleTypes = ets as VehicleType[] ?? ets.ToArray();

            if (vehicleTypes != null && vehicleTypes.Any())
            {
                foreach (var et in vehicleTypes)
                {
                    VehicleTypes.Add(new VehicleTypeViewModel(et));
                }
            }
            Device.StartTimer(TimeSpan.FromSeconds(2), Wait);
            Refreshing = false;
        }

        public ICommand SelectCommand { get; set; }

        public event EventHandler Selected = delegate { };

        public event EventHandler NotSelected = delegate { };

        public void Select()
        {
            if (SelectedVehicleType != null)
            {
                Selected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                NotSelected?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool Wait()
        {
            Busy = false;
            return false;
        }
    }
}