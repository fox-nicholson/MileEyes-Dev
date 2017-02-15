using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services.Models;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class AddressSelectionPage : ContentPage
    {
        public AddressSelectionPage()
        {
            InitializeComponent();
        }

        public event EventHandler<Address> AddressSelected = delegate { };

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            
            var address = (e.SelectedItem as Address);

            if (string.IsNullOrEmpty(address?.PlaceId))
            {
                (sender as ListView).SelectedItem = null;
                return;
            }

            AddressSelected?.Invoke(this, address);
        }

        private void CancelMenuItem_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            searchBar.Focus();
        }
    }
}
