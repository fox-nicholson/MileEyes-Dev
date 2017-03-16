using System;
using MileEyes.Services;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class JourneyConfirmPage : ContentPage
    {
        public JourneyConfirmPage()
        {
            InitializeComponent();

            (BindingContext as JourneyConfirmViewModel).SaveFailed += JourneyConfirmPage_SaveFailed;
            (BindingContext as JourneyConfirmViewModel).Saved += JourneyConfirmPage_Saved;
        }

        private void JourneyConfirmPage_Saved(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
            TrackerService.IsTracking = false;
        }

        private void JourneyConfirmPage_SaveFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () => { await DisplayAlert("Journey Not Saved", e, "Ok"); });
            TrackerService.IsTracking = false;
        }

        private async void CancelMenuItem_OnClicked(object sender, EventArgs e)
        {
            await Host.TrackerService.Cancel();
        }

        private void VehicleCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as JourneyConfirmViewModel).Busy) return;

            (BindingContext as JourneyConfirmViewModel).Busy = true;

            var selectVehiclePage = new VehicleSelectionPage();
            (selectVehiclePage.BindingContext as VehiclesViewModel).Selected += OnVehicleSelected;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectVehiclePage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void OnVehicleSelected(object sender, VehicleViewModel e)
        {
            (BindingContext as JourneyConfirmViewModel).Vehicle = e.Vehicle;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        private void CompanyCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as JourneyConfirmViewModel).Busy) return;

            (BindingContext as JourneyConfirmViewModel).Busy = true;

            var selectCompanyPage = new CompanySelectionPage();
            (selectCompanyPage.BindingContext as CompaniesViewModel).Selected += JourneyManualPage_CompanySelected;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectCompanyPage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void JourneyManualPage_CompanySelected(object sender, CompanyViewModel e)
        {
            (BindingContext as JourneyConfirmViewModel).Company = e.Company;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        private void PassengersCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as JourneyConfirmViewModel).Busy) return;

            (BindingContext as JourneyConfirmViewModel).Busy = true;

            var selectPassengersPage = new PassengersSelectionPage();
            (selectPassengersPage.BindingContext as PassengersSelectionViewModel).Selected +=
                JourneyManualPage_PassengersSelected;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectPassengersPage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void JourneyManualPage_PassengersSelected(object sender, Passenger e)
        {
            (BindingContext as JourneyConfirmViewModel).Passengers = e.Number;
            (BindingContext as JourneyConfirmViewModel).Passenger = e;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        private void ReasonCell_OnTapped(object sender, EventArgs e)
        {
            if ((BindingContext as JourneyConfirmViewModel).Busy) return;

            (BindingContext as JourneyConfirmViewModel).Busy = true;

            var selectReasonPage = new ReasonSelectionPage();
            (selectReasonPage.BindingContext as ReasonsViewModel).Selected += JourneyManualPage_ReasonSelected;
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(selectReasonPage); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void JourneyManualPage_ReasonSelected(object sender, string e)
        {
            (BindingContext as JourneyConfirmViewModel).Reason = e;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopAsync(); });
        }

        public bool Wait()
        {
            (BindingContext as JourneyConfirmViewModel).Busy = false;
            return false;
        }
    }
}