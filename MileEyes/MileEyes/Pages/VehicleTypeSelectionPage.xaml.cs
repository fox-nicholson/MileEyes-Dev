using System;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class VehicleTypeSelectionPage : ContentPage
    {
        public VehicleTypeSelectionPage()
        {
            InitializeComponent();

            Services.Host.VehicleTypeService.SyncFailed += VehicleTypeService_SyncFailed;
        }

        private void VehicleTypeService_SyncFailed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(
                async () =>
                {
                    await DisplayAlert("Vehicle Types", "We had trouble retrieving the list of Vehicle types", "Ok");
                });
        }
    }
}