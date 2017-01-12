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
    class CompaniesViewModel : ViewModel
    {
        public ObservableCollection<Company> Companies { get; set; } = new ObservableCollection<Company>();

        private CompanyViewModel _selectedCompany;

        public CompanyViewModel SelectedCompany
        {
            get { return _selectedCompany; }
            set
            {
                if (_selectedCompany == value) return;

                _selectedCompany = value;

                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        public CompaniesViewModel()
        {
            SelectCommand = new Command(Select);
            Refresh();
        }

        public async void Refresh()
        {
            Refreshing = true;

            Companies.Clear();

            foreach (var c in await Services.Host.CompanyService.GetCompanies())
            {
                Companies.Add(c);
            }

            Refreshing = false;
        }

        public ICommand SelectCommand { get; set; }

        public event EventHandler<CompanyViewModel> Selected = delegate { };

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
