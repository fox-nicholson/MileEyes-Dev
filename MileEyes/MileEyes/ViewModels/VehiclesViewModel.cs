using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    class VehiclesViewModel : ViewModel
    {
        public ObservableCollection<VehicleViewModel> Vehicles { get; set; } = new ObservableCollection<VehicleViewModel>();
        
        private VehicleViewModel _selectedVehicle;

        public VehicleViewModel SelectedVehicle
        {
            get { return _selectedVehicle; }
            set
            {
                _selectedVehicle = value;

                OnPropertyChanged(nameof(SelectedVehicle));
            }
        }

        public VehiclesViewModel()
        {
            RefreshCommand = new Command(Refresh);

            RemoveCommand = new Command<VehicleViewModel>(Remove);
            SelectCommand = new Command(Select);

            Refresh();
        }

        public event EventHandler NoVehicles = delegate { };

        public ICommand RefreshCommand { get; set; }

        public async void Refresh()
        {
            base.Refresh();

            Refreshing = true;

            Vehicles.Clear();

            var vehicles = await Services.Host.VehicleService.GetVehicles();

            if (vehicles.Any())
            {
                foreach (var v in vehicles)
                {
                    var vehicleViewModel = new VehicleViewModel(v);
                    Vehicles.Add(vehicleViewModel);
                }
            }
            else
            {
                NoVehicles?.Invoke(this, EventArgs.Empty);
            }

            Refreshing = false;
        }

        public event EventHandler<string> VehicleRemoved = delegate { };
        public event EventHandler<string> VehicleNotRemoved = delegate { };

        public ICommand RemoveCommand { get; set; }

        public async void Remove(VehicleViewModel v)
        {
            Busy = true;

            var vehicle = await Services.Host.VehicleService.RemoveVehicle(v.Id);

            if (vehicle == null)
            {
                VehicleNotRemoved?.Invoke(this, v.Registration + " was not removed." + Environment.NewLine + "This is most likely because the vehicle was used for a journey.");
            }
            else
            {
                VehicleRemoved?.Invoke(this, vehicle.Registration + " was removed.");
            }

            Busy = false;

            Refresh();
        }

        public ICommand SetDefaultCommand { get; set; }

        public async void SetDefault(VehicleViewModel v)
        {
            Busy = true;

            await Services.Host.VehicleService.SetDefault(v.Id);

            Refresh();

            Busy = false;
        }
        
        public ICommand SelectCommand { get; set; }

        public event EventHandler<VehicleViewModel> Selected = delegate { };

        public event EventHandler NotSelected = delegate { };

        public void Select()
        {
            if (SelectedVehicle != null)
            {
                Selected?.Invoke(this, SelectedVehicle);
            }
            else
            {
                NotSelected?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
