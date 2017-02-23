using System;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class VehicleSelectionPage : ContentPage
    {
        public VehicleSelectionPage()
        {
            InitializeComponent();

            (BindingContext as VehiclesViewModel).NoVehicles += VehicleSelectionPage_NoVehicles;
        }

        private void VehicleSelectionPage_NoVehicles(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var choice =
                    await DisplayAlert("No Vehicles Saved", "Would you like to add a new vehicle now?", "Yes", "No");

                if (choice)
                {
                    var addVehiclePage = new VehicleAddPage();
                    (addVehiclePage.BindingContext as VehicleViewModel).VehicleSaved +=
                        VehicleSelectionPage_VehicleSaved;

                    Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(addVehiclePage); });
                }
                else
                {
                    await Navigation.PopAsync();
                }
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            (BindingContext as VehiclesViewModel).Refresh();
        }

        private void VehicleSelectionPage_VehicleSaved(object sender, string e)
        {
            (BindingContext as VehiclesViewModel).Refresh();
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // throw new NotImplementedException();
        }
    }
}