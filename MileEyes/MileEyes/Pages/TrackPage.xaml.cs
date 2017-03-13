using System;
using MileEyes.CustomControls;
using MileEyes.Services;
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

            Host.TrackerService.Started += TrackerService_Started;
            Host.TrackerService.StartFailed += TrackerService_StartFailed;
            Host.TrackerService.Stopped += TrackerService_Stopped;
            Host.TrackerService.Cancelled += TrackerService_Cancelled;

            Host.JourneyService.JourneySaved += JourneyService_JourneySaved;
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
            if ((BindingContext as TrackViewModel).Busy) return;

            (BindingContext as TrackViewModel).Busy = true;

            Device.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private void TrackerService_Stopped(object sender, Services.Models.Journey e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopModalAsync();
                Device.OnPlatform(
                    async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyConfirmPage())
                        {
                            BarBackgroundColor = Color.FromHex("#103D47"),
                            BarTextColor = Color.White
                        });
                    }, async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyConfirmPage())
                        {
                            BarBackgroundColor = Color.FromHex("#58C0EE"),
                            BarTextColor = Color.White
                        });
                    });
            });
        }

        private void TrackerService_StartFailed(object sender, string e)
        {
            Device.BeginInvokeOnMainThread(async () => { await DisplayAlert("Tracking Problem", e, "Ok"); });
        }

        private void TrackerService_Started(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Device.OnPlatform(
                    async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyCurrentPage())
                        {
                            BarBackgroundColor = Color.FromHex("#103D47"),
                            BarTextColor = Color.White
                        });
                    }, async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyCurrentPage())
                        {
                            BarBackgroundColor = Color.FromHex("#58C0EE"),
                            BarTextColor = Color.White
                        });
                    });
            });
        }

        private void ManualJourneyButton_OnClicked(object sender, EventArgs e)
        {
            if ((BindingContext as TrackViewModel).Busy) return;

            (BindingContext as TrackViewModel).Busy = true;

            Device.BeginInvokeOnMainThread(() =>
            {
                Device.OnPlatform(
                    async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyManualPage())
                        {
                            BarBackgroundColor = Color.FromHex("#103D47"),
                            BarTextColor = Color.White
                        });
                    }, async () =>
                    {
                        await Navigation.PushModalAsync(new CustomNavigationPage(new JourneyManualPage())
                        {
                            BarBackgroundColor = Color.FromHex("#58C0EE"),
                            BarTextColor = Color.White
                        });
                    });
            });

            Device.StartTimer(TimeSpan.FromSeconds(1), Wait);
        }

        private bool Wait()
        {
            (BindingContext as TrackViewModel).Busy = false;
            return false;
        }
    }
}