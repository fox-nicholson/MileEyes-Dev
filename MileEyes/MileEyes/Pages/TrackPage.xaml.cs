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
    public partial class TrackPage : ContentPage
    {
        public static event EventHandler GotoJourneysPage = delegate { };

        public TrackPage()
        {
            InitializeComponent();

            Services.Host.TrackerService.Started += TrackerService_Started;
            Services.Host.TrackerService.StartFailed += TrackerService_StartFailed;
            Services.Host.TrackerService.Stopped += TrackerService_Stopped;
            Services.Host.TrackerService.Cancelled += TrackerService_Cancelled;

            Services.Host.JourneyService.JourneySaved += JourneyService_JourneySaved;
        }

        private void JourneyService_JourneySaved(object sender, EventArgs e)
        {
            GotoJourneysPage?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as TrackViewModel).Refresh();
            (BindingContext as TrackViewModel).GpsPingCallback();
        }

        private void TrackerService_Cancelled(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
            });
        }

        private void TrackerService_Stopped(object sender, Services.Models.Journey e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
                await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyConfirmPage())
                {
                    BarBackgroundColor = Color.FromHex("#103D47"),
                    BarTextColor = Color.White
                });
            });
        }

        private void TrackerService_StartFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Tracking Problem", e, "Ok");
            });
        }

        private void TrackerService_Started(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyCurrentPage())
                {
                    BarBackgroundColor = Color.FromHex("#103D47"),
                    BarTextColor = Color.White
                });
            });
        }

        private void ManualJourneyButton_OnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyManualPage())
                {
                    BarBackgroundColor = Color.FromHex("#103D47"),
                    BarTextColor = Color.White
                });
            });
        }
    }
}
