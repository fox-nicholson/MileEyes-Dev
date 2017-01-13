using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.CustomControls;
using MileEyes.Services.Models;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class JourneyManualPage : ContentPage
    {
        public JourneyManualPage()
        {
            InitializeComponent();

            (BindingContext as JourneyManualViewModel).Cancelled += JourneyManualPage_Cancelled;
            (BindingContext as JourneyManualViewModel).SaveFailed += JourneyManualPage_SaveFailed;
            (BindingContext as JourneyManualViewModel).Saved += JourneyManualPage_Saved;
        }

        private async void JourneyManualPage_Cancelled(object sender, EventArgs e)
        {
            var response =
                      await
                          DisplayActionSheet("Are you sure you wish to delete this Journey?", "Cancel", "Delete Journey");

            switch (response)
            {
                case "Delete Journey":
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopModalAsync();
                    });
                    break;
                case "Cancel":
                    break;
            }
        }

        private void JourneyManualPage_Saved(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }

        private void JourneyManualPage_SaveFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Journey Not Saved", e, "Ok");
            });
        }

        private void OriginCell_OnTapped(object sender, EventArgs e)
        {
            var selectOriginPage = new AddressSelectionPage();
            selectOriginPage.AddressSelected += JourneyManualPage_OriginAddressSelected;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushModalAsync(new CustomNavigationPage(selectOriginPage)
                {
                    BarBackgroundColor = Color.FromHex("#103D47"),
                    BarTextColor = Color.White
                });
            });
        }

        private void JourneyManualPage_OriginAddressSelected(object sender, Address e)
        {
            (BindingContext as JourneyManualViewModel).OriginAddress = e;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }

        private void DestinationCell_OnTapped(object sender, EventArgs e)
        {
            var selectDestinationPage = new AddressSelectionPage();
            selectDestinationPage.AddressSelected += JourneyManualPage_DestinationAddressSelected;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushModalAsync(new CustomNavigationPage(selectDestinationPage)
                {
                    BarBackgroundColor = Color.FromHex("#103D47"),
                    BarTextColor = Color.White
                });
            });
        }

        private void JourneyManualPage_DestinationAddressSelected(object sender, Address e)
        {
            (BindingContext as JourneyManualViewModel).DestinationAddress = e;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }

        private void VehicleCell_OnTapped(object sender, EventArgs e)
        {
            var selectVehiclePage = new VehicleSelectionPage();
            (selectVehiclePage.BindingContext as VehiclesViewModel).Selected += OnVehicleSelected;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectVehiclePage);
            });
        }

        private void OnVehicleSelected(object sender, VehicleViewModel e)
        {
            (BindingContext as JourneyManualViewModel).Vehicle = e.Vehicle;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }

        private void CompanyCell_OnTapped(object sender, EventArgs e)
        {
            var selectCompanyPage = new CompanySelectionPage();
            (selectCompanyPage.BindingContext as CompaniesViewModel).Selected += JourneyManualPage_CompanySelected;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectCompanyPage);
            });
        }

        private void JourneyManualPage_CompanySelected(object sender, CompanyViewModel e)
        {
            (BindingContext as JourneyManualViewModel).Company = e.Company;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }

        private void PassengersCell_OnTapped(object sender, EventArgs e)
        {
            var selectPassengersPage = new PassengersSelectionPage();
            (selectPassengersPage.BindingContext as PassengersSelectionViewModel).Selected += JourneyManualPage_PassengersSelected;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectPassengersPage);
            });
        }

        private void JourneyManualPage_PassengersSelected(object sender, Passenger e)
        {
            (BindingContext as JourneyManualViewModel).Passengers = e.Number;
            (BindingContext as JourneyManualViewModel).Passenger = e;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }

        private void ReasonCell_OnTapped(object sender, EventArgs e)
        {
            var selectReasonPage = new ReasonSelectionPage();
            (selectReasonPage.BindingContext as ReasonsViewModel).Selected += JourneyManualPage_ReasonSelected;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectReasonPage);
            });
        }

        private void JourneyManualPage_ReasonSelected(object sender, string e)
        {
            (BindingContext as JourneyManualViewModel).Reason = e;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }
    }
}
