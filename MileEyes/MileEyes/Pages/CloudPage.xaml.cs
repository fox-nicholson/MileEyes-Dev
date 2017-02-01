using System;
using MileEyes.Services.Models;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class CloudPage : ContentPage
    {
        public CloudPage()
        {
            InitializeComponent();

            (BindingContext as CloudViewModel).LoggedOut += CloudPage_LoggedOut;
        }

        private void CloudPage_LoggedOut(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopToRootAsync(); });
        }

        private void AddressCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as CloudViewModel).Busy) return;

            (BindingContext as CloudViewModel).Busy = true;

            var selectAddressPage = new CloudAddressSelectionPage();
            selectAddressPage.AddressSelected += SelectAddressPage_AddressSelected;
            ;

            Device.BeginInvokeOnMainThread(
                async () => { await Navigation.PushModalAsync(new NavigationPage(selectAddressPage)); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void SelectAddressPage_AddressSelected(object sender, Address e)
        {
            (BindingContext as CloudViewModel).Address = e;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
        }

        public bool Wait()
        {
            (BindingContext as CloudViewModel).Busy = false;
            return false;
        }
    }
}