using System;
using Xamarin.Forms;

namespace MileEyes.Pages
{
    public partial class EngineTypeSelectionPage : ContentPage
    {
        public EngineTypeSelectionPage()
        {
            InitializeComponent();

            Services.Host.EngineTypeService.SyncFailed += EngineTypeService_SyncFailed;
        }

        private void EngineTypeService_SyncFailed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(
                async () =>
                {
                    await DisplayAlert("Engine Types", "We had trouble retrieving the list of Engine types", "Ok");
                });
        }
    }
}