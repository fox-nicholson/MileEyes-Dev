using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.CustomControls;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            (BindingContext as SettingsViewModel).Refresh();
        }

        private void VehiclesCell_OnTapped(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new VehiclesPage());
            });
        }

        private void ConnectCell_OnTapped(object sender, EventArgs e)
        {
            if (Services.Host.AuthService.Authenticated)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PushAsync(new CloudPage());
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PushAsync(new PremiumFeaturesPage());
                });
            }
        }

        private void ReasonsCell_OnTapped(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new ReasonsPage());
            });
        }

        private void DefaultReasonCell_OnTapped(object sender, EventArgs e)
        {
            var selectReasonPage = new ReasonDefaultSelectionPage();
            (selectReasonPage.BindingContext as ReasonsViewModel).DefaultSelected += SettingsPage_Selected;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectReasonPage);
            });
        }

        private void SettingsPage_Selected(object sender, ReasonViewModel e)
        {
            if (string.IsNullOrEmpty(e.Id)) return;

            Services.Host.ReasonService.SetDefault(e.Id);

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }

        private void DefaultVehicleCell_OnTapped(object sender, EventArgs e)
        {
            var selectVehiclePage = new VehicleSelectionPage();
            (selectVehiclePage.BindingContext as VehiclesViewModel).Selected += SettingsPage_VehicleSelected;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectVehiclePage);
            });
        }

        private void SettingsPage_VehicleSelected(object sender, VehicleViewModel e)
        {
            if (string.IsNullOrEmpty(e.Id)) return;

            Services.Host.VehicleService.SetDefault(e.Id);

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }

        private void DefaultCompanyCell_OnTapped(object sender, EventArgs e)
        {
            var selectCompanyPage = new CompanySelectionPage();
            (selectCompanyPage.BindingContext as CompaniesViewModel).Selected += SettingsPage_CompanySelected; ;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectCompanyPage);
            });
        }

        private void SettingsPage_CompanySelected(object sender, CompanyViewModel e)
        {
            if (string.IsNullOrEmpty(e.Id)) return;

            Services.Host.CompanyService.SetDefault(e.Id);

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }

        private void DefaultPassengersCell_OnTapped(object sender, EventArgs e)
        {
            var selectPassengersPage = new PassengersSelectionPage();
            (selectPassengersPage.BindingContext as PassengersSelectionViewModel).Selected += SettingsPage_Selected1; ; ;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(selectPassengersPage);
            });
        }

        private void SettingsPage_Selected1(object sender, Passenger e)
        {
            Helpers.Settings.DefaultPassengers = e.Number;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
        }
    }
}
