using System;
using MileEyes.Helpers;
using MileEyes.Services;
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
            if ((BindingContext as SettingsViewModel).Busy) return;

            (BindingContext as SettingsViewModel).Busy = true;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new VehiclesPage()); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void ConnectCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as SettingsViewModel).Busy) return;

            (BindingContext as SettingsViewModel).Busy = true;

            if (Host.AuthService.Authenticated)
            {
                Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new CloudPage()); });

                Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new PremiumFeaturesPage()); });

                Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
            }
        }

        private void ReasonsCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as SettingsViewModel).Busy) return;

            (BindingContext as SettingsViewModel).Busy = true;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new ReasonsPage()); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void DefaultReasonCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as SettingsViewModel).Busy) return;

            (BindingContext as SettingsViewModel).Busy = true;

            var selectReasonPage = new ReasonDefaultSelectionPage();
            (selectReasonPage.BindingContext as ReasonsViewModel).DefaultSelected += SettingsPage_Selected;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectReasonPage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void SettingsPage_Selected(object sender, ReasonViewModel e)
        {
            if (string.IsNullOrEmpty(e.Id)) return;

            Host.ReasonService.SetDefault(e.Id);

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        private void DefaultVehicleCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as SettingsViewModel).Busy) return;

            (BindingContext as SettingsViewModel).Busy = true;

            var selectVehiclePage = new VehicleSelectionPage();
            (selectVehiclePage.BindingContext as VehiclesViewModel).Selected += SettingsPage_VehicleSelected;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectVehiclePage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void SettingsPage_VehicleSelected(object sender, VehicleViewModel e)
        {
            if (string.IsNullOrEmpty(e.Id)) return;

            Host.VehicleService.SetDefault(e.Id);

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        private void DefaultCompanyCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as SettingsViewModel).Busy) return;

            (BindingContext as SettingsViewModel).Busy = true;

            var selectCompanyPage = new CompanySelectionPage();
            (selectCompanyPage.BindingContext as CompaniesViewModel).Selected += SettingsPage_CompanySelected;
            ;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectCompanyPage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void SettingsPage_CompanySelected(object sender, CompanyViewModel e)
        {
            if (string.IsNullOrEmpty(e.Id)) return;

            Host.CompanyService.SetDefault(e.Id);

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        private void DefaultPassengersCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as SettingsViewModel).Busy) return;

            (BindingContext as SettingsViewModel).Busy = true;

            var selectPassengersPage = new PassengersSelectionPage();
            (selectPassengersPage.BindingContext as PassengersSelectionViewModel).Selected += SettingsPage_Selected1;
            ;
            ;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectPassengersPage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void SettingsPage_Selected1(object sender, Passenger e)
        {
            Settings.DefaultPassengers = e.Number;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        public bool Wait()
        {
            (BindingContext as SettingsViewModel).Busy = false;
            return false;
        }
    }
}