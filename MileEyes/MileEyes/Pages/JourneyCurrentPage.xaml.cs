using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MileEyes.Services;
using MileEyes.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MileEyes.Pages
{
    public partial class JourneyCurrentPage : ContentPage
    {
        public JourneyCurrentPage()
        {
            InitializeComponent();

            Services.Host.TrackerService.HasMoved += TrackerService_HasMoved;

            (BindingContext as JourneyCurrentViewModel).StopRequested += JourneyCurrentPage_StopRequested;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            (BindingContext as JourneyCurrentViewModel).Refresh();
        }

        private async void JourneyCurrentPage_StopRequested(object sender, EventArgs e)
        {
            var response =
                   await
                       DisplayActionSheet("Have you reached your destination?", "Cancel", "Stop & Delete Journey",
                           "Yes, Save Journey");

            switch (response)
            {
                case "Yes, Save Journey":
                    (BindingContext as JourneyCurrentViewModel).EndJourneyConfirmed();
                    break;
                case "Stop & Delete Journey":
                    await Services.Host.TrackerService.Cancel();
                    break;
                case "Cancel":
                    (BindingContext as JourneyCurrentViewModel).Busy = false;
                    break;
            }
        }

        private void TrackerService_HasMoved(object sender, EventArgs e)
        {
            if (Host.Backgrounded) return;

            var pos = Host.TrackerService.CurrentLocation;

            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(pos.Latitude,
                pos.Longitude), Distance.FromMiles(0.25)));

            // LocationIcon.Rotation = Services.Host.TrackerService.CurrentLocation.Heading;
        }
    }
}
