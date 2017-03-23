using System;
using MileEyes.Services.Models;
using Xamarin.Forms;
using MileEyes.ViewModels;

namespace MileEyes.Pages
{
    public partial class AddressSelectionPage : ContentPage
    {
        public AddressSelectionPage()
        {
            InitializeComponent();

            (BindingContext as AddressSelectionViewModel).SearchFailed += AddressSelectionPage_SearchFailed;
        }

        private void AddressSelectionPage_SearchFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () => { await DisplayAlert("Unable to Search", e, "Ok"); });
        }

        public event EventHandler<Address> AddressSelected = delegate { };

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            var address = e.SelectedItem as Address;

            if (string.IsNullOrEmpty(address?.PlaceId))
            {
                (sender as ListView).SelectedItem = null;
                return;
            }

            AddressSelected?.Invoke(this, address);
        }

        private void CancelMenuItem_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            searchBar.Focus();
        }
    }
}