using System;
using System.Linq;
using MileEyes.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.Pages
{
    public partial class JourneyPage : ContentPage
    {
        public JourneyPage(JourneyViewModel j)
        {
            InitializeComponent();

            BindingContext = j;

            InitRoute();
        }

        protected override void OnAppearing()
        {
            SegmentedControl.SelectedValue = "Details";
            base.OnAppearing();
        }

        private void InitRoute()
        {
            var j = BindingContext as JourneyViewModel;

            SegmentedControl.SelectedValue = "Details";

            if (j == null) return;
            if (j.Waypoints.Count > 2)
            {
                int midpoint = j.Waypoints.OrderBy(w => w.Step).Count() / 2;
                map.MoveToRegion(
                    MapSpan.FromCenterAndRadius(
                        new Position(j.Waypoints.ElementAt(midpoint).Latitude,
                            j.Waypoints.ElementAt(midpoint).Longitude),
                        Distance.FromMiles(j.Distance * 0.08)));
            }
            else
            {
                var midpoint =
                    Services.Helpers.TrigHelpers.MidPoint(
                        new[] {j.OriginAddress.Latitude, j.OriginAddress.Longitude},
                        new[] {j.DestinationAddress.Latitude, j.DestinationAddress.Longitude});
                map.MoveToRegion(
                    MapSpan.FromCenterAndRadius(
                        new Position(midpoint[0], midpoint[1]),
                        Distance.FromMiles(j.Distance * 0.08)));
            }


            j.InitRoute();
        }

        private void Switch_OnToggled(object sender, ToggledEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => { await Navigation.PushAsync(new PremiumFeaturesPage()); });
        }

        private void SegmentedControl_OnValueChanged(object sender, EventArgs e)
        {
            switch (SegmentedControl.SelectedValue.ToUpper())
            {
                case "DETAILS":
                    (BindingContext as JourneyViewModel).ShowDetails = true;
                    break;
                case "MAP":
                    (BindingContext as JourneyViewModel).ShowDetails = false;
                    break;
                case "":
                    (BindingContext as JourneyViewModel).ShowDetails = true;
                    break;
            }
        }
    }
}