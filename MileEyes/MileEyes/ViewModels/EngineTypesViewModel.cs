using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    internal class EngineTypesViewModel : ViewModel
    {
        public ObservableCollection<EngineTypeViewModel> EngineTypes { get; set; } =
            new ObservableCollection<EngineTypeViewModel>();

        private EngineTypeViewModel _selectedEngineType;

        public EngineTypeViewModel SelectedEngineType
        {
            get { return _selectedEngineType; }
            set
            {
                if (_selectedEngineType == value) return;

                _selectedEngineType = value;

                OnPropertyChanged(nameof(SelectedEngineType));
            }
        }

        public EngineTypesViewModel()
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

            EngineTypes.Clear();

            var ets = await Services.Host.EngineTypeService.GetEngineTypes();

            var engineTypes = ets as EngineType[] ?? ets.ToArray();

            if (engineTypes != null && engineTypes.Any())
            {
                foreach (var et in engineTypes)
                {
                    EngineTypes.Add(new EngineTypeViewModel(et));
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
            if (SelectedEngineType != null)
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