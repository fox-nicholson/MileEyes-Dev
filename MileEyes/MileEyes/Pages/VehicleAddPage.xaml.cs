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
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Vehicle Not Saved", e, "Try Again");
            });
        }

        private void VehicleAddPage_VehicleSaved(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Vehicle Saved", e, "Ok");
                await Navigation.PopAsync();
            });
        }

        private void Cell_OnTapped(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var engineTypeSelectionPage = new EngineTypeSelectionPage();
                (engineTypeSelectionPage.BindingContext as EngineTypesViewModel).Selected += VehicleAddPage_Selected;
                (engineTypeSelectionPage.BindingContext as EngineTypesViewModel).NotSelected += VehicleAddPage_NotSelected;
                await Navigation.PushModalAsync(new CustomNavigationPage(engineTypeSelectionPage)
                {
                    BarBackgroundColor = Color.FromHex("#103D47"),
                    BarTextColor = Color.White
                });
            });
        }

        private void VehicleAddPage_NotSelected(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await
                    DisplayAlert("No Engine Type Selected", "Engine Type was not selected, please select one or go back.",
                        "Ok");
            });
        }

        private void VehicleAddPage_Selected(object sender, EventArgs e)
        {
            var engineType = (sender as EngineTypesViewModel).SelectedEngineType.EngineType;

            (BindingContext as VehicleViewModel).EngineType = engineType;
            (BindingContext as VehicleViewModel).Vehicle.EngineType = engineType;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }
    }
}
