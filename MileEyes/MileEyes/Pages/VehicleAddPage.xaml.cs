using System;
using MileEyes.CustomControls;
using MileEyes.ViewModels;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class VehicleAddPage : ContentPage
    {
        public VehicleAddPage()
        {
            InitializeComponent();

            (BindingContext as VehicleViewModel).VehicleSaved += VehicleAddPage_VehicleSaved;
            (BindingContext as VehicleViewModel).VehicleNotSaved += VehicleAddPage_VehicleNotSaved;
        }

        private void VehicleAddPage_VehicleNotSaved(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () => { await DisplayAlert("Vehicle Not Saved", e, "Try Again"); });
        }

        private void VehicleAddPage_VehicleSaved(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Vehicle Saved", e, "Ok");
                await Navigation.PopAsync();
            });
        }

        private void VehicleTypeCell_OnTapped(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var vehicleTypeSelectionPage = new VehicleTypeSelectionPage();
                (vehicleTypeSelectionPage.BindingContext as VehicleTypesViewModel).Selected += VehicleTypeAddPage_Selected;
                (vehicleTypeSelectionPage.BindingContext as VehicleTypesViewModel).NotSelected += VehicleTypeAddPage_NotSelected;
                Device.OnPlatform(
                    async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(vehicleTypeSelectionPage)
                        {
                            BarBackgroundColor = Color.FromHex("#103D47"),
                            BarTextColor = Color.White
                        });
                    }, async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(vehicleTypeSelectionPage)
                        {
                            BarBackgroundColor = Color.FromHex("#58C0EE"),
                            BarTextColor = Color.White
                        });
                    });
            });
        }

        private void EngineTypeCell_OnTapped(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var engineTypeSelectionPage = new EngineTypeSelectionPage();
                (engineTypeSelectionPage.BindingContext as EngineTypesViewModel).Selected += VehicleEngineAddPage_Selected;
                (engineTypeSelectionPage.BindingContext as EngineTypesViewModel).NotSelected +=
                    VehicleEngineAddPage_NotSelected;
                Device.OnPlatform(
                    async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(engineTypeSelectionPage)
                        {
                            BarBackgroundColor = Color.FromHex("#103D47"),
                            BarTextColor = Color.White
                        });
                    }, async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(engineTypeSelectionPage)
                        {
                            BarBackgroundColor = Color.FromHex("#58C0EE"),
                            BarTextColor = Color.White
                        });
                    });
            });
        }

        private void VehicleEngineAddPage_NotSelected(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await
                    DisplayAlert("No Engine Type Selected",
                        "Engine Type was not selected, please select one or go back.",
                        "Ok");
            });
        }

        private void VehicleTypeAddPage_NotSelected(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await
                    DisplayAlert("No Vehicle Type Selected",
                        "Vehicle Type was not selected, please select one or go back.",
                        "Ok");
            });
        }

        private void VehicleEngineAddPage_Selected(object sender, EventArgs e)
        {
            var engineType = (sender as EngineTypesViewModel).SelectedEngineType.EngineType;

            (BindingContext as VehicleViewModel).EngineType = engineType;
            (BindingContext as VehicleViewModel).Vehicle.EngineType = engineType;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
        }

        private void VehicleTypeAddPage_Selected(object sender, EventArgs e)
        {
            var vehicleType = (sender as VehicleTypesViewModel).SelectedVehicleType.VehicleType;

            (BindingContext as VehicleViewModel).VehicleType = vehicleType;
            (BindingContext as VehicleViewModel).Vehicle.VehicleType = vehicleType;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
        }
    }
}