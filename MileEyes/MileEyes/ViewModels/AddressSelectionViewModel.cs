using System.Collections.ObjectModel;
using System.Windows.Input;
using MileEyes.Services.Models;
using Xamarin.Forms;
using System;

namespace MileEyes.ViewModels
{
    internal class AddressSelectionViewModel : ViewModel
    {
        public ICommand SearchCommand { get; set; }

        private string _searchTerm;

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                if (_searchTerm == value) return;

                _searchTerm = value;

                OnPropertyChanged(nameof(SearchTerm));

                Search();
            }
        }

        public ObservableCollection<Address> Addresses { get; set; }

        private Address _selectedAddress;

        public Address SelectedAddress
        {
            get { return _selectedAddress; }
            set
            {
                if (_selectedAddress == value) return;
                _selectedAddress = value;
                OnPropertyChanged(nameof(SelectedAddress));
            }
        }

        public AddressSelectionViewModel()
        {
            Addresses = new ObservableCollection<Address>();

            SearchCommand = new Command(Search);
        }

        public event EventHandler<string> SearchFailed = delegate { };

        public async void Search()
        {
            Refreshing = true;

            var addresses = await Services.Host.GeocodingService.AddressLookup(_searchTerm);

            Addresses.Clear();

            if (_searchTerm == "") return;

            foreach (var address in addresses)
            {
                if (address.Label == "No Internet") { SearchFailed?.Invoke(this, "Unable to connect to addressing services. \nPlease check your network connection."); return; }
                Addresses.Add(address);
            }

            Refreshing = false;
        }
    }
}