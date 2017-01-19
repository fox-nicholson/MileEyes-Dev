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
    class CompanySelectionViewModel : ViewModel
    {
        public ObservableCollection<Company> Companies { get; set; } = new ObservableCollection<Company>();

        private Company _selectedCompany;

        public Company SelectedCompany
        {
            get { return _selectedCompany; }
            set
            {
                if (_selectedCompany == value) return;

                _selectedCompany = value;

                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        public CompanySelectionViewModel()
        {
            SelectCommand = new Command(Select);
            Refresh();
        }

        public override async void Refresh()
        {
            base.Refresh();

            Refreshing = true;

            Companies.Clear();

            foreach (var c in await Services.Host.CompanyService.GetCompanies())
            {
                Companies.Add(c);
            }

            Refreshing = false;
        }

        public ICommand SelectCommand { get; set; }

        public event EventHandler<Company> Selected = delegate { };

        public event EventHandler NotSelected = delegate { };

        public void Select()
        {
            if (SelectedCompany != null)
            {
                Selected?.Invoke(this, SelectedCompany);
            }
            else
            {
                NotSelected?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
