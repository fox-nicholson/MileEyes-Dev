using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace MileEyes.ViewModels
{
    internal class PassengersSelectionViewModel : ViewModel
    {
        public List<Passenger> Passengers { get; set; } = new List<Passenger>(new[]
        {
            #region Passengers List
            new Passenger
            {
                Name = "Just Me",
                Number = 0
            },
            new Passenger
            {
                Name = "One",
                Number = 1
            },
            new Passenger
            {
                Name = "Two",
                Number = 2
            },
            new Passenger
            {
                Name = "Three",
                Number = 3
            },
            new Passenger
            {
                Name = "Four",
                Number = 4
            } 
            #endregion
        });

        private Passenger _selectedPassenger;

        public Passenger SelectedPassenger
        {
            get { return _selectedPassenger; }
            set
            {
                if (_selectedPassenger == value) return;

                _selectedPassenger = value;

                OnPropertyChanged(nameof(SelectedPassenger));
            }
        }

        public PassengersSelectionViewModel()
        {
            SelectCommand = new Command(Select);
        }

        public ICommand SelectCommand { get; set; }

        public event EventHandler<Passenger> Selected = delegate { };

        public event EventHandler NotSelected = delegate { };

        public void Select()
        {
            if (SelectedPassenger != null)
            {
                Selected?.Invoke(this, SelectedPassenger);
            }
            else
            {
                NotSelected?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    internal class Passenger : ViewModel
    {
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

        private int _number;

        public int Number
        {
            get { return _number; }
            set
            {
                if (_number == value) return;
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }
    }
}